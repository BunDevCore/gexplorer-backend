namespace GdanskExplorer.Dtos;

public class UserReturnDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public double OverallAreaAmount { get; set; }
    public DateTime JoinedAt { get; set; }
    public List<TripReturnDto> Trips { get; set; } = new();
    public int TripAmount { get; set; }
    public double TotalTripLength { get; set; }
    public Dictionary<Guid, double> DistrictAreas { get; set; } = null!;
    public List<AchievementGetDto> Achievements { get; set; } = null!;
}