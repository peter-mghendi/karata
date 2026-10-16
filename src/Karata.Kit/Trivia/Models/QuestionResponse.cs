namespace Karata.Kit.Trivia.Models;

public record QuestionResponse(long Id, string Text, List<ChoiceResponse> Choices);