using Microsoft.EntityFrameworkCore;
using VoteSvc.Models;

namespace VoteSvc.Data;

public class VoteDbContext : DbContext
{
    public VoteDbContext(DbContextOptions<VoteDbContext> options) : base(options)
    {
    }
    
    public DbSet<Vote> Votes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vote>(e =>
        {
            e.HasKey(v => new { v.VoterId, v.TargetId, v.TargetType });
            e.Property(v => v.TargetType).HasConversion<string>();
            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_vote_value", "value IN (1, -1)");
                t.HasCheckConstraint("ck_vote_target_type", "target_type IN ('Question', 'Answer')");
            });
            e.HasIndex(v => new { v.TargetId, v.TargetType }); // for the SUM (for score) query
        });
    }
}
