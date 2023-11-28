using GdanskExplorer.Data;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Achievements;

public class GeometryVisitedAchievement : IAchievable
{
    public Geometry Target { get; set;  }
    public string Id { get; set; }
    public string IconId { get; }

    public GeometryVisitedAchievement(string id, Geometry target, string iconId = "default")
    {
        Target = target;
        IconId = iconId;
        Id = id;
    }

    public bool CheckUser(User user) =>
        Target switch {
            MultiPoint multiPoint => multiPoint.Within(user.OverallArea),
            MultiPolygon multiPolygon => multiPolygon.Geometries.Cast<Polygon>().All(p => p.Intersects(user.OverallArea)),
            Point point => point.Within(user.OverallArea),
            Polygon polygon => polygon.Intersects(user.OverallArea),
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