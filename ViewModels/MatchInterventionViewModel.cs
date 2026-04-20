using System.ComponentModel.DataAnnotations;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.ViewModels;

public sealed class MatchInterventionViewModel
{
    public int ProposalId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ResearchAreaName { get; set; } = string.Empty;

    [Display(Name = "Student")]
    public string StudentEmail { get; set; } = string.Empty;

    public ProposalStatus Status { get; set; }

    public bool HasMatch { get; set; }

    [Display(Name = "Matched supervisor")]
    public string? CurrentSupervisorEmail { get; set; }

    [Display(Name = "Matched at")]
    public DateTimeOffset? MatchedAt { get; set; }

    /// <summary>
    /// Supervisors who cover this proposal's research area (for reassign / force match).
    /// </summary>
    public IList<SupervisorPickItem> EligibleSupervisors { get; set; } = new List<SupervisorPickItem>();

    /// <summary>
    /// POST: selected supervisor for reassign or force match.
    /// </summary>
    [Display(Name = "Supervisor")]
    public string? SelectedSupervisorId { get; set; }
}

public sealed class SupervisorPickItem
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
