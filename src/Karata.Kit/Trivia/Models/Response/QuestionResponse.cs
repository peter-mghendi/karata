namespace Karata.Kit.Trivia.Models.Response;

public record QuestionResponse(long Id, string Text, List<ChoiceResponse> Choices);