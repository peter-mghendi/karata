namespace Karata.Trivia.Models;

public class Topic
{
    public long Id { get; init; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public List<Game> Games { get; } = [];

    public List<Question> Questions { get; } = [];
}