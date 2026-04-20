namespace BlindMatchPAS.Services.Dtos;

public sealed class AdminMatchSummaryDto
{
    public int ProposalId { get; init; }

    public string ProposalTitle { get; init; } = string.Empty;

    public string StudentEmail { get; init; } = string.Empty;

    public string SupervisorEmail { get; init; } = string.Empty;

    public string ResearchAreaName { get; init; } = string.Empty;

    public DateTimeOffset MatchedAt { get; init; }
}
