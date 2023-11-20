namespace GdanskExplorer.Dtos;

public class UserReturnDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}