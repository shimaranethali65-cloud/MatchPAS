using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public sealed class ResearchAreaService : IResearchAreaService
{
    private readonly ApplicationDbContext _db;

    public ResearchAreaService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ResearchArea>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ResearchAreas.AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResearchArea>> GetForSupervisorAsync(string supervisorId, CancellationToken cancellationToken = default)
    {
        return await _db.SupervisorResearchAreas.AsNoTracking()
            .Where(x => x.SupervisorId == supervisorId)
            .Select(x => x.ResearchArea)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResearchArea?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.ResearchAreas.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ServiceResult<ResearchArea>> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        var trimmed = name.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return ServiceResult<ResearchArea>.Fail("Name is required.");

        var entity = new ResearchArea
        {
            Name = trimmed,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
        };

        _db.ResearchAreas.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult<ResearchArea>.Ok(entity);
    }

    public async Task<ServiceResult> UpdateAsync(int id, string name, string? description, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ResearchAreas.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity is null)
            return ServiceResult.Fail("Research area was not found.");

        var trimmed = name.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return ServiceResult.Fail("Name is required.");

        entity.Name = trimmed;
        entity.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ResearchAreas.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity is null)
            return ServiceResult.Fail("Research area was not found.");

        var hasProposals = await _db.ProjectProposals.AnyAsync(p => p.ResearchAreaId == id, cancellationToken);
        if (hasProposals)
            return ServiceResult.Fail("Cannot delete a research area that has proposals.");

        var hasSupervisorLinks = await _db.SupervisorResearchAreas.AnyAsync(s => s.ResearchAreaId == id, cancellationToken);
        if (hasSupervisorLinks)
            return ServiceResult.Fail("Cannot delete a research area that is assigned to supervisors.");

        _db.ResearchAreas.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }
}
