using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Dtos;
using BlindMatchPAS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public sealed class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _db;

    public MatchingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AnonymousProposalDto>> ListAnonymousProposalsAsync(
        string supervisorId,
        int? researchAreaIdFilter,
        CancellationToken cancellationToken = default)
    {
        var areaIds = await SupervisorResearchAreaIdsAsync(supervisorId, cancellationToken);
        if (areaIds.Count == 0)
            return Array.Empty<AnonymousProposalDto>();

        var query = _db.ProjectProposals.AsNoTracking()
            .Where(p => p.Status == ProposalStatus.Submitted && areaIds.Contains(p.ResearchAreaId));

        if (researchAreaIdFilter is { } rid)
            query = query.Where(p => p.ResearchAreaId == rid && areaIds.Contains(rid));

        return await query
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => new AnonymousProposalDto
            {
                ProposalId = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechStack = p.TechStack,
                ResearchAreaId = p.ResearchAreaId,
                ResearchAreaName = p.ResearchArea.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<AnonymousProposalDto>> GetAnonymousProposalForSupervisorAsync(
        string supervisorId,
        int proposalId,
        CancellationToken cancellationToken = default)
    {
        var areaIds = await SupervisorResearchAreaIdsAsync(supervisorId, cancellationToken);
        if (areaIds.Count == 0)
            return ServiceResult<AnonymousProposalDto>.Fail("You have no research areas assigned.");

        // Project to anonymous fields only — never load StudentId, email, or name (blind matching).
        var dto = await _db.ProjectProposals.AsNoTracking()
            .Where(p => p.Id == proposalId
                        && p.Status == ProposalStatus.Submitted
                        && areaIds.Contains(p.ResearchAreaId))
            .Select(p => new AnonymousProposalDto
            {
                ProposalId = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechStack = p.TechStack,
                ResearchAreaId = p.ResearchAreaId,
                ResearchAreaName = p.ResearchArea.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return ServiceResult<AnonymousProposalDto>.Fail("Proposal is not available for anonymous review.");

        return ServiceResult<AnonymousProposalDto>.Ok(dto);
    }

    public async Task<ServiceResult<MatchRecord>> ConfirmMatchAsync(
        string supervisorId,
        int proposalId,
        CancellationToken cancellationToken = default)
    {
        // SqlServerRetryingExecutionStrategy requires user transactions to run inside CreateExecutionStrategy().
        var strategy = _db.Database.CreateExecutionStrategy();
        ServiceResult<MatchRecord>? outcome = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

            var proposal = await _db.ProjectProposals
                .Include(p => p.MatchRecord)
                .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

            if (proposal is null)
            {
                outcome = ServiceResult<MatchRecord>.Fail("Proposal was not found.");
                return;
            }

            if (proposal.Status != ProposalStatus.Submitted)
            {
                outcome = ServiceResult<MatchRecord>.Fail("Only submitted proposals can be matched.");
                return;
            }

            if (proposal.MatchRecord is not null)
            {
                outcome = ServiceResult<MatchRecord>.Fail("This proposal is already matched.");
                return;
            }

            var allowed = await _db.SupervisorResearchAreas.AnyAsync(
                x => x.SupervisorId == supervisorId && x.ResearchAreaId == proposal.ResearchAreaId,
                cancellationToken);

            if (!allowed)
            {
                outcome = ServiceResult<MatchRecord>.Fail("You cannot confirm a match outside your research areas.");
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var match = new MatchRecord
            {
                ProposalId = proposal.Id,
                SupervisorId = supervisorId,
                MatchedAt = now
            };

            proposal.Status = ProposalStatus.Matched;
            proposal.IdentitiesRevealed = true;
            proposal.UpdatedAt = now;

            _db.MatchRecords.Add(match);
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            await _db.Entry(match).Reference(m => m.ProjectProposal).LoadAsync(cancellationToken);
            await _db.Entry(match).Reference(m => m.Supervisor).LoadAsync(cancellationToken);

            outcome = ServiceResult<MatchRecord>.Ok(match);
        });

        return outcome ?? ServiceResult<MatchRecord>.Fail("Operation did not complete.");
    }

    public async Task<ServiceResult<MatchedProposalForSupervisorDto>> GetMatchedProposalForSupervisorAsync(
        string supervisorId,
        int proposalId,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.ProjectProposals.AsNoTracking()
            .Where(p => p.Id == proposalId
                        && p.MatchRecord != null
                        && p.MatchRecord.SupervisorId == supervisorId
                        && p.IdentitiesRevealed)
            .Select(p => new MatchedProposalForSupervisorDto
            {
                ProposalId = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechStack = p.TechStack,
                ResearchAreaName = p.ResearchArea.Name,
                StudentId = p.StudentId,
                StudentEmail = p.Student.Email ?? string.Empty,
                StudentDisplayName = p.Student.DisplayName,
                MatchedAt = p.MatchRecord!.MatchedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return ServiceResult<MatchedProposalForSupervisorDto>.Fail("Matched proposal was not found or identities are not revealed.");

        return ServiceResult<MatchedProposalForSupervisorDto>.Ok(row);
    }

    public async Task<AdminMatchesDashboardDto> GetAdminMatchesDashboardAsync(CancellationToken cancellationToken = default)
    {
        // Explicit joins — reliable translation; navigations on MatchRecord alone can misbehave in some providers.
        var confirmed = await (
            from m in _db.MatchRecords.AsNoTracking()
            join p in _db.ProjectProposals.AsNoTracking() on m.ProposalId equals p.Id
            join ra in _db.ResearchAreas.AsNoTracking() on p.ResearchAreaId equals ra.Id
            join st in _db.Users on p.StudentId equals st.Id
            join sv in _db.Users on m.SupervisorId equals sv.Id
            orderby m.MatchedAt descending
            select new AdminMatchSummaryDto
            {
                ProposalId = p.Id,
                ProposalTitle = p.Title,
                StudentEmail = st.Email ?? string.Empty,
                SupervisorEmail = sv.Email ?? string.Empty,
                ResearchAreaName = ra.Name,
                MatchedAt = m.MatchedAt
            }).ToListAsync(cancellationToken);

        var pending = await (
            from p in _db.ProjectProposals.AsNoTracking()
            join ra in _db.ResearchAreas.AsNoTracking() on p.ResearchAreaId equals ra.Id
            join st in _db.Users on p.StudentId equals st.Id
            where p.Status == ProposalStatus.Submitted
                  && !_db.MatchRecords.Any(m => m.ProposalId == p.Id)
            orderby p.UpdatedAt descending
            select new AdminPendingProposalDto
            {
                ProposalId = p.Id,
                Title = p.Title,
                StudentEmail = st.Email ?? string.Empty,
                ResearchAreaName = ra.Name,
                LastUpdated = p.UpdatedAt
            }).ToListAsync(cancellationToken);

        return new AdminMatchesDashboardDto
        {
            ConfirmedMatches = confirmed,
            SubmittedAwaitingMatch = pending
        };
    }

    private async Task<HashSet<int>> SupervisorResearchAreaIdsAsync(string supervisorId, CancellationToken cancellationToken)
    {
        var ids = await _db.SupervisorResearchAreas.AsNoTracking()
            .Where(x => x.SupervisorId == supervisorId)
            .Select(x => x.ResearchAreaId)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }
}
