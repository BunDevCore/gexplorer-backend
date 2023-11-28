using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GdanskExplorer.Data;

public class GExplorerContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public GExplorerContext(DbContextOptions<GExplorerContext> options) : base(options) {}
    
    /// <summary>
    /// This constructor only exists to appease the compiler in the GExplorerLeaderboardContext constructor. It will never be called otherwise.
    /// </summary>
    /// <param name="options">db context options, purposefully the non-generic variant so any can be passed</param>
    protected GExplorerContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Trip> Trips { get; set; } = null!;

    public DbSet<AchievementGet> AchievementGets { get; set; } = null!;
    public DbSet<Achievement> Achievements { get; set; } = null!;

    public DbSet<District> Districts { get; set; } = null!;
    public DbSet<DistrictAreaCacheEntry> DistrictAreaCacheEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AchievementGet>()
            .HasKey(x => new { x.UserId, x.AchievementId });
        
        modelBuilder.Entity<User>()
            .HasMany<Achievement>(x => x.Achievements)
            .WithMany(x => x.Achievers)
            .UsingEntity<AchievementGet>();
        
        modelBuilder.Entity<DistrictAreaCacheEntry>()
            .HasKey(x => new { x.DistrictId, x.UserId });

        modelBuilder.Entity<User>()
            .HasMany(x => x.Trips)
            .WithOne(x => x.User);

        modelBuilder.Entity<User>()
            .HasMany(x => x.DistrictAreas)
            .WithOne(x => x.User);

        modelBuilder.Entity<District>()
            .HasMany(x => x.DistrictAreaCacheEntries)
            .WithOne(x => x.District);

        modelBuilder.Entity<User>()
            .Property(x => x.JoinedAt)
            .HasDefaultValueSql("now()");
    }
}