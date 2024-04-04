using AutoMapper;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GdanskExplorer.Controllers;

[Route("[controller]")]
public class AchievementController : ControllerBase
{
    private readonly GExplorerContext _db;
    private readonly IMapper _mapper;


    public AchievementController(GExplorerContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet("")]
    public IEnumerable<string> GetAll()
    {
        return _db.Achievements.Select(x => x.Id);
    }

    [HttpGet("{achievementId}")]
    public async Task<ActionResult<AchievementDetailsDto>> GetAchievementDetails(string achievementId)
    {
        var achievement = await _db.Achievements.FindAsync(achievementId);

        if (achievement is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AchievementDetailsDto>(achievement));
    }
    

}