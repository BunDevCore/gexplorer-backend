namespace GdanskExplorer.Dtos;

public class LeaderboardEntry<V, T>
{
    public T Inner { get; set; } = default!;
    public long Rank { get; set; }

    public V Value { get; set; } = default!;

    public override string ToString()
    {
        return $"{nameof(Inner)}: {Inner}, {nameof(Rank)}: {Rank}";
    }
}