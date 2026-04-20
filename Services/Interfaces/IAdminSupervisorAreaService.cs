using BlindMatchPAS.Services;
using BlindMatchPAS.ViewModels;

namespace BlindMatchPAS.Services.Interfaces;

public interface IAdminSupervisorAreaService
{
    Task<IReadOnlyList<SupervisorListItemViewModel>> GetSupervisorsWithAreaCountsAsync(CancellationToken cancellationToken = default);

    Task<SupervisorAreasEditViewModel?> GetEditModelAsync(string supervisorId, CancellationToken cancellationToken = default);

    Task<ServiceResult> SetSupervisorResearchAreasAsync(
        string supervisorId,
        IReadOnlyList<int> researchAreaIds,
        CancellationToken cancellationToken = default);
}
