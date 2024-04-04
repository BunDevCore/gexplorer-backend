namespace GdanskExplorer.Dtos;

public class TripReturnDto
{
    public Guid Id { get; set; }
    public ShortUserReturnDto User { get; set; } = null!;
    public double Area { get; set; }
    public double Length { get; set; }
    public double NewArea { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime UploadTime { get; set; }
}