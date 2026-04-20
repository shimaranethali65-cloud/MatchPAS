namespace BlindMatchPAS.Services.Dtos;

/// <summary>
/// Shown to the matched supervisor only after <see cref="Models.ProjectProposal.IdentitiesRevealed"/> is true.
/// </summary>
public sealed class MatchedProposalForSupervisorDto
{
    public int ProposalId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Abstract { get; init; } = string.Empty;

    public string TechStack { get; init; } = string.Empty;

    public string ResearchAreaName { get; init; } = string.Empty;

    public string StudentId { get; init; } = string.Empty;

    public string StudentEmail { get; init; } = string.Empty;

    public string? StudentDisplayName { get; init; }

    public DateTimeOffset MatchedAt { get; init; }
}
