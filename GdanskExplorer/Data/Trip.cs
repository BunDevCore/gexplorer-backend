using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class Trip
{
    public Guid Id { get; set; }
    public User User { get; set; } = null!;
    public MultiPolygon Polygon { get; set; } = null!;
    public double Area { get; set; }
    public double Length { get; set; }
    public DateTime UploadDate { get; set; }
}