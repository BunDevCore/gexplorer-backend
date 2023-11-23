using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class User : IdentityUser<Guid>
{
    public MultiPolygon OverallArea { get; set; } = new(Array.Empty<Polygon>());
    public double OverallAreaAmount { get; set; }

    public DateTime JoinedAt { get; set; }
    public List<Trip> Trips { get; set; } = new();

    public List<AchievementGet> AchievementGets { get; set; } = new();
    public List<Achievement> Achievements { get; set; } = new();

    public List<DistrictAreaCacheEntry> DistrictAreas { get; set; } = new();
}