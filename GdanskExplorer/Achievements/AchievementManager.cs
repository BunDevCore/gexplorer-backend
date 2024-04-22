using GdanskExplorer.Data;

namespace GdanskExplorer.Achievements;

public class AchievementManager
{
    private readonly GExplorerContext _db;

    public AchievementManager(GExplorerContext db)
    {
        _db = db;
    }

    // possible custom logic for mapping achievement ids to specific classes will go in this method later
    // if found in db (a prerequisite because of foreign keys and stuff), just returns the plain target IAchievable or the always false one (for now!) 
    public IAchievable? GetById(string id) => _db.Achievements.Find(id)?.ToIAchievable();
    public async Task<IAchievable?> GetByIdAsync(string id) => (await _db.Achievements.FindAsync(id))?.ToIAchievable();

    public async Task<Achievement?> GetEntity(string id) => await _db.Achievements.FindAsync(id);

    /// <summary>
    /// Caller's responsibility to ensure that none of the achievements here have already been gotten!
    /// </summary>
    /// <param name="achievements">list of achievements to check against</param>
    /// <param name="user">user being checked</param>
    /// <returns>IEnumerable of AchievementGets which *do not have AchievedOnTripId* set!</returns>
    public IEnumerable<AchievementGet> CheckUserUnchecked(List<Achievement> achievements, User user)
    {
        // plain geometry achievements, do not need to go through any possible custom logic and can just call ToIAchievable to avoid additional queries
        var geometryAchievements = achievements
            .AsParallel()
            .Where(a => a.Target is not null)
            .Select(x => new { achievable = x.ToIAchievable(), entity = x })
            .Where(x => x.achievable.CheckUser(user));
        
        // achievements with custom logic: target is null, might need id
        var customAchievements = achievements
            .Where(a => a.Target is null)
            .Select(x => new { achievable = GetById(x.Id), entity = x })
            .AsParallel() // only here to avoid possible parallel querying on the DbContext all this uses
            .Where(x => x.achievable != null)
            .Where(x => x.achievable!.CheckUser(user)); // guaranteed to not be null because of line above

        var achieved = geometryAchievements.Concat(customAchievements!) // same deal
            .Select(x => x.entity).Distinct();

        return achieved.Select(a => new AchievementGet
        {
            AchievementId = a.Id,
            User = user,
            TimeAchieved = DateTime.UtcNow,
            UserId = user.Id
        });
    }

    public IEnumerable<AchievementGet> CheckUser(User user)
    {
        return CheckUserUnchecked(_db.Achievements.Where(x => !user.Achievements.Contains(x)).ToList(), user);
    }
    
    public IEnumerable<AchievementGet> CheckTrip(List<Achievement> achievements, Trip trip)
    {
        // plain geometry achievements, do not need to go through any possible custom logic and can just call ToIAchievable to avoid additional queries
        var geometryAchievements = achievements
            .AsParallel()
            .Where(a => a.Target is not null)
            .Select(x => new { achievable = x.ToIAchievable(), entity = x })
            .Where(x => x.achievable.CheckTrip(trip));
        
        // achievements with custom logic: target is null, might need id
        var customAchievements = achievements
            .Where(a => a.Target is null)
            .Select(x => new { achievable = GetById(x.Id), entity = x })
            .AsParallel() // only here to avoid possible parallel querying on the DbContext all this uses
            .Where(x => x.achievable != null)
            .Where(x => x.achievable!.CheckTrip(trip)); // guaranteed to not be null because of line above

        var achieved = geometryAchievements.Concat(customAchievements!) // same deal
            .Select(x => x.entity).Distinct();

        return achieved.Select(a => new AchievementGet
        {
            AchievementId = a.Id,
            User = trip.User,
            TimeAchieved = DateTime.UtcNow,
            UserId = trip.User.Id
        });
    }
}

internal static class AchievementExtensions
{
    public static IAchievable ToIAchievable(this Achievement a)
    {
        if (a.Target is null)
        {
            return new UnachievableAchievement
            {
                // add icon id here when it's in db
                Id = a.Id
            };
        }

        // add icon id here when it's in db
        return new GeometryVisitedAchievement(a.Id, a.Target);
    }
}