using System.Linq.Expressions;
using AutoMapper;
using DotSpatial.Projections;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.Annotations;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly GExplorerContext _db;
    private readonly IMapper _mapper;
    private readonly DotSpatialReprojector _reproject;
    private readonly IOptions<AreaCalculationOptions> _areaOptions;

    public UserController(GExplorerContext db, IMapper mapper, IOptions<AreaCalculationOptions> areaOptions)
    {
        _db = db;
        _mapper = mapper;
        _areaOptions = areaOptions;
        _reproject = new DotSpatialReprojector(ProjectionInfo.FromEpsgCode(4326),
            ProjectionInfo.FromEpsgCode(_areaOptions.Value.CommonAreaSrid));
    }

    private async Task<ActionResult<UserReturnDto>> HandleSearch(Expression<Func<User, bool>> condition)
    {
        var user = await _db.Users
                    .Include(x =>
                        x.Trips.OrderByDescending(t => t.UploadTime))
                    .Include(x => x.DistrictAreas)
                    .Include(x => x.Achievements)
                    .SimplifyUser()
                    .Where(condition)
                    .FirstOrDefaultAsync();
                
        
                if (user is null)
                {
                    return NotFound();
                }
                
                foreach (var trip in user.Trips)
                {
                    trip.User = user;
                    _db.Entry(trip).State = EntityState.Unchanged; // make ef think nothing has changed because, well, it hasn't and i just know better, this all arises from SimplifyUser because i do not want the whole polygon loaded here
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
}