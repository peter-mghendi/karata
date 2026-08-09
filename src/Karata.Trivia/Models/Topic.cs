namespace Karata.Trivia.Models;

public class Topic
{
    public long Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public List<Question> Questions { get; } = [];
}