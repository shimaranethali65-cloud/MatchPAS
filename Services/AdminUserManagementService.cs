using BlindMatchPAS.Models;
using BlindMatchPAS.Services.Interfaces;
using BlindMatchPAS.Services.Requests;
using BlindMatchPAS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services;

public sealed class AdminUserManagementService : IAdminUserManagementService
{
    private static readonly string[] AllAppRoles =
    [
        RoleNames.Student,
        RoleNames.Supervisor,
        RoleNames.Admin
    ];

    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserManagementService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<AdminUserRowViewModel>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var rows = new List<AdminUserRowViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);

            rows.Add(new AdminUserRowViewModel
            {
                Id = u.Id,
                Email = u.Email ?? u.UserName ?? "",
                DisplayName = u.DisplayName,
                RolesSummary = roles.Count == 0 ? "—" : string.Join(", ", roles.OrderBy(r => r))
            });
        }

        return rows;
    }

    public async Task<ServiceResult<ApplicationUser>> CreateUserAsync(
        CreateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.IsStudent && !request.IsSupervisor && !request.IsAdmin)
            return ServiceResult<ApplicationUser>.Fail("Select at least one role.");

        var email = request.Email.Trim();
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return ServiceResult<ApplicationUser>.Fail("A user with that email already exists.");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = request.DisplayName.Trim()
        };

        var create = await _userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            var msg = string.Join("; ", create.Errors.Select(e => e.Description));
            return ServiceResult<ApplicationUser>.Fail(msg);
        }

        if (request.IsStudent)
            await _userManager.AddToRoleAsync(user, RoleNames.Student);
        if (request.IsSupervisor)
            await _userManager.AddToRoleAsync(user, RoleNames.Supervisor);
        if (request.IsAdmin)
            await _userManager.AddToRoleAsync(user, RoleNames.Admin);

        return ServiceResult<ApplicationUser>.Ok(user);
    }

    public async Task<ServiceResult<EditUserRolesViewModel>> GetEditRolesModelAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ServiceResult<EditUserRolesViewModel>.Fail("User was not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var vm = new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? "",
            DisplayName = user.DisplayName,
            IsStudent = roles.Contains(RoleNames.Student),
            IsSupervisor = roles.Contains(RoleNames.Supervisor),
            IsAdmin = roles.Contains(RoleNames.Admin)
        };

        return ServiceResult<EditUserRolesViewModel>.Ok(vm);
    }

    public async Task<ServiceResult> SetUserRolesAsync(
        string userId,
        IReadOnlySet<string> roles,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ServiceResult.Fail("User was not found.");

        var desired = new HashSet<string>(StringComparer.Ordinal);
        if (roles.Contains(RoleNames.Student))
            desired.Add(RoleNames.Student);
        if (roles.Contains(RoleNames.Supervisor))
            desired.Add(RoleNames.Supervisor);
        if (roles.Contains(RoleNames.Admin))
            desired.Add(RoleNames.Admin);

        if (desired.Count == 0)
            return ServiceResult.Fail("Select at least one role.");

        var current = (await _userManager.GetRolesAsync(user)).ToHashSet(StringComparer.Ordinal);

        var removingAdmin = current.Contains(RoleNames.Admin) && !desired.Contains(RoleNames.Admin);
        if (removingAdmin)
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);
            if (adminUsers.Count <= 1 && adminUsers.FirstOrDefault()?.Id == userId)
                return ServiceResult.Fail("Cannot remove the last administrator.");
        }

        foreach (var role in AllAppRoles)
        {
            var want = desired.Contains(role);
            var has = current.Contains(role);
            if (want && !has)
                await _userManager.AddToRoleAsync(user, role);
            if (!want && has)
                await _userManager.RemoveFromRoleAsync(user, role);
        }

        return ServiceResult.Ok();
    }
}
