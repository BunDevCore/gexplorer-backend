namespace GdanskExplorer.Data;

public class AchievementGet
{
    public string AchievementId { get; set; }
    public Achievement Achievement { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime TimeAchieved { get; set; }

    public Trip AchievedOnTrip { get; set; } = null!;
    
    public Guid AchievedOnTripId { get; set; }
}