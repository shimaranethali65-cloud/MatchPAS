using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlindMatchPAS.Models;

[Table("Proposals")]
public class ProjectProposal
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser Student { get; set; } = null!;

    public int ResearchAreaId { get; set; }

    public ResearchArea ResearchArea { get; set; } = null!;

    [Required]
    [StringLength(500, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,500}$", ErrorMessage = "Title must not be empty or whitespace-only.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(8000, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,8000}$", ErrorMessage = "Abstract must contain non-whitespace text.")]
    [Display(Name = "Abstract")]
    public string Abstract { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,2000}$", ErrorMessage = "Tech stack must contain non-whitespace text.")]
    [Display(Name = "Tech stack")]
    public string TechStack { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public ProposalStatus Status { get; set; } = ProposalStatus.Draft;

    /// <summary>
    /// When true, student and supervisor identities may be shown to each other (after match rules allow reveal).
    /// </summary>
    [Display(Name = "Identities revealed")]
    public bool IdentitiesRevealed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public MatchRecord? MatchRecord { get; set; }
}
