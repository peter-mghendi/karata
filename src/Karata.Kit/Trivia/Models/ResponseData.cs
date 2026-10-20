using Karata.Kit.Platform.Models;

namespace Karata.Kit.Trivia.Models;

public record ResponseData(long Id, int TimeLeft, int Points, ChoiceData? Choice, UserData User);