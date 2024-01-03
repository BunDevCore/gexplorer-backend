using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class User : IdentityUser<Guid>
{
    public User() : base() {}

    public User(GExplorerContext db) : base()
    {
        DistrictAreas = db.Districts.Select(x => new DistrictAreaCacheEntry
        {
            UserId = this.Id,
            Area = 0,
            DistrictId = x.Id
        }).ToList();
        
        db.SaveChangesAsync();
    }
    
    public MultiPolygon OverallArea { get; set; } = new(Array.Empty<Polygon>());
    public double OverallAreaAmount { get; set; }

    public DateTime JoinedAt { get; set; }
    public List<Trip> Trips { get; set; } = new();

    public int TripAmount => Trips.Count;

    public List<AchievementGet> AchievementGets { get; set; } = new();
    public List<Achievement> Achievements { get; set; } = new();

    public List<DistrictAreaCacheEntry> DistrictAreas { get; set; } = new();
}