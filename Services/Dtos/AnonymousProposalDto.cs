namespace BlindMatchPAS.Services.Dtos;

/// <summary>
/// Supervisor-facing view: no student identity fields (blind matching).
/// </summary>
public sealed class AnonymousProposalDto
{
    public int ProposalId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Abstract { get; init; } = string.Empty;

    public string TechStack { get; init; } = string.Empty;

    public int ResearchAreaId { get; init; }

    public string ResearchAreaName { get; init; } = string.Empty;
}
