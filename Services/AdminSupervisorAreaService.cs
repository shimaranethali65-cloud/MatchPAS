using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using BlindMatchPAS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public sealed class AdminSupervisorAreaService : IAdminSupervisorAreaService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminSupervisorAreaService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<SupervisorListItemViewModel>> GetSupervisorsWithAreaCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(RoleNames.Supervisor);
        var list = new List<SupervisorListItemViewModel>();

        foreach (var u in users.OrderBy(x => x.Email))
        {
            var count = await _db.SupervisorResearchAreas.AsNoTracking()
                .CountAsync(x => x.SupervisorId == u.Id, cancellationToken);

            list.Add(new SupervisorListItemViewModel
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "",
                DisplayName = u.DisplayName,
                AreaCount = count
            });
        }

        return list;
    }

    public async Task<SupervisorAreasEditViewModel?> GetEditModelAsync(string supervisorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(supervisorId);
        if (user is null || !await _userManager.IsInRoleAsync(user, RoleNames.Supervisor))
            return null;

        var allAreas = await _db.ResearchAreas.AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var assignedIds = await _db.SupervisorResearchAreas.AsNoTracking()
            .Where(x => x.SupervisorId == supervisorId)
            .Select(x => x.ResearchAreaId)
            .ToListAsync(cancellationToken);

        var assigned = assignedIds.ToHashSet();

        return new SupervisorAreasEditViewModel
        {
            SupervisorId = user.Id,
            Email = user.Email ?? user.UserName ?? "",
            DisplayName = user.DisplayName,
            Areas = allAreas.Select(a => new ResearchAreaAssignmentItem
            {
                ResearchAreaId = a.Id,
                Name = a.Name,
                IsSelected = assigned.Contains(a.Id)
            }).ToList()
        };
    }

    public async Task<ServiceResult> SetSupervisorResearchAreasAsync(
        string supervisorId,
        IReadOnlyList<int> researchAreaIds,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(supervisorId);
        if (user is null)
            return ServiceResult.Fail("User was not found.");

        if (!await _userManager.IsInRoleAsync(user, RoleNames.Supervisor))
            return ServiceResult.Fail("That user is not a supervisor.");

        var distinctIds = researchAreaIds.Distinct().ToList();
        if (distinctIds.Count > 0)
        {
            var valid = await _db.ResearchAreas.AsNoTracking()
                .Where(r => distinctIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (valid.Count != distinctIds.Count)
                return ServiceResult.Fail("One or more research areas are invalid.");
        }

        var existing = await _db.SupervisorResearchAreas
            .Where(x => x.SupervisorId == supervisorId)
            .ToListAsync(cancellationToken);

        _db.SupervisorResearchAreas.RemoveRange(existing);

        foreach (var areaId in distinctIds)
        {
            _db.SupervisorResearchAreas.Add(new SupervisorResearchArea
            {
                SupervisorId = supervisorId,
                ResearchAreaId = areaId
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Ok();
    }
}
