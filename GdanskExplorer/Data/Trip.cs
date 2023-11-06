using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class Trip
{
    public Guid Id { get; set; }
    public User User { get; set; } = null!;
    public MultiPolygon Polygon { get; set; } = null!;
    public float Area { get; set; }
    public float Length { get; set; }
    public DateTime UploadDate { get; set; }
}