using NetTopologySuite.Geometries;

namespace GdanskExplorer.Topology;

public class TripTopologyInfo
{
    public LineString GpsPath;

    public Polygon GpsPolygon;
    
    public Polygon AreaPolygon;
    public LineString LocalLinestring;
    public DateTime StartTime;
    public DateTime EndTime;
    

    public TripTopologyInfo(LineString gpsPath, Polygon gpsPolygon, Polygon areaPolygon, LineString localLinestring, DateTime startTime, DateTime endTime)
    {
        GpsPath = gpsPath;
        GpsPolygon = gpsPolygon;
        AreaPolygon = areaPolygon;
        LocalLinestring = localLinestring;
        StartTime = startTime;
        EndTime = endTime;
    }
}