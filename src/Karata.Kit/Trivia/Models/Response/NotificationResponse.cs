using Karata.Kit.Trivia.Models.Enum;

namespace Karata.Kit.Trivia.Models.Response;

public record NotificationResponse(
    long Id, 
    NotificationAction Action,
    DateTimeOffset SentAt,
    DateTimeOffset? ReadAt, 
    GameResponse Game
)
{
    public bool IsRead => ReadAt is not null;
    
    public string Text => Action switch
    {
        NotificationAction.Play => $"{Game.PlayerOne.Username} has challenged you in {Game.Topic.Title}. Play now!",
        NotificationAction.Results => $"{Game.PlayerTwo.Username} has completed your challenge in {Game.Topic.Title}. View results now!",
        _ => throw new ArgumentOutOfRangeException()
    };

    public string Url => Action switch
    {
        NotificationAction.Play => $"game/{Game.Identifier}/play",
        NotificationAction.Results => $"game/{Game.Identifier}/results",
        _ => throw new ArgumentOutOfRangeException()
    };
}