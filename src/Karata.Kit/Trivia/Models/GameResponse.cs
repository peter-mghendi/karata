using System.Collections.Immutable;
using Karata.Kit.Platform.Models;

namespace Karata.Kit.Trivia.Models;

public record GameResponse(Guid Id, TopicResponse Topic, UserData Creator, ImmutableArray<UserData> Players);