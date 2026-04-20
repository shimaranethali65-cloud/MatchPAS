using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using BlindMatchPAS.Services.Requests;

namespace BlindMatchPAS.Services.Interfaces;

public interface IProposalService
{
    Task<IReadOnlyList<ProjectProposal>> ListForStudentAsync(string studentId, CancellationToken cancellationToken = default);

    Task<ProjectProposal?> GetOwnedProposalAsync(int proposalId, string studentId, CancellationToken cancellationToken = default);

    Task<ServiceResult<ProjectProposal>> CreateAsync(string studentId, CreateProposalRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<ProjectProposal>> UpdateAsync(int proposalId, string studentId, UpdateProposalRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> WithdrawAsync(int proposalId, string studentId, CancellationToken cancellationToken = default);

    Task<ServiceResult> SubmitAsync(int proposalId, string studentId, CancellationToken cancellationToken = default);
}
