using System.Text;
using NetTopologySuite.Geometries;

namespace GdanskExplorer;

public static class Extensions
{
    public static Stream AsUtf8Stream(this string s)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(s ?? ""));
    }

    public static MultiPolygon AsMultiPolygon(this Geometry g) =>
        g switch
        {
            MultiPolygon multiPolygon => multiPolygon,
            Polygon polygon => new MultiPolygon(new[] { polygon }),
            _ => throw new ArgumentOutOfRangeException(nameof(g))
        };
}