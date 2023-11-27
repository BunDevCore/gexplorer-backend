using GdanskExplorer.Data;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Achievements;

public class GeometryVisitedAchievement : IAchievable
{
    public Geometry Target { get; }
    public Guid Id { get; }
    public string IconId { get; }

    public GeometryVisitedAchievement(Guid id, Geometry target, string iconId = "default")
    {
        Target = target;
        IconId = iconId;
        Id = id;
    }

    public bool CheckOverallArea(MultiPolygon area) =>
        Target switch {
            MultiPoint multiPoint => multiPoint.Within(area),
            MultiPolygon multiPolygon => multiPolygon.Geometries.Cast<Polygon>().All(p => p.Intersects(area)),
            Point point => point.Within(area),
            Polygon polygon => polygon.Intersects(area),
            _ => false,
        };

    public bool CheckTrip(Trip trip) =>
        Target switch {
            MultiPoint multiPoint => multiPoint.Within(trip.Polygon),
            MultiPolygon multiPolygon => multiPolygon.Geometries.Cast<Polygon>().All(p => p.Intersects(trip.Polygon)),
            Point point => point.Within(trip.Polygon),
            Polygon polygon => polygon.Intersects(trip.Polygon),
            _ => false,
        };
}