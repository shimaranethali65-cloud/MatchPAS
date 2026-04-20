using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

public class RegisterViewModel
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    [RegularExpression(
        @"^[\p{L}\p{M}\p{N}\s'’\-\.]+$",
        ErrorMessage = "Use letters, numbers, spaces, and simple punctuation only.")]
    [Display(Name = "Display name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256, MinimumLength = 3)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least {2} characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "I am registering as")]
    public string Role { get; set; } = RoleNames.Student;
}
