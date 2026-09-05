using System.Collections.Immutable;
using Karata.Kit.Platform.Models;

namespace Karata.Kit.Trivia.Models.Response;

public record GameResponse(long Id, Guid Identifier, TopicResponse Topic, UserData PlayerOne, UserData PlayerTwo)
{
    public ImmutableArray<UserData> Players => [PlayerOne, PlayerTwo];
}