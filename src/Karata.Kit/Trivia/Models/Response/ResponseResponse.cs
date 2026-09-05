using Karata.Kit.Platform.Models;

namespace Karata.Kit.Trivia.Models.Response;

public record ResponseResponse(long Id, int TimeLeft, int Points, ChoiceResponse? Choice, UserData User);