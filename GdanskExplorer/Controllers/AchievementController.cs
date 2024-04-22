using System.Collections.Immutable;
using System.Text.Json;
using AutoMapper;
using DotSpatial.Projections;
using GdanskExplorer.Achievements;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class AchievementController : ControllerBase
{
    private readonly GExplorerContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly DotSpatialReprojector _reproject;
    private readonly AchievementManager _achievementManager;
    private readonly ILogger<AchievementController> _logger;


    public AchievementController(GExplorerContext db, IMapper mapper, UserManager<User> userManager,
        IOptions<AreaCalculationOptions> options, AchievementManager achievementManager,
        ILogger<AchievementController> logger)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
        _achievementManager = achievementManager;
        _logger = logger;
        _reproject = new DotSpatialReprojector(ProjectionInfo.FromEpsgCode(4326),
            ProjectionInfo.FromEpsgCode(options.Value.CommonAreaSrid));
    }

    [HttpGet("")]
    public async Task<AchievementListDto> GetAll()
    {
        var achievements = await _db.Achievements.Where(x => !x.IsSecret).Select(x => x.Id)
            .ToListAsync();
        return new AchievementListDto
        {
            Achievements = achievements,
            AchievementCount = achievements.Count
        };
    }

    [HttpGet("recent")]
    public ActionResult<AchievementGetDto> GetRecentAchievements()
    {
        var query = _db.AchievementGets
            .Include(x => x.Achievement)
            .Where(x => !x.Achievement.IsSecret)
            .OrderByDescending(x => x.TimeAchieved)
            .Take(10);
        return Ok(_mapper.ProjectTo<AchievementGetDto>(query));
    }

    [HttpGet("id/{achievementId}")]
    public async Task<ActionResult<AchievementDetailsDto>> GetAchievementDetails(string achievementId)
    {
        var achievement = await _db.Achievements.FindAsync(achievementId);

        if (achievement is null)
        {
            return NotFound();
        }

        // regular achievement, just go return it like a sane person
        if (!achievement.IsSecret) return Ok(_mapper.Map<AchievementDetailsDto>(achievement));

        // shh! don't show secret achievements to someone who hasn't already gotten them...
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User) ?? string.Empty);
            await _db.Entry(achievement).Collection(x => x.AchievementGets).LoadAsync();
            if (achievement.AchievementGets.Any(x => x.UserId == userId))
            {
                return Ok(_mapper.Map<AchievementDetailsDto>(achievement));
            }

            return NotFound();
        }
        catch
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("import")]
    public async Task<ActionResult<IEnumerable<string>>> ImportAchievements([FromBody] JsonElement json)
    {
        var bodyString = json.GetRawText();
        var reader = new GeoJsonReader();
        FeatureCollection? fc;
        try
        {
            fc = reader.Read<FeatureCollection>(bodyString);
        }
        catch (JsonReaderException e)
        {
            _logger.LogError("bad geojson submitted to achievement import, {Exception}", e);
            return BadRequest("bad GeoJSON body!");
        }

        if (fc is null)
        {
            _logger.LogError("achievement import feature collection read failed for unknown reason!");
            return BadRequest("could not read feature collection for unknown reason");
        }

        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            var achievements = fc.AsParallel().Select(FeatureToAchievement).ToList();
            var dbAchievements = await _db.Achievements.ToListAsync();
            // there could be a hashset of achievement names but for how many achievements there actually are in the db this would be just unnecessary complexity

            foreach (var achievement in achievements)
            {
                if (dbAchievements.FirstOrDefault(x => x.Id == achievement.Id) is { } a)
                {
                    a.IsSecret = achievement.IsSecret;
                    a.Target = achievement.Target;
                }
                else
                {
                    _db.Achievements.Add(achievement);
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("New achievements successfully saved to db!");

            var users = await _db.Users.ToListAsync();
            _logger.LogInformation("recomputing achievements for all users... {NumChecks} to do",
                achievements.Count * users.Count);

            var achievementGets = users.AsParallel().SelectMany(user =>
                {
                    _logger.LogDebug("recomputing achievements for {User}", user.UserName);
                    return _achievementManager.CheckUserUnchecked(achievements, user);
                }
            );

            var allGets = _db.AchievementGets.Select(x => new { x.UserId, x.AchievementId }).ToImmutableHashSet();
            _db.AchievementGets.AddRange(achievementGets
                .Where(ag => !allGets.Contains(new {ag.UserId, ag.AchievementId})));

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(_db.Achievements.Select(x => x.Id));
        }
        catch (InvalidDataException e)
        {
            _logger.LogError("error converting feature to achievement, {Exception}", e);
            return BadRequest(e.Message);
        }
    }

    private Achievement FeatureToAchievement(IFeature feat)
    {
        var maybeId = feat.Attributes.GetOptionalValue("id");
        if (maybeId is not string id)
        {
            throw new InvalidDataException($"id attribute is not present or not an object; id={maybeId}");
        }

        var secretAttr = feat.Attributes.GetOptionalValue("secret");
        var isSecret = secretAttr is "secret";

        var geometry = feat.Geometry.Copy();
        geometry.Apply(_reproject.InputSwizzled()
            .InputSwizzled()); // huh /?????????? it ONLY works with TWO swizzles which should be IDENTICAL ?????????????????? THIS IS BLACK MAGIC AND ITS 3AM AND I WANT TO GO TO SLEEP

        return new Achievement
        {
            Id = id,
            Target = geometry,
            IsSecret = isSecret
        };
    }
}