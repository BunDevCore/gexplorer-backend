using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GdanskExplorer.Dtos;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GeometryJsonConverter : JsonConverter<Geometry>
{
    public override Geometry Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var r = new GeoJsonReader();
        return r.Read<Geometry>(reader.GetString());
    }

    public override void Write(
        Utf8JsonWriter writer,
        Geometry geometry,
        JsonSerializerOptions options)
    {
        var json = new GeoJsonWriter().Write(geometry);
        writer.WriteRawValue(json, true);
    }
}