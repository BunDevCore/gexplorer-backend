using NetTopologySuite.Geometries;

namespace GdanskExplorer.Achievements;

public interface IAchievable
{
    public Guid Id { get; }
    public bool CheckOverallArea(MultiPolygon area);

    public bool CheckTrip(Polygon tripPolygon);
}