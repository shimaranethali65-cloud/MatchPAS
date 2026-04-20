using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels;

public class ResearchAreaFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@"^(?!\s+$)[\s\S]{1,200}$", ErrorMessage = "Name must not be empty or whitespace-only.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Description")]
    [RegularExpression(@"^$|^(?!\s+$)[\s\S]{1,1000}$", ErrorMessage = "Description cannot be only whitespace.")]
    public string? Description { get; set; }
}
