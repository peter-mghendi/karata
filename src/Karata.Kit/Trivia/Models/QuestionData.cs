namespace Karata.Kit.Trivia.Models;

public record QuestionData(long Id, string Text, List<ChoiceData> Choices);