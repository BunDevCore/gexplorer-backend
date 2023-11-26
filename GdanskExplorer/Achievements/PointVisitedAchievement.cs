using NetTopologySuite.Geometries;

namespace GdanskExplorer.Achievements;

public class PointVisitedAchievement : IAchievable
{
    public Point Target { get; }
    public Guid Id { get; }

    public PointVisitedAchievement(Guid id, Point target)
    {
        Target = target;
        Id = id;
    }
    
    public bool CheckOverallArea(MultiPolygon area) => 
        Target.Within(area);

    public bool CheckTrip(Polygon tripPolygon) =>
        Target.Within(tripPolygon);
}