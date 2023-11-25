namespace GdanskExplorer.Dtos;

public class TripReturnDto
{
    public Guid Id { get; set; }
    public ShortUserReturnDto User { get; set; } = null!;
    public double Area { get; set; }
}