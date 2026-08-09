using Karata.Runtime.Models;

namespace Karata.Trivia.Models;

public class User : KarataUser
{
    public List<Game> CreatedGames { get; } = [];
    
    public List<Game> InvitedGames { get; } = [];
    
    public List<NotificationSubscription> NotificationSubscriptions { get; } = [];
}

