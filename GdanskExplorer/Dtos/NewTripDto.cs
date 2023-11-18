namespace GdanskExplorer.Dtos;

public class NewTripDto
{
    public Guid? User { get; set; }
    public string GpxContents { get; set; } = null!;
}