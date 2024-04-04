namespace GdanskExplorer.Dtos;

public class AchievementGetDto
{
    public ShortUserReturnDto User { get; set; } = null!;
    public string AchievementId { get; set; } = null!;
    public DateTime TimeAchieved { get; set; }
    public Guid AchievedOnTripId { get; set; }
}