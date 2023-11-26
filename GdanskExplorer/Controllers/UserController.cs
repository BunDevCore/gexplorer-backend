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

    [HttpGet("{username}")]
    public async Task<UserReturnDto> GetByUsername(string username)
    {
        var user = await _db.Users
            .Include(x =>
                x.Trips.OrderByDescending(t => t.UploadTime))
            .Include(x => x.DistrictAreas)
            .Select(x => new User
            {
                Id = x.Id, Trips = x.Trips, DistrictAreas = x.DistrictAreas, UserName = x.UserName,
                JoinedAt = x.JoinedAt, OverallAreaAmount = x.OverallAreaAmount
            })
            .Where(x => x.UserName == username)
            .FirstOrDefaultAsync();
        return _mapper.Map<UserReturnDto>(user);
    }
    
    [HttpGet("id/{id:guid}")]
    public async Task<UserReturnDto> GetById(Guid id)
    {
        var user = await _db.Users
            .Include(x =>
                x.Trips.OrderByDescending(t => t.UploadTime))
            .Include(x => x.DistrictAreas)
            .Select(x => new User
            {
                Id = x.Id, Trips = x.Trips, DistrictAreas = x.DistrictAreas, UserName = x.UserName,
                JoinedAt = x.JoinedAt, OverallAreaAmount = x.OverallAreaAmount
            })
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        return _mapper.Map<UserReturnDto>(user);
    }
}