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

    public DbSet<Place> Places { get; set; } = null!;

    public DbSet<PlaceVisitedRow> PlaceVisitedRows { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // "fake" the dbset here so i can return it in leaderboard queries
        modelBuilder.Entity<DatabaseLeaderboardRow>().HasNoKey();

        modelBuilder.Entity<AchievementGet>()
            .HasKey(x => new { x.UserId, x.AchievementId });

        modelBuilder.Entity<AchievementGet>()
            .HasOne<Trip>(x => x.AchievedOnTrip)
            .WithMany(x => x.NewAchievements);
        
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
        
        modelBuilder.Entity<PlaceVisitedRow>()
            .HasOne<User>(x => x.User)
            .WithMany(x => x.PlaceRows);
        
        modelBuilder.Entity<User>()
            .HasMany<Place>(x => x.Places)
            .WithMany(x => x.Users)
            .UsingEntity<PlaceVisitedRow>();
    }
    
    public void InitDistrictAreas(User u)
    {
        u.DistrictAreas = Districts.Select(x => new DistrictAreaCacheEntry
        {
            UserId = u.Id,
            Area = 0,
            DistrictId = x.Id
        }).ToList();
    }
}