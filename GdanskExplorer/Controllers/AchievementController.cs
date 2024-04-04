using AutoMapper;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class AchievementController : ControllerBase
{
    private readonly GExplorerContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;



    public AchievementController(GExplorerContext db, IMapper mapper, UserManager<User> userManager)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
    }

    [HttpGet("")]
    public IEnumerable<string> GetAll()
    {
        return _db.Achievements.Where(x => !x.IsSecret).Select(x => x.Id);
    }

    [HttpGet("{achievementId}")]
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
    

}