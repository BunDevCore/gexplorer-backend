using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Dtos;

public class DetailedTripReturnDto
{
    public Guid Id { get; set; }
    public ShortUserReturnDto User { get; set; } = null!;
    public double Area { get; set; }
    public double Length { get; set; }

    [JsonConverter(typeof(GeometryJsonConverter))]
    public Geometry GpsPolygon { get; set; } = null!;
    
    public double NewArea { get; set; }
    public List<AchievementGetDto> NewAchievements { get; set; } = new();
    
    public DateTime UploadTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Starred { get; set; }
}