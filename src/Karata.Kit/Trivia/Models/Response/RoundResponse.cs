namespace Karata.Kit.Trivia.Models.Response;

public record RoundResponse(long Id, int Index, QuestionResponse Question, List<ResponseResponse> Responses)
{
    public ChoiceResponse? Answer => Question.Choices.SingleOrDefault(c => c.IsCorrect);
    
    public bool IsAnswer(ChoiceResponse choice) => Answer?.Id == choice.Id;
}