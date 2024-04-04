using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Data;

public class Achievement
{
    [MaxLength(100)]
    public string Id { get; set; }
    public List<AchievementGet> AchievementGets { get; set; }
    public List<User> Achievers { get; set; } = new();
    
    public Geometry? Target { get; set; } = Polygon.Empty;

    public bool IsSecret { get; set; } = false;
}