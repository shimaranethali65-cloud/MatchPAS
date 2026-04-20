using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

public class StudentProposalFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Select a research area.")]
    [Range(1, int.MaxValue, ErrorMessage = "Select a research area.")]
    [Display(Name = "Research area")]
    public int ResearchAreaId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,500}$", ErrorMessage = "Title must not be empty or whitespace-only.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(8000, MinimumLength = 1)]
    [DataType(DataType.MultilineText)]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,8000}$", ErrorMessage = "Abstract must contain non-whitespace text.")]
    public string Abstract { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    [Display(Name = "Tech stack")]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,2000}$", ErrorMessage = "Tech stack must contain non-whitespace text.")]
    public string TechStack { get; set; } = string.Empty;

    public IList<ResearchArea> ResearchAreas { get; set; } = new List<ResearchArea>();
}
