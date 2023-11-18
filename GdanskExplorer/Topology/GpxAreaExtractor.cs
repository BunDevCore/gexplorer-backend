using System.Xml;
using DotSpatial.Projections;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Buffer;

namespace GdanskExplorer.Topology;

public class GpxAreaExtractor
{
    private readonly ILogger<GpxAreaExtractor> _log;
    private readonly GeometryFactory _gpsFactory = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
    private readonly DotSpatialReprojector _reprojectBuffer;
    private readonly DotSpatialReprojector _reprojectCommon;
    private readonly AreaCalculationOptions _options;

    public GpxAreaExtractor(AreaCalculationOptions options, ILogger<GpxAreaExtractor> log)
    {
        _log = log;
        _options = options;
        _reprojectBuffer = new DotSpatialReprojector(ProjectionInfo.FromEpsgCode(4326),
            ProjectionInfo.FromProj4String(_options.BufferProj4));
        _reprojectCommon = new DotSpatialReprojector(ProjectionInfo.FromEpsgCode(4326),
            ProjectionInfo.FromEpsgCode(_options.CommonAreaSrid));
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

            var bufferLinestring = gpsLinestring.Copy();
            bufferLinestring.Apply(_reprojectBuffer);
            _log.LogDebug("arealinestring length = {Length}", bufferLinestring.Length);

            var bufferedPolygon = bufferLinestring.Buffer(_options.BufferRadius) as Polygon;

            _log.LogDebug("polygon is null? {isNull}", bufferedPolygon == null);

            var gpsPolygon = bufferedPolygon?.Copy() as Polygon;
            gpsPolygon?.Apply(_reprojectBuffer.Reversed());

            var areaPolygon = gpsPolygon?.Copy() as Polygon;
            areaPolygon?.Apply(_reprojectCommon);

            _log.LogInformation("track segment processed");

            Console.WriteLine();

            if (bufferedPolygon is null)
            {
                throw new InvalidOperationException("buffer polygon somehow ended up null");
            }
            
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