using AutoMapper;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GdanskExplorer.Controllers;

[ApiController]
[Route("[controller]")]
public class PlacesController : ControllerBase
{
    private readonly GExplorerContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    
    public PlacesController(GExplorerContext db, UserManager<User> userManager, IMapper mapper)
    {
        _db = db;
        _userManager = userManager;
        _mapper = mapper;
    }

    [Authorize]
    [HttpGet("self")]
    public async Task<ActionResult<IEnumerable<PlaceStateReturnDto>>> GetOwnPlaceStates()
    {
        var userId = _userManager.GetUserId(User)!;
        var user = await _db.Users.Include(x => x.PlaceRows).SimplifyUser()
            .Where(x => x.Id == Guid.Parse(userId)).FirstOrDefaultAsync();
        if (user == null) return Unauthorized();

        return Ok(_mapper.Map<List<PlaceStateReturnDto>>(user.PlaceRows));
    }
    
    [Authorize]
    [HttpGet("id/{id}")]
    public async Task<ActionResult<IEnumerable<PlaceStateReturnDto>>> GetPlaceStatesById(string id)
    {
        var userId = _userManager.GetUserId(User)!;
        var user = await _db.Users.Include(x => x.PlaceRows).SimplifyUser()
            .Where(x => x.Id == Guid.Parse(userId)).FirstOrDefaultAsync();
        if (user == null) return Unauthorized();
        
        var place = await _db.Places.FindAsync(id);
        if (place is null) return NotFound();

        var placeRow = user.PlaceRows.FirstOrDefault(x => x.PlaceId == id);
        if (placeRow is null)
        {
            return Ok(new PlaceStateReturnDto { Id = id, Saved = false, Visited = false });
        }
        
        return Ok(_mapper.Map<PlaceStateReturnDto>(placeRow));
    }
    
    
    [Authorize]
    [HttpPost("id/{id}")]
    public async Task<ActionResult> SetPlaceState(string id, [FromBody] PlaceStateDto placeState)
    {
        var place = await _db.Places.FindAsync(id);
        if (place is null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var placeRow = user.PlaceRows.FirstOrDefault(x => x.UserId == user.Id);
        if (placeRow is null)
        {
            user.PlaceRows.Add(new PlaceVisitedRow
            {
                PlaceId = id,
                UserId = user.Id,
                Saved = placeState.Saved,
                Visited = placeState.Visited
            });
        }
        else
        {
            placeRow.Saved = placeState.Saved;
            placeRow.Visited = placeState.Visited;
        }

        await _db.SaveChangesAsync();

        return Ok();
    }
}