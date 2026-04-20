using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Models;

public class SupervisorResearchArea
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    [Display(Name = "Supervisor")]
    public string SupervisorId { get; set; } = string.Empty;

    public ApplicationUser Supervisor { get; set; } = null!;

    [Display(Name = "Research area")]
    public int ResearchAreaId { get; set; }

    public ResearchArea ResearchArea { get; set; } = null!;
}
