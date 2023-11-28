using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Execution;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Zomp.EFCore.WindowFunctions;

namespace GdanskExplorer.Controllers;

[ApiController]
[Route("[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly GExplorerLeaderboardContext _db;
    private readonly ILogger<LeaderboardController> _logger;
    private readonly IMapper _mapper;

    public const int PageSize = 30;

    public LeaderboardController(GExplorerLeaderboardContext db, ILogger<LeaderboardController> logger, IMapper mapper)
    {
        _db = db;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("overall")]
    public Dictionary<long, LeaderboardEntryDto<double>> OverallAreaNoPage()
    {
        return OverallAreaWithPage(1);
    }


    [HttpGet("overall/{page:int}")]
    public Dictionary<long, LeaderboardEntryDto<double>> OverallAreaWithPage(int page)
    {
        var leaderboard = _db.Users.SimplifyUser()
            .Select(x => new LeaderboardEntry<double, ShortUserReturnDto>
            {
                Value = x.OverallAreaAmount,
                Inner = _mapper.Map<ShortUserReturnDto>(x),
                Rank = EF.Functions.Rank(EF.Functions.Over()
                    .OrderByDescending(x.OverallAreaAmount))
            }).Page(PageSize, page);

        return leaderboard.ToDictionary(x => x.Rank,
            x => _mapper.Map<LeaderboardEntryDto<double>>(x));
    }

    [HttpGet("district/{id:guid}")]
    public async Task<ActionResult<Dictionary<long, LeaderboardEntryDto<double>>>> PerDistrictNoPage(Guid id)
    {
        return await PerDistrictWithPage(id, 1);
    }
    
    [HttpGet("district/{id:guid}/{page:int}")]
    public async Task<ActionResult<Dictionary<long, LeaderboardEntryDto<double>>>> PerDistrictWithPage(Guid id,
        int page)
    {
        var district = await _db.Districts.FindAsync(id);

        if (district is null)
        {
            return NotFound();
        }

        var leaderboard = _db.Users
            .Include(x => x.DistrictAreas) // include district area navigation
            .SimplifyUser() // do not query every single field ever, *especially* OverallArea to save on bandwidth and db performance
            .Select(x => new LeaderboardEntry<double, ShortUserReturnDto>
            {
                Value =
                    x.DistrictAreas
                        .Where(dace => dace.DistrictId == id) // include only relevant entries
                        .Sum(dace => dace.Area), // there's only one, but it plays nicely with ef core to just sum them
                Inner = _mapper.Map<ShortUserReturnDto>(x), // map user object
                Rank = EF.Functions.Rank(EF.Functions.Over()
                    .OrderByDescending(x.DistrictAreas
                        .Where(dace => dace.DistrictId == id) // same deal as before
                        .Sum(dace => dace.Area)))
            }).Page(PageSize, page);

        return leaderboard.ToDictionary(x => x.Rank,
            x => _mapper.Map<LeaderboardEntryDto<double>>(x));
    }
}