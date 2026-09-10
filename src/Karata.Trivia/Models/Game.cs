namespace Karata.Trivia.Models;

public class Game
{
    public Guid Id { get; set; }
    
    public required DateTimeOffset CreatedAt { get; init; }

    public long TopicId { get; set; }

    public required Topic Topic { get; init; }

    public required User Creator { get; init; }

    public List<GameParticipant> Participants { get; init; } = [];
    
    public List<User> Players { get; init; } = [];

    public List<Round> Rounds { get; init; } = [];
}