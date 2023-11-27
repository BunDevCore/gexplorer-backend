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
        CreateMap<User, UserReturnDto>()
            .ForMember(x => x.Trips,
                opt => opt.MapFrom(x =>
                    x.Trips.Take(10)))
            .ForMember(x => x.DistrictAreas,
                opt => opt.MapFrom(x => 
                    x.DistrictAreas.ToDictionary(dace => dace.DistrictId, dace => dace.Area)));
        CreateMap<District, ShortDistrictDto>();

        CreateMap(typeof(LeaderboardEntry<,>), typeof(LeaderboardEntryDto<>))
            .ConvertUsing(typeof(LeaderboardEntryConverter<>));
    }
}