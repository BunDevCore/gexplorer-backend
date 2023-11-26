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
    private readonly GExplorerContext _db;
    private readonly ILogger<LeaderboardController> _logger;
    private readonly IMapper _mapper;

    public const int PageSize = 30;

    public LeaderboardController(GExplorerContext db, ILogger<LeaderboardController> logger, IMapper mapper)
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
}