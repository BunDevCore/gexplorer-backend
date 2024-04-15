using System.Text.Json;
using AutoMapper;
using DotSpatial.Projections;
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


    public AchievementController(GExplorerContext db, IMapper mapper, UserManager<User> userManager, IOptions<AreaCalculationOptions> options)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
        _reproject = new DotSpatialReprojector(ProjectionInfo.FromEpsgCode(4326),
            ProjectionInfo.FromEpsgCode(options.Value.CommonAreaSrid));
    }

    [HttpGet("")]
    public IEnumerable<string> GetAll()
    {
        return _db.Achievements.Where(x => !x.IsSecret).Select(x => x.Id);
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
        // var bodyString = await new StreamReader(HttpContext.Request.BodyReader.AsStream()).ReadToEndAsync();
        var reader = new GeoJsonReader();
        FeatureCollection? fc;
        try
        {
            fc = reader.Read<FeatureCollection>(bodyString);
        }
        catch (JsonReaderException)
        {
            return BadRequest("bad GeoJSON body!");
        }
        
        if (fc is null)
        {
            return BadRequest("could not read feature collection for unknown reason");
        }

        try
        {
            var achievements = fc.AsParallel().Select(FeatureToAchievement);
            await _db.Achievements.AddRangeAsync(achievements);
            await _db.SaveChangesAsync();
        
            return Ok(_db.Achievements.Select(x => x.Id));
        }
        catch (InvalidDataException e)
        {
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
        geometry.Apply(_reproject.Reversed());

        return new Achievement
        {
            Id = id,
            Target = geometry,
            IsSecret = isSecret
        };
    }
}