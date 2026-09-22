using Karata.Kit.Platform.Models;

namespace Karata.Kit.Trivia.Models;

public record ResponseResponse(long Id, int TimeLeft, int Points, ChoiceResponse? Choice, UserData User);