using AutoMapper;

namespace GdanskExplorer.Dtos;

public class LeaderboardEntryConverter<TV> : ITypeConverter<LeaderboardEntry<TV, ShortUserReturnDto>, LeaderboardEntryDto<TV>>
{
    public LeaderboardEntryDto<TV> Convert(LeaderboardEntry<TV, ShortUserReturnDto> source, LeaderboardEntryDto<TV> destination, ResolutionContext context)
    {
        return new LeaderboardEntryDto<TV>
        {
            User = context.Mapper.Map<ShortUserReturnDto>(source.Inner),
            Value = source.Value
        };
    }
}