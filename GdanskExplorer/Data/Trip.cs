using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class Trip
{
    public Guid Id { get; set; }
    public User User { get; set; } = null!;
    public MultiPolygon Area { get; set; } = null!;
    public DateTime UploadDate { get; set; }
}