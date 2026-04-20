using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

public sealed class EditUserRolesViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Display name")]
    public string? DisplayName { get; set; }

    [Display(Name = RoleNames.Student)]
    public bool IsStudent { get; set; }

    [Display(Name = RoleNames.Supervisor)]
    public bool IsSupervisor { get; set; }

    [Display(Name = RoleNames.Admin)]
    public bool IsAdmin { get; set; }
}
