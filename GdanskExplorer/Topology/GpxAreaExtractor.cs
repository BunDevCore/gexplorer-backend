using System.Xml;
using DotSpatial.Projections;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

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

        _log.LogDebug("found {NumTracks} tracks in gpx file", gpx.Tracks.Count);

        var polygons = gpx.Tracks.SelectMany(track => track.Segments).Select(segment =>
        {
            var startTime = segment.Waypoints.FirstOrDefault()?.TimestampUtc;
            if (!startTime.HasValue)
            {
                throw new TimeRequiredException("GPX track segment start point must have a timestamp!");
            }

            var endTime = segment.Waypoints.LastOrDefault()?.TimestampUtc;
            if (!endTime.HasValue)
            {
                throw new TimeRequiredException("GPX track segment end point must have a timestamp!");
            }

            _log.LogDebug("converting track segment ({NumWaypoints} waypoints)", segment.Waypoints.Count);

            var gpsLinestring = _gpsFactory.CreateLineString(segment.Waypoints.Select(x =>
                new Coordinate(x.Longitude, x.Latitude)).ToArray());

            var fullBufferLinestring = gpsLinestring.Copy() as LineString ??
                                       throw new InvalidOperationException("buffer linestring is null");
            fullBufferLinestring.Apply(_reprojectBuffer);

            var bufferLinestring = SimplifyLinestring(fullBufferLinestring);

            _log.LogDebug("post-simplification coordinates = {NumCoordinates}", bufferLinestring.Coordinates.Length);
            _log.LogDebug("bufferlinestring length = {Length}", bufferLinestring.Length);

            var bufferedPolygon = bufferLinestring.Buffer(_options.BufferRadius) as Polygon;

            _log.LogDebug("polygon is null? {IsNull}", bufferedPolygon == null);

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

            // these can be "null-asserted" by using .Value because they are explicitly checked at the beginning
            return new TripTopologyInfo(gpsLinestring, gpsPolygon, areaPolygon, bufferLinestring, startTime.Value, endTime.Value);
        });

        return polygons.ToArray();
    }

    private LineString SimplifyLinestring(LineString fullLinestring)
    {
        var newCoords = new List<Coordinate> { fullLinestring.Coordinates[0] };
        foreach (var coord in fullLinestring.Coordinates.Skip(1))
        {
            var last = newCoords.Last();
            var distanceSquared = (last.X - coord.X) * (last.X - coord.X) + (last.Y - coord.Y) * (last.Y - coord.Y);

            if (distanceSquared > _options.SimplificationDistanceSquared)
            {
                newCoords.Add(coord);
            }
        }

        return new LineString(newCoords.ToArray());
    }
}