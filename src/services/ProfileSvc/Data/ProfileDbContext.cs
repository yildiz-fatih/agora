using Microsoft.EntityFrameworkCore;
using ProfileSvc.Models;

namespace ProfileSvc.Data;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options)
    {
    }
    
    public DbSet<Profile> Profiles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
        
    }
}
