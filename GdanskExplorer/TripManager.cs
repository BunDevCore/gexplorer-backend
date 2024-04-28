using AutoMapper;
using GdanskExplorer.Achievements;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;
using GdanskExplorer.Topology;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace GdanskExplorer;

/// <summary>
/// This class abstracts away trip management - adding, removing, recomputing and so on
/// </summary>
public class TripManager
{
    private readonly ILogger<TripManager> _logger;
    private readonly GExplorerContext _db;
    private readonly UserManager<User> _userManager;
    private readonly GpxAreaExtractor _areaExtractor;
    private readonly IMapper _mapper;
    private readonly AchievementManager _achievementMgr;

    public TripManager(GExplorerContext db, ILogger<TripManager> logger, UserManager<User> userManager,
        GpxAreaExtractor areaExtractor, IMapper mapper, AchievementManager achievementMgr)
    {
        _db = db;
        _logger = logger;
        _userManager = userManager;
        _areaExtractor = areaExtractor;
        _mapper = mapper;
        _achievementMgr = achievementMgr;
    }

    public async Task<List<Trip>> AddTrip(User user, NewTripDto newTrip)
    {
        // todo: run this on another thread

            // black box magic
            var tripTopologies = _areaExtractor.ProcessGpx(newTrip.GpxContents.AsUtf8Stream());
            var uploadTime = DateTime.UtcNow;

            // compute all the trips together
            var unifiedTripArea = tripTopologies.Aggregate(
                MultiPolygon.Empty as Geometry,
                (current, topologyInfo) =>
                    current.Union(topologyInfo.AreaPolygon));

            // add them to user overall area and update the amount
            var oldOverallArea = user.OverallArea;
            user.OverallArea = user.OverallArea.Union(unifiedTripArea).AsMultiPolygon();
            user.OverallAreaAmount = user.OverallArea.Area;

            // convert to database entities
            var dbTrips = tripTopologies.Select(topology =>
                TopologyToTripEntity(user, topology, uploadTime, oldOverallArea)
            ).ToList();

            var newAreaCacheEntries = UpdateDistrictAreas(user);

            // purge all the previous ones for this user /shrug
            if (await _db.DistrictAreaCacheEntries.AnyAsync(x => x.UserId == user.Id))
            {
                await _db.DistrictAreaCacheEntries.Where(x => x.UserId == user.Id).ExecuteDeleteAsync();
            }

            // add new ones
            _db.AddRange(newAreaCacheEntries);


            // save all trips
            _db.AddRange(dbTrips);

            // achievement check
            var newAchievements = await GetNewAchievements(user, dbTrips);
            _db.AddRange(newAchievements);

            await _db.SaveChangesAsync();
            return dbTrips;
    }

    public Trip TopologyToTripEntity(User user, TripTopologyInfo topology, DateTime uploadTime, MultiPolygon oldOverallArea)
    {
        return new Trip
        {
            Id = new Guid(),
            User = user,
            GpsLineString = topology.GpsPath,
            GpsPolygon = topology.GpsPolygon,
            Polygon = topology.AreaPolygon,
            UploadTime = uploadTime,
            Area = topology.AreaPolygon.Area,
            Length = topology.LocalLinestring.Length,
            NewArea = topology.AreaPolygon.Difference(oldOverallArea).Area,
            StartTime = topology.StartTime,
            EndTime = topology.EndTime
        };
    }

    public ParallelQuery<DistrictAreaCacheEntry> UpdateDistrictAreas(User user)
    {
        // update district caches
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var newAreaCacheEntries = _db.Districts.AsParallel().Select(district =>
            new DistrictAreaCacheEntry
            {
                District = district,
                DistrictId = district.Id,
                User = user,
                UserId = user.Id,
                // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
                Area = district.Geometry.Intersection(user.OverallArea).Area,
            }
        );
        return newAreaCacheEntries;
    }

    public async Task RecomputeOverallArea(User user, List<Trip> trips)
    {
        // initial sanity check
        if (trips.Any(x => x.UserId != user.Id))
        {
            throw new ArgumentException("user does not own all trips!", nameof(trips));
        }

        if (trips.Count == 0)
        {
            user.OverallArea = MultiPolygon.Empty;
            await _db.DistrictAreaCacheEntries.Where(x => x.UserId == user.Id)
                .ExecuteUpdateAsync(
                    x => x.SetProperty(dace => dace.Area, 0));
            await _db.SaveChangesAsync();
            return;
        }
        
        trips.Sort((x, y) => x.UploadTime.CompareTo(y.UploadTime));
        var tx = await _db.Database.BeginTransactionAsync();
        var newOverallArea = await Task.Run(() =>
        {
            var overallArea = MultiPolygon.Empty as Geometry;
            
            foreach (var trip in trips)
            {
                trip.NewArea = trip.Polygon.Difference(overallArea).Area;
                overallArea = overallArea.Union(trip.Polygon);
            }

            return overallArea;
        }) as MultiPolygon;

        user.OverallArea = newOverallArea!; // i don't think there's a way for this to turn out to be null with all the checks above and whatnot
        user.OverallAreaAmount = newOverallArea!.Area;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task<IEnumerable<AchievementGet>> GetNewAchievements(User user, List<Trip> dbTrips)
    {
        await _db.Entry(user).Collection<Achievement>(x => x.Achievements).LoadAsync();

        var newAchievements = _achievementMgr.CheckUser(user).Select(
            x =>
            {
                x.AchievedOnTripId = dbTrips.Last().Id;
                return x;
            }).ToList();
        return newAchievements;
    }
}