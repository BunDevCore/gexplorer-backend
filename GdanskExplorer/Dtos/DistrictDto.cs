using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Dtos;

public class DistrictDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    [JsonConverter(typeof(GeometryJsonConverter))]
    public Geometry Geometry { get; set; } = null!;

    public double Area { get; set; }
}