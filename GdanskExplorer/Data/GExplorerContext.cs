using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GdanskExplorer.Data;

public class GExplorerContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public GExplorerContext(DbContextOptions<GExplorerContext> options) : base(options) {}

    public DbSet<Trip> Trips { get; set; } = null!;

    public DbSet<AchievementGet> AchievementGets { get; set; } = null!;

    public DbSet<District> Districts { get; set; } = null!;
    public DbSet<DistrictAreaCacheEntry> DistrictAreaCacheEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AchievementGet>()
            .HasKey(x => new { x.UserId, x.AchievementId });
        
        modelBuilder.Entity<DistrictAreaCacheEntry>()
            .HasKey(x => new { x.DistrictId, x.UserId });

        modelBuilder.Entity<User>()
            .HasMany(x => x.Trips)
            .WithOne(x => x.User);

        modelBuilder.Entity<User>()
            .HasMany<Achievement>(x => x.Achievements)
            .WithMany(x => x.Achievers)
            .UsingEntity<AchievementGet>();

        modelBuilder.Entity<User>()
            .Property(x => x.JoinedAt)
            .HasDefaultValueSql("now()");
    }
}