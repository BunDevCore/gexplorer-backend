using NetTopologySuite.Geometries;

namespace GdanskExplorer.Topology;

public class TripTopologyInfo
{
    public LineString GpsPath;

    public Polygon GpsPolygon;
    
    public Polygon AreaPolygon;
    

    public TripTopologyInfo(LineString gpsPath, Polygon gpsPolygon, Polygon areaPolygon)
    {
        GpsPath = gpsPath;
        GpsPolygon = gpsPolygon;
        AreaPolygon = areaPolygon;
    }
}