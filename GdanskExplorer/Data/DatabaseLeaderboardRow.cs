namespace GdanskExplorer.Data;

public class DatabaseLeaderboardRow
{
    public long Rank { get; set; }
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public DateTime JoinedAt { get; set; }
    public double OverallAreaAmount { get; set; }
    public double Value { get; set; }
}