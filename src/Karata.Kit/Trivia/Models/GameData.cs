using System.Collections.Immutable;
using Karata.Kit.Platform.Models;

namespace Karata.Kit.Trivia.Models;

public record GameData(
    Guid Id,
    TopicData Topic,
    ImmutableArray<UserData> Players,
    UserData Creator,
    DateTimeOffset CreatedAt
);