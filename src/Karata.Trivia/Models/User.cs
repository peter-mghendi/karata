namespace Karata.Trivia.Models;

public class User
{
    public required string Id { get; set; }

    public required string Username { get; set; }

    public List<Game> CreatedGames { get; } = [];
    
    public List<Game> InvitedGames { get; } = [];
    
    public List<NotificationSubscription> NotificationSubscriptions { get; } = [];
}

