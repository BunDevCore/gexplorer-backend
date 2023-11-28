using Microsoft.EntityFrameworkCore;

namespace GdanskExplorer.Data;


/// <summary>
/// Do not use this, except in LeaderboardController. This context is a dirty workaround for the fact that the EF Core
/// extension I used for SQL rank() breaks delete operations :/
/// </summary>
public class GExplorerLeaderboardContext : GExplorerContext
{
    public GExplorerLeaderboardContext(DbContextOptions<GExplorerLeaderboardContext> options) : base(options)
    {
    }
}