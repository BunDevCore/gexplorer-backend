using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class Trip
{
    public Guid Id { get; set; }
    public User User { get; set; } = null!;
    public Polygon Polygon { get; set; } = null!;
    [Column(TypeName = "geography")]
    public Polygon GpsPolygon { get; set; } = null!;
    [Column(TypeName = "geography")]
    public LineString GpsLineString { get; set; } = null!;
    public double Area { get; set; }
    public double Length { get; set; }
    public DateTime UploadTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<AchievementGet> NewAchievements { get; set; } = new();
    public double NewArea { get; set; }
}