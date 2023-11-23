using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class District
{
    public Guid Id { get; set; }
    public Polygon Geometry { get; set; } = null!;
    public double Area { get; set; }
    public string Name { get; set; } = null!;
}