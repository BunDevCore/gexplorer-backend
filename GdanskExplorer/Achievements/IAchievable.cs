using GdanskExplorer.Data;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Achievements;

public interface IAchievable
{
    public string Id { get; }
    public string IconId { get; }
    public bool CheckOverallArea(MultiPolygon area);

    public bool CheckTrip(Trip trip);
}