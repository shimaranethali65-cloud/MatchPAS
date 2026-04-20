using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Services.Requests;

public sealed class CreateProposalRequest
{
    [Required]
    public int ResearchAreaId { get; init; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(8000, MinimumLength = 1)]
    public string Abstract { get; init; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string TechStack { get; init; } = string.Empty;
}

public sealed class UpdateProposalRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(8000, MinimumLength = 1)]
    public string Abstract { get; init; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string TechStack { get; init; } = string.Empty;

    [Required]
    public int ResearchAreaId { get; init; }
}
