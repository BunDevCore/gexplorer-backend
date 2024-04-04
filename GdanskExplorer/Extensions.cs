using System.Text;
using GdanskExplorer.Data;
using NetTopologySuite.Geometries;

namespace GdanskExplorer;

public static class Extensions
{
    public static Stream AsUtf8Stream(this string? s)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(s ?? ""));
    }

    public static MultiPolygon AsMultiPolygon(this Geometry g) =>
        g switch
        {
            MultiPolygon multiPolygon => multiPolygon,
            Polygon polygon => new MultiPolygon(new[] { polygon }),
            _ => throw new ArgumentOutOfRangeException(nameof(g))
        };

    public static async Task<IEnumerable<T1>> SelectManyAsync<T, T1>(this IEnumerable<T> enumeration,
        Func<T, Task<IEnumerable<T1>>> func) => (await Task.WhenAll(enumeration.Select(func))).SelectMany(s => s);

    public static IEnumerable<T> Page<T>(this IEnumerable<T> e, int pageSize, int page) =>
        e.Skip(pageSize * page - pageSize).Take(pageSize);

    public static IQueryable<T> Page<T>(this IQueryable<T> e, int pageSize, int page) =>
        e.Skip(pageSize * page - pageSize).Take(pageSize);

    public static IQueryable<User> SimplifyUser(this IQueryable<User> q) => q
        .Select(x =>
            new User
            {
                Id = x.Id,
                UserName = x.UserName,
                JoinedAt = x.JoinedAt,
                OverallAreaAmount = x.OverallAreaAmount,
                DistrictAreas = x.DistrictAreas,
                Achievements = x.Achievements,
                AchievementGets = x.AchievementGets,
                Trips = x.Trips
            });
}