namespace BlindMatchPAS.ViewModels;

public sealed class AdminUserRowViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string RolesSummary { get; set; } = string.Empty;
}
