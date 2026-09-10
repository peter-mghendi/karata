namespace Karata.Trivia.Models;

public class Choice
{
    public long Id { get; set; }

    public required string Text { get; init; }

    public bool IsCorrect { get; init; }
    
    public long QuestionId { get; set; }

    public required Question Question { get; set; }
}