using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Services.Interfaces;

public interface IAdminMatchOverrideService
{
    Task<ServiceResult<MatchInterventionViewModel>> GetInterventionModelAsync(int proposalId, CancellationToken cancellationToken = default);

    Task<ServiceResult> ClearMatchAsync(int proposalId, CancellationToken cancellationToken = default);

    Task<ServiceResult> ReassignSupervisorAsync(int proposalId, string newSupervisorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin-only: create a match for a submitted proposal without supervisor self-service confirm.
    /// </summary>
    Task<ServiceResult> ForceMatchAsync(int proposalId, string supervisorId, CancellationToken cancellationToken = default);
}
