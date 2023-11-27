namespace GdanskExplorer.Data;

public class AchievementGet
{
    public string AchievementId { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime TimeAchieved { get; set; }
}