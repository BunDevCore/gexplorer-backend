namespace GdanskExplorer.Data;

public class Place
{
    public string Id { get; set; }
    public List<User> Users { get; set; } = new();
}