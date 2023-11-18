using System.Xml;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Buffer;

namespace GdanskExplorer.Topology;

public class GpxAreaExtractor
{
    private readonly DotSpatialReprojector _reproject;
    private readonly ILogger<GpxAreaExtractor> _log;
    private readonly GeometryFactory _gpsFactory = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    private readonly GeometryFactory _areaFactory;

    public GpxAreaExtractor(DotSpatialReprojector reproject, ILogger<GpxAreaExtractor> log)
    {
        _reproject = reproject;
        _log = log;
        _areaFactory = NtsGeometryServices.Instance.CreateGeometryFactory(reproject.DestSrid);
    }

    public TripTopologyInfo[] ProcessGpx(Stream gpxContents)
    {
        var gpx = GpxFile.ReadFrom(XmlReader.Create(gpxContents), new GpxReaderSettings
        {
            IgnoreUnexpectedChildrenOfTopLevelElement = true,
        });

        _log.LogDebug("found {tracks} tracks in gpx file", gpx.Tracks.Count);

        var polygons = gpx.Tracks.SelectMany(track => track.Segments).Select(segment =>
        {
            _log.LogDebug("converting track segment ({numWaypoints} waypoints)", segment.Waypoints.Count);
            
            var gpsLinestring = _gpsFactory.CreateLineString(segment.Waypoints.Select(x =>
                new Coordinate(x.Longitude, x.Latitude)).ToArray());

            var areaLinestring = gpsLinestring.Copy();
            areaLinestring.Apply(_reproject);
            _log.LogDebug("arealinestring length = {Length}", areaLinestring.Length);

            var areaPolygon = areaLinestring.Buffer(7, EndCapStyle.Flat) as Polygon;

            _log.LogDebug("polygon is null? {isNull}", areaPolygon == null);

            var gpsPolygon = areaPolygon?.Copy() as Polygon;
            gpsPolygon?.Apply(_reproject.Reversed());

            _log.LogInformation("track segment processed");

            Console.WriteLine();

            if (areaPolygon is null)
            {
                throw new InvalidOperationException("area polygon somehow ended up null");
            }

            if (gpsPolygon is null)
            {
                throw new InvalidOperationException("gps polygon somehow ended up null");
            }
            
            return new TripTopologyInfo(gpsLinestring, gpsPolygon, areaPolygon);
        });
        
        return polygons.ToArray();
    }
}