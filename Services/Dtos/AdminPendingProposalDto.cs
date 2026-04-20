namespace BlindMatchPAS.Services.Dtos;

/// <summary>
/// Submitted proposal with no <see cref="Models.MatchRecord"/> yet (admin visibility).
/// </summary>
public sealed class AdminPendingProposalDto
{
    public int ProposalId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string StudentEmail { get; init; } = string.Empty;

    public string ResearchAreaName { get; init; } = string.Empty;

    public DateTimeOffset LastUpdated { get; init; }
}
