using AutoMapper;
using GdanskExplorer.Data;
using GdanskExplorer.Dtos;

namespace GdanskExplorer;

public class GExplorerAutoMapperProfile : Profile
{
    public GExplorerAutoMapperProfile()
    {
        ReplaceMemberName("UserName", "Username");
        CreateMap<User, ShortUserReturnDto>();
        CreateMap<Trip, TripReturnDto>();
        CreateMap<Trip, DetailedTripReturnDto>();
        CreateMap<District, DistrictDto>()
            .ForMember(x => x.Geometry,
                opt => opt.MapFrom(x =>
                    x.GpsGeometry));

        CreateMap<AchievementGet, AchievementGetDto>();

        CreateMap<User, UserReturnDto>()
            .ForMember(x => x.Trips,
                opt => opt.MapFrom(x =>
                    x.Trips.Take(50).OrderByDescending(t => t.StartTime)))
            .ForMember(x => x.DistrictAreas,
                opt => opt.MapFrom(x =>
                    x.DistrictAreas.ToDictionary(dace => dace.DistrictId, dace => dace.Area)))
            .ForMember(x => x.Achievements,
                opt => opt.MapFrom(x =>
                    x.AchievementGets))
            .ForMember(x => x.TotalTripLength, opt => opt.MapFrom(x =>
                x.Trips.Sum(t => t.Length)));
        CreateMap<District, ShortDistrictDto>();

        CreateMap(typeof(LeaderboardEntry<,>), typeof(LeaderboardEntryDto<>))
            .ConvertUsing(typeof(LeaderboardEntryConverter<>));

        CreateMap<Achievement, AchievementDetailsDto>()
            .ForMember(x => x.AchievedCount,
                opt => opt.MapFrom(x =>
                    x.Achievers.Count));

        CreateMap<GpxImportErrorKind, GpxImportErrorDto>()
            .ForMember(x => x.Cause, opt => opt.MapFrom(x =>
                x.ToString()));

        CreateMap<DatabaseLeaderboardRow, ShortUserReturnDto>();
    }
}