namespace Karata.Trivia.Models;

public class Choice
{
    public long Id { get; set; }

    public required string Text { get; set; }

    public bool IsCorrect { get; set; }

    public required Question Question { get; set; }
}