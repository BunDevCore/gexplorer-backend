using GdanskExplorer.Data;

namespace GdanskExplorer.Achievements;

public interface IAchievable
{
    public string Id { get; }
    public string IconId { get; }
    public bool CheckUser(User user);

    public bool CheckTrip(Trip trip);
}