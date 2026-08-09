namespace Karata.Trivia.Models;

public class Question
{
    public long Id { get; set; }

    public required string Text { get; set; }

    public required Topic Topic { get; set; }

    public List<Choice> Choices { get; } = [];
}