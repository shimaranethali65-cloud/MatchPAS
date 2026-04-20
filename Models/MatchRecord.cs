using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlindMatchPAS.Models;

[Table("ProjectMatches")]
public class MatchRecord
{
    public int Id { get; set; }

    public int ProposalId { get; set; }

    public ProjectProposal ProjectProposal { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    [Display(Name = "Supervisor")]
    public string SupervisorId { get; set; } = string.Empty;

    public ApplicationUser Supervisor { get; set; } = null!;

    [Display(Name = "Matched date")]
    [Column("ConfirmedAt")]
    public DateTimeOffset MatchedAt { get; set; }
}
