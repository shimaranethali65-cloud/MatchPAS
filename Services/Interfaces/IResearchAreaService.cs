using BlindMatchPAS.Models;
using BlindMatchPAS.Services;

namespace BlindMatchPAS.Services.Interfaces;

public interface IResearchAreaService
{
    Task<IReadOnlyList<ResearchArea>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResearchArea>> GetForSupervisorAsync(string supervisorId, CancellationToken cancellationToken = default);

    Task<ResearchArea?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResult<ResearchArea>> CreateAsync(string name, string? description, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAsync(int id, string name, string? description, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
