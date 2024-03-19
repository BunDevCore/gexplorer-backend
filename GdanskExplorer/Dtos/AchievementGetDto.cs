namespace GdanskExplorer.Dtos;

public class AchievementGetDto
{
    public string AchievementId { get; set; } = null!;
    public DateTime TimeAchieved { get; set; }
    public Guid AchievedOnTripId { get; set; }
}