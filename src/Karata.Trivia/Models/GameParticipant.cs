namespace Karata.Trivia.Models;

public class GameParticipant
{
    public Guid GameId { get; set; }
    public string UserId { get; set; } = null!;

    public Game Game { get; set; } = null!;
    public User User { get; set; } = null!;
}