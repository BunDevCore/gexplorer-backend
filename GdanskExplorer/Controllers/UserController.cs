using System.Linq.Expressions;
using AutoMapper;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly GExplorerContext _db;
    private readonly IMapper _mapper;

    public UserController(GExplorerContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<ActionResult<UserReturnDto>> HandleSearch(Expression<Func<User, bool>> condition)
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
}