using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.Services.Dtos;

namespace BlindMatchPAS.Services.Interfaces;

public interface IMatchingService
{
    /// <summary>
    /// Anonymous proposals in the supervisor research areas (no student identity).
    /// </summary>
    Task<IReadOnlyList<AnonymousProposalDto>> ListAnonymousProposalsAsync(
        string supervisorId,
        int? researchAreaIdFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Blind detail for review. Fails if the proposal is not in scope or not submitted; never exposes student identity.
    /// </summary>
    Task<ServiceResult<AnonymousProposalDto>> GetAnonymousProposalForSupervisorAsync(
        string supervisorId,
        int proposalId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<MatchRecord>> ConfirmMatchAsync(string supervisorId, int proposalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Full proposal plus student identity for the matched supervisor only.
    /// </summary>
    Task<ServiceResult<MatchedProposalForSupervisorDto>> GetMatchedProposalForSupervisorAsync(
        string supervisorId,
        int proposalId,
        CancellationToken cancellationToken = default);

    Task<AdminMatchesDashboardDto> GetAdminMatchesDashboardAsync(CancellationToken cancellationToken = default);
}
