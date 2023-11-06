using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public MultiPolygon OverallArea { get; set; } = new(new Polygon[] { });
    
    public DateTime DateJoined { get; set; }
    public List<Trip> Trips { get; set; }
}