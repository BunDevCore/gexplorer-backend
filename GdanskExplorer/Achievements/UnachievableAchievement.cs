using GdanskExplorer.Data;

namespace GdanskExplorer.Achievements;

public class UnachievableAchievement : IAchievable
{
    public string Id { get; set; } = "null";
    public string IconId { get; set; } = "default";
    
    public bool CheckUser(User user) => false;

    public bool CheckTrip(Trip trip) => false;
}