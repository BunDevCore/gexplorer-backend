using AutoMapper;
using GdanskExplorer.Data;

namespace GdanskExplorer.Dtos;

public class LeaderboardEntryConverter<TV> : ITypeConverter<LeaderboardEntry<TV, User>, LeaderboardEntryDto<TV>>
{
    public LeaderboardEntryDto<TV> Convert(LeaderboardEntry<TV, User> source, LeaderboardEntryDto<TV> destination, ResolutionContext context)
    {
        return new LeaderboardEntryDto<TV>
        {
            User = context.Mapper.Map<ShortUserReturnDto>(source.Inner),
            Value = source.Value
        };
    }
}