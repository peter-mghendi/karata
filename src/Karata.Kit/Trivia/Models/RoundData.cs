namespace Karata.Kit.Trivia.Models;

public record RoundData(long Id, int Index, QuestionData Question, List<ResponseData> Responses)
{
    public ChoiceData? Answer => Question.Choices.SingleOrDefault(c => c.IsCorrect);
    
    public bool IsAnswer(ChoiceData choice) => Answer?.Id == choice.Id;
}