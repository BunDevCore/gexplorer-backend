using NetTopologySuite.Geometries;

namespace GdanskExplorer.Topology;

public class TripTopologyInfo
{
    public LineString GpsPath;

    public Polygon GpsPolygon;
    
    public Polygon AreaPolygon;
    public LineString LocalLinestring;
    

    public TripTopologyInfo(LineString gpsPath, Polygon gpsPolygon, Polygon areaPolygon, LineString localLinestring)
    {
        GpsPath = gpsPath;
        GpsPolygon = gpsPolygon;
        AreaPolygon = areaPolygon;
        LocalLinestring = localLinestring;
    }
}