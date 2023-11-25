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
        CreateMap<District, DistrictDto>();
    }
}