using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using BlindMatchPAS.Services.Requests;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public sealed class ProposalService : IProposalService
{
    private readonly ApplicationDbContext _db;

    public ProposalService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectProposal>> ListForStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _db.ProjectProposals.AsNoTracking()
            .Include(p => p.ResearchArea)
            .Include(p => p.MatchRecord)
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectProposal?> GetOwnedProposalAsync(int proposalId, string studentId, CancellationToken cancellationToken = default)
    {
        return await _db.ProjectProposals
            .Include(p => p.ResearchArea)
            .Include(p => p.MatchRecord)
                .ThenInclude(m => m!.Supervisor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.StudentId == studentId, cancellationToken);
    }

    public async Task<ServiceResult<ProjectProposal>> CreateAsync(
        string studentId,
        CreateProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        var areaExists = await _db.ResearchAreas.AnyAsync(r => r.Id == request.ResearchAreaId, cancellationToken);
        if (!areaExists)
            return ServiceResult<ProjectProposal>.Fail("Research area is not valid.");

        var now = DateTimeOffset.UtcNow;
        var entity = new ProjectProposal
        {
            StudentId = studentId,
            ResearchAreaId = request.ResearchAreaId,
            Title = request.Title.Trim(),
            Abstract = request.Abstract.Trim(),
            TechStack = request.TechStack.Trim(),
            Status = ProposalStatus.Draft,
            IdentitiesRevealed = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.ProjectProposals.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(entity).Reference(p => p.ResearchArea).LoadAsync(cancellationToken);
        return ServiceResult<ProjectProposal>.Ok(entity);
    }

    public async Task<ServiceResult<ProjectProposal>> UpdateAsync(
        int proposalId,
        string studentId,
        UpdateProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _db.ProjectProposals
            .Include(p => p.MatchRecord)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.StudentId == studentId, cancellationToken);

        if (proposal is null)
            return ServiceResult<ProjectProposal>.Fail("Proposal was not found.");

        if (proposal.Status is ProposalStatus.Matched or ProposalStatus.Withdrawn)
            return ServiceResult<ProjectProposal>.Fail("This proposal can no longer be edited.");

        if (proposal.MatchRecord is not null)
            return ServiceResult<ProjectProposal>.Fail("This proposal is already matched.");

        var areaExists = await _db.ResearchAreas.AnyAsync(r => r.Id == request.ResearchAreaId, cancellationToken);
        if (!areaExists)
            return ServiceResult<ProjectProposal>.Fail("Research area is not valid.");

        proposal.Title = request.Title.Trim();
        proposal.Abstract = request.Abstract.Trim();
        proposal.TechStack = request.TechStack.Trim();
        proposal.ResearchAreaId = request.ResearchAreaId;
        proposal.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(proposal).Reference(p => p.ResearchArea).LoadAsync(cancellationToken);
        return ServiceResult<ProjectProposal>.Ok(proposal);
    }

    public async Task<ServiceResult> WithdrawAsync(int proposalId, string studentId, CancellationToken cancellationToken = default)
    {
        var proposal = await _db.ProjectProposals
            .Include(p => p.MatchRecord)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.StudentId == studentId, cancellationToken);

        if (proposal is null)
            return ServiceResult.Fail("Proposal was not found.");

        if (proposal.MatchRecord is not null || proposal.Status == ProposalStatus.Matched)
            return ServiceResult.Fail("A matched proposal cannot be withdrawn.");

        if (proposal.Status == ProposalStatus.Withdrawn)
            return ServiceResult.Fail("Proposal is already withdrawn.");

        proposal.Status = ProposalStatus.Withdrawn;
        proposal.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SubmitAsync(int proposalId, string studentId, CancellationToken cancellationToken = default)
    {
        var proposal = await _db.ProjectProposals
            .Include(p => p.MatchRecord)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.StudentId == studentId, cancellationToken);

        if (proposal is null)
            return ServiceResult.Fail("Proposal was not found.");

        if (proposal.MatchRecord is not null || proposal.Status == ProposalStatus.Matched)
            return ServiceResult.Fail("A matched proposal cannot be resubmitted.");

        if (proposal.Status == ProposalStatus.Withdrawn)
            return ServiceResult.Fail("A withdrawn proposal cannot be submitted.");

        if (proposal.Status != ProposalStatus.Draft)
            return ServiceResult.Fail("Only draft proposals can be submitted.");

        proposal.Status = ProposalStatus.Submitted;
        proposal.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }
}
