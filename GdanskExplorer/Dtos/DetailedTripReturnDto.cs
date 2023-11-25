using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Dtos;

public class DetailedTripReturnDto
{
    public Guid Id { get; set; }
    public ShortUserReturnDto User { get; set; } = null!;
    public double Area { get; set; }

    [JsonConverter(typeof(GeometryJsonConverter))]
    public Geometry GpsPolygon { get; set; } = null!;
}