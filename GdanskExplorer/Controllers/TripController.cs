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

    public TripController(ILogger<TripController> logger, UserManager<User> userManager, GExplorerContext db,
        GpxAreaExtractor areaExtractor, IMapper mapper, AchievementManager achievementMgr)
    {
        _logger = logger;
        _userManager = userManager;
        _db = db;
        _areaExtractor = areaExtractor;
        _mapper = mapper;
        _achievementMgr = achievementMgr;
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
            // todo: run this on another thread

            // black box magic
            var tripTopologies = _areaExtractor.ProcessGpx(newTrip.GpxContents.AsUtf8Stream());
            var uploadTime = DateTime.UtcNow;

            // convert to database entities
            var dbTrips = tripTopologies.Select(topology =>
                new Trip
                {
                    Id = new Guid(),
                    User = user,
                    GpsLineString = topology.GpsPath,
                    GpsPolygon = topology.GpsPolygon,
                    Polygon = topology.AreaPolygon,
                    UploadTime = uploadTime,
                    Area = topology.AreaPolygon.Area,
                    Length = topology.LocalLinestring.Length,
                }
            ).ToList();

            // compute all the trips together
            var unifiedTripArea = tripTopologies.Aggregate(
                MultiPolygon.Empty as Geometry,
                (current, topologyInfo) =>
                    current.Union(topologyInfo.AreaPolygon));

            // add them to user overall area and update the amount
            user.OverallArea = user.OverallArea.Union(unifiedTripArea).AsMultiPolygon();
            user.OverallAreaAmount = user.OverallArea.Area;

            // update district caches
            // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
            var newAreaCacheEntries = _db.Districts.AsParallel().Select(district =>
                new DistrictAreaCacheEntry
                {
                    District = district,
                    DistrictId = district.Id,
                    User = user,
                    UserId = user.Id,
                    // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                    Area = district.Geometry.Intersection(user.OverallArea).Area,
                }
            );

            // purge all the previous ones for this user /shrug
            if (await _db.DistrictAreaCacheEntries.AnyAsync(x => x.UserId == user.Id))
            {
                await _db.DistrictAreaCacheEntries.Where(x => x.UserId == user.Id).ExecuteDeleteAsync(); 
            }
            // add new ones
            await _db.AddRangeAsync(newAreaCacheEntries);
            

            // save all trips
            await _db.AddRangeAsync(dbTrips);
            
            // achievement check
            await _db.Entry(user).Collection<Achievement>(x => x.Achievements).LoadAsync();
            var remainingAchievements = _db.Achievements.Where(a => !user.Achievements.Contains(a)).ToList();

            var newAchievements = _achievementMgr.CheckUser(remainingAchievements, user).ToList();
            await _db.AddRangeAsync(newAchievements);
            
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<IEnumerable<TripReturnDto>>(dbTrips));
        }
        catch (XmlException e)
        {
            //todo: make this not spit stack traces on prod
            return BadRequest($"invalid GPX syntax: {e}");
        }
    }

    [HttpGet("id/{guid:guid}")]
    public async Task<ActionResult<DetailedTripReturnDto>> GetById([FromRoute] Guid guid)
    {
        var trip = _db.Trips.FirstOrDefault(x => x.Id.Equals(guid));

        if (trip == null)
        {
            return NotFound();
        }
        
        var user = await _db.Entry(trip).Reference(x => x.User).Query().SimplifyUser().FirstAsync();
        _logger.LogDebug("got trip owner = {User}", user);
        trip.User = user;

        _db.Entry(trip).State = EntityState.Unchanged;
        _db.Entry(user).State = EntityState.Unchanged;

        await _db.SaveChangesAsync();
        
        return Ok(_mapper.Map<DetailedTripReturnDto>(trip));
    }
}