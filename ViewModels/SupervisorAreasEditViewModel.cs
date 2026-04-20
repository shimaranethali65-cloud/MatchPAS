namespace BlindMatchPAS.ViewModels;

public class SupervisorAreasEditViewModel
{
    public string SupervisorId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public List<ResearchAreaAssignmentItem> Areas { get; set; } = new();
}

public class ResearchAreaAssignmentItem
{
    public int ResearchAreaId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsSelected { get; set; }
}
