using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

public class ResearchArea
{
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,200}$", ErrorMessage = "Name must not be empty or whitespace-only.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Description")]
    [RegularExpression(@"^$|^(?!\s+$)[\s\S]{1,1000}$", ErrorMessage = "Description cannot be only whitespace.")]
    public string? Description { get; set; }

    public ICollection<ProjectProposal> ProjectProposals { get; set; } = new List<ProjectProposal>();

    public ICollection<SupervisorResearchArea> SupervisorResearchAreas { get; set; } = new List<SupervisorResearchArea>();
}
