using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using BlindMatchPAS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public sealed class AdminMatchOverrideService : IAdminMatchOverrideService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminMatchOverrideService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ServiceResult<MatchInterventionViewModel>> GetInterventionModelAsync(
        int proposalId,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _db.ProjectProposals.AsNoTracking()
            .Include(p => p.ResearchArea)
            .Include(p => p.MatchRecord)
            .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

        if (proposal is null)
            return ServiceResult<MatchInterventionViewModel>.Fail("Proposal was not found.");

        var student = await _db.Users.AsNoTracking()
            .FirstAsync(u => u.Id == proposal.StudentId, cancellationToken);

        string? supervisorEmail = null;
        if (proposal.MatchRecord is not null)
        {
            var sv = await _db.Users.AsNoTracking()
                .FirstAsync(u => u.Id == proposal.MatchRecord.SupervisorId, cancellationToken);
            supervisorEmail = sv.Email ?? sv.UserName;
        }

        var picks = await EligibleSupervisorPickItemsAsync(proposal.ResearchAreaId, cancellationToken);

        var vm = new MatchInterventionViewModel
        {
            ProposalId = proposal.Id,
            Title = proposal.Title,
            ResearchAreaName = proposal.ResearchArea.Name,
            StudentEmail = student.Email ?? student.UserName ?? "",
            Status = proposal.Status,
            HasMatch = proposal.MatchRecord is not null,
            CurrentSupervisorEmail = supervisorEmail,
            MatchedAt = proposal.MatchRecord?.MatchedAt,
            EligibleSupervisors = picks
        };

        return ServiceResult<MatchInterventionViewModel>.Ok(vm);
    }

    public async Task<ServiceResult> ClearMatchAsync(int proposalId, CancellationToken cancellationToken = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        ServiceResult? outcome = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

            var proposal = await _db.ProjectProposals
                .Include(p => p.MatchRecord)
                .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

            if (proposal is null)
            {
                outcome = ServiceResult.Fail("Proposal was not found.");
                return;
            }

            if (proposal.MatchRecord is null)
            {
                outcome = ServiceResult.Fail("This proposal has no match to clear.");
                return;
            }

            _db.MatchRecords.Remove(proposal.MatchRecord);
            proposal.Status = ProposalStatus.Submitted;
            proposal.IdentitiesRevealed = false;
            proposal.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            outcome = ServiceResult.Ok();
        });

        return outcome ?? ServiceResult.Fail("Operation did not complete.");
    }

    public async Task<ServiceResult> ReassignSupervisorAsync(
        int proposalId,
        string newSupervisorId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newSupervisorId))
            return ServiceResult.Fail("Select a supervisor.");

        var strategy = _db.Database.CreateExecutionStrategy();
        ServiceResult? outcome = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

            var proposal = await _db.ProjectProposals
                .Include(p => p.MatchRecord)
                .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

            if (proposal is null)
            {
                outcome = ServiceResult.Fail("Proposal was not found.");
                return;
            }

            if (proposal.MatchRecord is null)
            {
                outcome = ServiceResult.Fail("This proposal is not matched. Use force match instead.");
                return;
            }

            var user = await _userManager.FindByIdAsync(newSupervisorId);
            if (user is null || !await _userManager.IsInRoleAsync(user, RoleNames.Supervisor))
            {
                outcome = ServiceResult.Fail("Invalid supervisor.");
                return;
            }

            var allowed = await _db.SupervisorResearchAreas.AnyAsync(
                x => x.SupervisorId == newSupervisorId && x.ResearchAreaId == proposal.ResearchAreaId,
                cancellationToken);

            if (!allowed)
            {
                outcome = ServiceResult.Fail("That supervisor is not assigned to this proposal's research area.");
                return;
            }

            var now = DateTimeOffset.UtcNow;
            proposal.MatchRecord.SupervisorId = newSupervisorId;
            proposal.MatchRecord.MatchedAt = now;
            proposal.IdentitiesRevealed = true;
            proposal.UpdatedAt = now;

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            outcome = ServiceResult.Ok();
        });

        return outcome ?? ServiceResult.Fail("Operation did not complete.");
    }

    public async Task<ServiceResult> ForceMatchAsync(
        int proposalId,
        string supervisorId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(supervisorId))
            return ServiceResult.Fail("Select a supervisor.");

        var strategy = _db.Database.CreateExecutionStrategy();
        ServiceResult? outcome = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

            var proposal = await _db.ProjectProposals
                .Include(p => p.MatchRecord)
                .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

            if (proposal is null)
            {
                outcome = ServiceResult.Fail("Proposal was not found.");
                return;
            }

            if (proposal.Status != ProposalStatus.Submitted)
            {
                outcome = ServiceResult.Fail("Only submitted proposals can be force-matched.");
                return;
            }

            if (proposal.MatchRecord is not null)
            {
                outcome = ServiceResult.Fail("This proposal already has a match. Clear or reassign instead.");
                return;
            }

            var user = await _userManager.FindByIdAsync(supervisorId);
            if (user is null || !await _userManager.IsInRoleAsync(user, RoleNames.Supervisor))
            {
                outcome = ServiceResult.Fail("Invalid supervisor.");
                return;
            }

            var allowed = await _db.SupervisorResearchAreas.AnyAsync(
                x => x.SupervisorId == supervisorId && x.ResearchAreaId == proposal.ResearchAreaId,
                cancellationToken);

            if (!allowed)
            {
                outcome = ServiceResult.Fail("That supervisor is not assigned to this proposal's research area.");
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

            outcome = ServiceResult.Ok();
        });

        return outcome ?? ServiceResult.Fail("Operation did not complete.");
    }

    private async Task<IList<SupervisorPickItem>> EligibleSupervisorPickItemsAsync(
        int researchAreaId,
        CancellationToken cancellationToken)
    {
        var supervisorIds = await _db.SupervisorResearchAreas.AsNoTracking()
            .Where(x => x.ResearchAreaId == researchAreaId)
            .Select(x => x.SupervisorId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (supervisorIds.Count == 0)
            return Array.Empty<SupervisorPickItem>();

        var users = await _db.Users.AsNoTracking()
            .Where(u => supervisorIds.Contains(u.Id))
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return users.Select(u => new SupervisorPickItem
        {
            Id = u.Id,
            Label = string.IsNullOrWhiteSpace(u.DisplayName)
                ? (u.Email ?? u.UserName ?? u.Id)
                : $"{u.DisplayName} ({u.Email ?? u.UserName})"
        }).ToList();
    }
}
