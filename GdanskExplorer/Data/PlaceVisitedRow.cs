namespace GdanskExplorer.Data;

public class PlaceVisitedRow
{
    public Place Place { get; set; }
    public User User { get; set; }
    
    public string PlaceId { get; set; }
    public Guid UserId { get; set; }
    
    public bool Saved { get; set; }
    public bool Visited { get; set; }
}