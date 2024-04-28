using System.Collections.Immutable;
using System.Text;
using System.Xml;
using AutoMapper;
using GdanskExplorer.Achievements;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GdanskExplorer.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TripController : ControllerBase
{
    private readonly ILogger<TripController> _logger;
    private readonly GExplorerContext _db;
    private readonly UserManager<User> _userManager;
    private readonly GpxAreaExtractor _areaExtractor;
    private readonly IMapper _mapper;
    private readonly AchievementManager _achievementMgr;
    private readonly TripManager _tripManager;

    public TripController(ILogger<TripController> logger, UserManager<User> userManager, GExplorerContext db,
        GpxAreaExtractor areaExtractor, IMapper mapper, AchievementManager achievementMgr, TripManager tripManager)
    {
        _logger = logger;
        _userManager = userManager;
        _db = db;
        _areaExtractor = areaExtractor;
        _mapper = mapper;
        _achievementMgr = achievementMgr;
        _tripManager = tripManager;
    }
    
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    [HttpPost("new/mobile")]
    public async Task<ActionResult<IEnumerable<TripReturnDto>>> AddMobileTrip([FromBody] MobileNewTripDto newTrip)
    {
        await using var sw = new Utf8StringWriter();
        await using var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings {Async = true});

        var waypoints = newTrip.Points.Select(point => new GpxWaypoint(
                coordinate: new Coordinate(point.Longitude, point.Latitude))
            .WithTimestampUtc(DateTimeOffset.FromUnixTimeMilliseconds(point.Timestamp).UtcDateTime));
        var segment = new List<GpxTrackSegment> { new GpxTrackSegment().WithWaypoints(waypoints) }.ToImmutableArray();
        var track = new List<GpxTrack> { new GpxTrack().WithSegments(segment) };

        GpxWriter.Write(writer: xmlWriter,
            settings: null,
            metadata: new GpxMetadata("internal"),
            waypoints: Enumerable.Empty<GpxWaypoint>(),
            routes: Enumerable.Empty<GpxRoute>(),
            tracks: track,
            new { });

        sw.Flush();
        xmlWriter.Flush();
        xmlWriter.Close();
        sw.Close();
        var gpxContents = sw.GetStringBuilder().ToString();
        var result = await AddNewTrip(new NewTripDto {GpxContents = gpxContents, User = null});

        return result;
    }

    [HttpPost("new")]
    public async Task<ActionResult<IEnumerable<TripReturnDto>>> AddNewTrip([FromBody] NewTripDto newTrip)
    {
        var user = await _userManager.GetUserAsync(User);
        _logger.LogInformation("a new gpx import is being attempted by {Username}", user?.UserName ?? "no user!");

        if (user is null)
        {
            return Unauthorized();
        }

        if (newTrip.User is not null && // user field exists
            newTrip.User != user.Id && // not adding for self
            !await _userManager.IsInRoleAsync(user, "Admin")) // not an admin
        {
            return Forbid();
        }

        if (newTrip.User is not null)
        {
            // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
            user = await _db.Users
                .FirstOrDefaultAsync(x => x.Id == newTrip.User);
            if (user is null) return NotFound("user id not found");
        }

        try
        {
            var newTrips = await _tripManager.AddTrip(user, newTrip);
            return Ok(_mapper.Map<List<TripReturnDto>>(newTrips));
        }
        catch (XmlException)
        {
            return BadRequest(_mapper.Map<GpxImportErrorDto>(GpxImportErrorKind.SyntaxError));
        }
        catch (TimeRequiredException)
        {
            return BadRequest(_mapper.Map<GpxImportErrorDto>(GpxImportErrorKind.TimeRequired));
        }
    }


    [HttpGet("starred")]
    public async Task<ActionResult<TripReturnDto>> GetStarred()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(_mapper.ProjectTo<TripReturnDto>(_db.Trips
            .Where(x => x.User.Id == user.Id)
            .Where(x => x.Starred)));
    }

    [HttpGet("id/{guid:guid}")]
    public async Task<ActionResult<DetailedTripReturnDto>> GetById([FromRoute] Guid guid)
    {
        var trip = _db.Trips.FirstOrDefault(x => x.Id.Equals(guid));

        if (trip == null)
        {
            return NotFound();
        }

        // load user separately instead of through .Include on the top level IQueryable because i can't be bothered to implement SimplifyUser properly
        // so it works with non User IQueryables
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == trip.UserId);
        if (user == null) throw new NullReferenceException();
        _logger.LogDebug("got trip owner = {User}", user);
        trip.User = user;

        // i messed about with the fields but let's just convince EF nothing happened
        _db.Entry(trip).State = EntityState.Unchanged;
        _db.Entry(user).State = EntityState.Unchanged;

        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<DetailedTripReturnDto>(trip));
    }

    [HttpPost("id/{guid:guid}/star")]
    public async Task<ActionResult<DetailedTripReturnDto>> SetStarred([FromRoute] Guid guid,
        [FromBody] StarStatusDto starred)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var trip = await _db.Trips.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == guid);

        if (trip == null)
        {
            return NotFound();
        }

        if (trip.User.Id != user.Id)
        {
            return Forbid();
        }

        trip.Starred = starred.Starred;

        await _db.SaveChangesAsync();
        return Ok(_mapper.Map<TripReturnDto>(trip));
    }
}