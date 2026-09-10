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
        NotificationAction.Play => $"{Game.Creator.Username} has challenged you in {Game.Topic.Title}. Play now!",
        NotificationAction.Results => $"A player has completed your challenge in {Game.Topic.Title}. View results now!",
        _ => throw new ArgumentOutOfRangeException()
    };

    public string Url => Action switch
    {
        NotificationAction.Play => $"/trivia/game/{Game.Id}/play",
        NotificationAction.Results => $"/trivia/game/{Game.Id}/results",
        _ => throw new ArgumentOutOfRangeException()
    };
}