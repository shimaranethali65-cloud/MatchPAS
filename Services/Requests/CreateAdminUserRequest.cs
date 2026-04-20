namespace BlindMatchPAS.Services.Requests;

public sealed class CreateAdminUserRequest
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsStudent { get; init; }

    public bool IsSupervisor { get; init; }

    public bool IsAdmin { get; init; }
}
