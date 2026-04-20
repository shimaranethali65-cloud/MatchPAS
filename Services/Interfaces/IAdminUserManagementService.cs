using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.Services.Requests;
using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Services.Interfaces;

public interface IAdminUserManagementService
{
    Task<IReadOnlyList<AdminUserRowViewModel>> ListUsersAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<ApplicationUser>> CreateUserAsync(CreateAdminUserRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<EditUserRolesViewModel>> GetEditRolesModelAsync(string userId, CancellationToken cancellationToken = default);

    Task<ServiceResult> SetUserRolesAsync(string userId, IReadOnlySet<string> roles, CancellationToken cancellationToken = default);
}
