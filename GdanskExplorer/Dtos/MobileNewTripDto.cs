namespace GdanskExplorer.Controllers;

public class MobileNewTripDto
{
    public List<MobileTripPoint> Points { get; set; } = new();
}

public class MobileTripPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public long Timestamp { get; set; }
}