using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlindMatchPAS.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(256)]
    public string? DisplayName { get; set; }

    public ICollection<ProjectProposal> ProjectProposals { get; set; } = new List<ProjectProposal>();

    public ICollection<MatchRecord> SupervisedMatches { get; set; } = new List<MatchRecord>();

    public ICollection<SupervisorResearchArea> SupervisorResearchAreas { get; set; } = new List<SupervisorResearchArea>();
}
