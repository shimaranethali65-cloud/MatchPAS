using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.ViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [StringLength(256, MinimumLength = 3)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
