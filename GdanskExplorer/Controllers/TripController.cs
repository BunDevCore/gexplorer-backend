using System.Xml;
using AutoMapper;
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
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<TripController> _logger;
    private readonly GExplorerContext _db;
    private readonly UserManager<User> _userManager;
    private readonly GpxAreaExtractor _areaExtractor;
    private readonly IMapper _mapper;

    public TripController(ILogger<TripController> logger, UserManager<User> userManager, GExplorerContext db,
        GpxAreaExtractor areaExtractor, IMapper mapper)
    {
        _logger = logger;
        _userManager = userManager;
        _db = db;
        _areaExtractor = areaExtractor;
        _mapper = mapper;
    }

    [HttpPost("new")]
    public async Task<ActionResult<IEnumerable<TripReturnDto>>> AddNewTrip([FromBody] NewTripDto newTrip)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if (newTrip.User is not null &&
            newTrip.User != user.Id &&
            !await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return Forbid();
        }

        try
        {
            // todo: run this on another thread
            // todo: update district caches
            var tripTopologies = _areaExtractor.ProcessGpx(newTrip.GpxContents.AsUtf8Stream());
            var uploadTime = DateTime.UtcNow;
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

            var unifiedTripArea = tripTopologies.Aggregate(
                MultiPolygon.Empty as Geometry,
                (current, topologyInfo) =>
                    current.Union(topologyInfo.AreaPolygon));

            user.OverallArea = user.OverallArea.Union(unifiedTripArea).AsMultiPolygon();
            
            await _db.AddRangeAsync(dbTrips);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<IEnumerable<TripReturnDto>>(dbTrips));
        }
        catch (XmlException e)
        {
            return BadRequest($"invalid GPX syntax: {e}");
        }
    }

    [HttpGet("id/{guid:guid}")]
    public ActionResult<DetailedTripReturnDto> GetById([FromRoute] Guid guid)
    {
        var trip = _db.Trips.FirstOrDefault(x => x.Id.Equals(guid));

        if (trip == null)
        {
            return NotFound();
        }
        
        return Ok(_mapper.Map<DetailedTripReturnDto>(trip));
    }
    
}