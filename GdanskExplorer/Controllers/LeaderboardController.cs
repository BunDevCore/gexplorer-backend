using AutoMapper;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public Task<Dictionary<long, LeaderboardEntryDto<double>>> OverallAreaNoPage()
    {
        return OverallAreaWithPage(1);
    }


    [HttpGet("overall/{page:int}")]
    public async Task<Dictionary<long, LeaderboardEntryDto<double>>> OverallAreaWithPage(int page)
    {
        _logger.LogDebug("getting overall leaderboard page {Page}", page);

        FormattableString query = $"""
                                   SELECT LD.R as "Rank", LD."Id", LD."UserName", LD."OverallAreaAmount", LD."OverallAreaAmount" as "Value", LD."JoinedAt" FROM (
                                       SELECT a0."Id", a0."JoinedAt", a0."UserName", a0."OverallAreaAmount", RANK() OVER(ORDER BY a0."OverallAreaAmount" DESC, a0."Id" DESC) as R
                                       FROM "AspNetUsers" AS a0
                                   ) as LD
                                   """;
        
        var leaderboard = await _db.Set<DatabaseLeaderboardRow>()
            .FromSql(query)
            .Select(x => new LeaderboardEntry<double, ShortUserReturnDto>
            {
                Inner = _mapper.Map<ShortUserReturnDto>(x),
                Rank = x.Rank,
                Value = x.Value
            })
            .ToDictionaryAsync(x => x.Rank, x =>
            _mapper.Map<LeaderboardEntryDto<double>>(x));

        return leaderboard;
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
        _logger.LogDebug("getting district {District} leaderboard page {Page}", id, page);
        
        var district = await _db.Districts.FindAsync(id);

        if (district is null)
        {
            return NotFound();
        }

        FormattableString query = $"""
                                   SELECT LD."R" as "Rank", LD."Id", LD."UserName", LD."OverallAreaAmount", LD."Area" as "Value", LD."JoinedAt"
                                   FROM (SELECT U."Id", U."UserName", U."OverallAreaAmount", U."JoinedAt", "Area",
                                                rank() over (order by "Area" DESC NULLS LAST, "OverallAreaAmount", "Id")                       as "R"
                                         from "AspNetUsers" as U
                                                  LEFT JOIN "DistrictAreaCacheEntries" as DACE
                                                            on DACE."UserId" = U."Id" and
                                                               DACE."DistrictId" = {district.Id}) as LD
                                   """;
        
        var leaderboard = await _db.Set<DatabaseLeaderboardRow>()
            .FromSql(query)
            .Select(x => new LeaderboardEntry<double, ShortUserReturnDto>
            {
                Inner = _mapper.Map<ShortUserReturnDto>(x),
                Rank = x.Rank,
                Value = x.Value
            })
            .ToDictionaryAsync(x => x.Rank, x =>
                _mapper.Map<LeaderboardEntryDto<double>>(x));

        // var leaderboard = _db.Users
        //     .Include(x => x.DistrictAreas) // include district area navigation
        //     .SimplifyUser() // do not query every single field ever, *especially* OverallArea to save on bandwidth and db performance
        //     .Select(x => new LeaderboardEntry<double, ShortUserReturnDto>
        //     {
        //         Value =
        //             x.DistrictAreas
        //                 .Where(dace => dace.DistrictId == id) // include only relevant entries
        //                 .Sum(dace => dace.Area), // there's only one, but it plays nicely with ef core to just sum them
        //         Inner = _mapper.Map<ShortUserReturnDto>(x), // map user object
        //         Rank = EF.Functions.Rank(EF.Functions.Over()
        //             .OrderByDescending(x.DistrictAreas
        //                 .Where(dace => dace.DistrictId == id) // same deal as before
        //                 .Sum(dace => dace.Area) + x.LeaderboardBias)
        //             .ThenBy(x.Id))
        //     }).Page(PageSize, page);

        return leaderboard;
    }

    [HttpGet("overall/{userId:guid}")]
    public async Task<ActionResult<LeaderboardRankDto>> GetOverallRankForId(Guid userId)
    {
        FormattableString query = $"""
                                    SELECT LD.R as "Value" FROM (
                                        SELECT a0."Id", RANK() OVER(ORDER BY a0."OverallAreaAmount" DESC, a0."Id" DESC) as R
                                        FROM "AspNetUsers" AS a0
                                    ) as LD
                                    WHERE LD."Id" = {userId}
                                   """;

        try
        {
            var rank = await _db.Database.SqlQuery<long>(query).FirstAsync();
            return new LeaderboardRankDto {Rank = rank};
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
    
    [HttpGet("district/{districtId:guid}/{userId:guid}")]
    public async Task<ActionResult<LeaderboardRankDto>> GetDistrictRankForId(Guid districtId, Guid userId)
    {
        var district = await _db.Districts.FindAsync(districtId);

        if (district is null)
        {
            return NotFound();
        }
        
        FormattableString query = $"""
                                    SELECT LD."R" as "Value"
                                   FROM (SELECT U."Id"                                                              as "UserId",
                                                rank() over (order by "Area" DESC NULLS LAST)                       as "R"
                                         from "AspNetUsers" as U
                                                  LEFT JOIN "DistrictAreaCacheEntries" as DACE
                                                            on DACE."UserId" = U."Id" and
                                                               DACE."DistrictId" = {districtId}) as LD
                                   where LD."UserId" = {userId}
                                   """;

        try
        {
            var rank = await _db.Database.SqlQuery<long>(query).FirstAsync();
            return new LeaderboardRankDto {Rank = rank};
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}

public class LeaderboardRankDto
{
    public long Rank { get; set; }
}