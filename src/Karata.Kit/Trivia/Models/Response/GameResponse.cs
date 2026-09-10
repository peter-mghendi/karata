using System.Collections.Immutable;
using Karata.Kit.Platform.Models;

namespace Karata.Kit.Trivia.Models.Response;

public record GameResponse(Guid Id, TopicResponse Topic, UserData Creator, ImmutableArray<UserData> Players);