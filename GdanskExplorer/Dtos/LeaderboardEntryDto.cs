namespace GdanskExplorer.Dtos;

public class LeaderboardEntryDto<TV>
{
    public ShortUserReturnDto User { get; set; } = null!;
    public TV Value { get; set; } = default!;
}