using System.Linq.Expressions;
using AutoMapper;
using DotSpatial.Projections;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly GExplorerContext _db;
    private readonly IMapper _mapper;
    private readonly DotSpatialReprojector _reproject;

    public UserController(GExplorerContext db, IMapper mapper, IOptions<AreaCalculationOptions> areaOptions)
    {
        _db = db;
        _mapper = mapper;
        _reproject = new DotSpatialReprojector(ProjectionInfo.FromEpsgCode(4326),
            ProjectionInfo.FromEpsgCode(areaOptions.Value.CommonAreaSrid));
    }

    //TODO: this code is really stinky, why?
    private async Task<ActionResult<UserReturnDto>> HandleSearch(Expression<Func<User, bool>> condition)
    {
        var user = await _db.Users.Where(condition).FirstOrDefaultAsync();


        if (user is null)
        {
            return NotFound();
        }

        await _db.Entry(user).Collection(x => x.DistrictAreas).LoadAsync();
        await _db.Entry(user).Collection(x => x.Trips).LoadAsync();
        await _db.Entry(user).Collection(x => x.Achievements).LoadAsync();
        await _db.Entry(user).Collection(x => x.AchievementGets).LoadAsync();
        

        foreach (var trip in user.Trips)
        {
            trip.User = user;
            _db.Entry(trip).State =
                EntityState
                    .Unchanged; // make ef think nothing has changed because, well, it hasn't and i just know better, this all arises from SimplifyUser because i do not want the whole polygon loaded here
        }

        return Ok(_mapper.Map<UserReturnDto>(user));
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<UserReturnDto>> GetByUsername(string username) =>
        await HandleSearch(x => x.UserName == username);

    [HttpGet("id/{id:guid}")]
    public async Task<ActionResult<UserReturnDto>> GetById(Guid id) =>
        await HandleSearch(x => x.Id == id);

    [HttpGet("id/{id:guid}/polygon")]
    public async Task<ActionResult<Geometry>> GetPolygonForId(Guid id)
    {
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var user = await _db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        var poly = user.OverallArea.Copy();
        poly.Apply(_reproject.Reversed());
        return Ok(poly);
    }

    [HttpGet("id/{username}/polygon")]
    public async Task<ActionResult<Geometry>> GetPolygonForUsername(string username)
    {
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var user = await _db.Users.Where(x => x.UserName == username).FirstOrDefaultAsync();
        if (user is null)
        {
            return NotFound();
        }

        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        var poly = user.OverallArea.Copy();
        poly.Apply(_reproject.Reversed());
        return Ok(poly);
    }
}