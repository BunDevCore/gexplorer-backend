namespace GdanskExplorer.Data;

public class Achievement
{
    public string Id { get; set; }
    public List<AchievementGet> AchievementGets { get; set; }
    public List<User> Achievers { get; set; } = new();
}