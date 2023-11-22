namespace GdanskExplorer;

public class AreaCalculationOptions
{
    public const string SectionName = "AreaCalc";

    public int CommonAreaSrid { get; set; } = 6933;
    public string BufferProj4 { get; set; } = null!;
    public double BufferRadius { get; set; } = 7.0;
    public double SimplificationDistanceSquared { get; set; } = 25.0;
}