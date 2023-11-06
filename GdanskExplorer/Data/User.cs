using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class User : IdentityUser<Guid>
{
    public MultiPolygon OverallArea { get; set; } = new(Array.Empty<Polygon>());

    public DateTime JoinedAt { get; set; }
    public List<Trip> Trips { get; set; } = new();
    
}