using BlindMatchPAS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Data;

/// <summary>
/// Identity + domain data. Delete behaviors: <see cref="DeleteBehavior.Restrict"/> on references to users and
/// research areas (avoids SQL Server cascade cycles and accidental mass deletes). <see cref="DeleteBehavior.Cascade"/>
/// only where a child is strictly owned (supervisor research-area links; match row when its proposal is removed).
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProjectProposal> ProjectProposals => Set<ProjectProposal>();

    public DbSet<ResearchArea> ResearchAreas => Set<ResearchArea>();

    public DbSet<MatchRecord> MatchRecords => Set<MatchRecord>();

    public DbSet<SupervisorResearchArea> SupervisorResearchAreas => Set<SupervisorResearchArea>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ProjectProposal>(entity =>
        {
            entity.HasOne(p => p.Student)
                .WithMany(u => u.ProjectProposals)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.ResearchArea)
                .WithMany(r => r.ProjectProposals)
                .HasForeignKey(p => p.ResearchAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.Status).HasConversion<int>();
        });

        builder.Entity<MatchRecord>(entity =>
        {
            entity.HasIndex(e => e.ProposalId).IsUnique();

            entity.HasOne(m => m.ProjectProposal)
                .WithOne(p => p.MatchRecord)
                .HasForeignKey<MatchRecord>(m => m.ProposalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Supervisor)
                .WithMany(u => u.SupervisedMatches)
                .HasForeignKey(m => m.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SupervisorResearchArea>(entity =>
        {
            entity.HasIndex(e => new { e.SupervisorId, e.ResearchAreaId }).IsUnique();

            entity.HasOne(e => e.Supervisor)
                .WithMany(u => u.SupervisorResearchAreas)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ResearchArea)
                .WithMany(r => r.SupervisorResearchAreas)
                .HasForeignKey(e => e.ResearchAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
