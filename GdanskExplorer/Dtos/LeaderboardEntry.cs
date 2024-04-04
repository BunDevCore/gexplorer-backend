namespace GdanskExplorer.Dtos;

public class LeaderboardEntry<TV, TUser>
{
    public TUser Inner { get; set; } = default!;
    public long Rank { get; set; }

    public TV Value { get; set; } = default!;

    public override string ToString()
    {
        return $"{nameof(Inner)}: {Inner}, {nameof(Rank)}: {Rank}";
    }
}