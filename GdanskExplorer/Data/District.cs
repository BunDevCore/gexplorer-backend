using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class District
{
    public Guid Id { get; set; }
    public Polygon Area { get; set; } = null!;
    public string Name { get; set; } = null!;
}