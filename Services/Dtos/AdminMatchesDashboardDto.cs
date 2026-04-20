namespace BlindMatchPAS.Services.Dtos;

public sealed class AdminMatchesDashboardDto
{
    public IReadOnlyList<AdminMatchSummaryDto> ConfirmedMatches { get; init; } = Array.Empty<AdminMatchSummaryDto>();

    public IReadOnlyList<AdminPendingProposalDto> SubmittedAwaitingMatch { get; init; } = Array.Empty<AdminPendingProposalDto>();
}
