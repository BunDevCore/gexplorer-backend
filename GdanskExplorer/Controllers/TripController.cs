using System.Xml;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    public TripController(ILogger<TripController> logger, UserManager<User> userManager, GExplorerContext db,
        GpxAreaExtractor areaExtractor)
    {
        _logger = logger;
        _userManager = userManager;
        _db = db;
        _areaExtractor = areaExtractor;
    }

    [HttpPost("new")]
    public async Task<IActionResult> AddNewTrip([FromBody] NewTripDto newTrip)
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
            return Unauthorized("cannot add trip to another user");
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
                    Length = -1, // not sure how to calculate it rn
                }
            );

            await _db.AddRangeAsync(dbTrips);
            await _db.SaveChangesAsync();
        }
        catch (XmlException e)
        {
            return BadRequest($"invalid GPX syntax: {e}");
        }

        return Ok();
    }
}