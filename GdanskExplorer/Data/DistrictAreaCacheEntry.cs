using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class DistrictAreaCacheEntry
{
    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public double Area { get; set; }
}