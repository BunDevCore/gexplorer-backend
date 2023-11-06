using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GdanskExplorer.Data;

public class GExplorerContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public GExplorerContext(DbContextOptions<GExplorerContext> options) : base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Trip> Trips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AchievementGet>()
            .HasKey(x => new { x.UserId, x.AchievementId });

        modelBuilder.Entity<User>()
            .HasMany(x => x.Trips)
            .WithOne(x => x.User);

        modelBuilder.Entity<User>()
            .HasMany<Achievement>()
            .WithMany()
            .UsingEntity<AchievementGet>();
    }
}