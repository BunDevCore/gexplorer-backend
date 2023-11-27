namespace GdanskExplorer.Dtos;

public class ShortUserReturnDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public double OverallAreaAmount { get; set; }
}