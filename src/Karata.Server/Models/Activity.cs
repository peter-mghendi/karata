using Karata.Kit.Domain.Models;

namespace Karata.Server.Models;

public class Activity
{
    public int Id { get; set; }
    public required ActivityType Type { get; set; }
    public required string Text { get; set; }
    public required string Action { get; set; }
    public required string Link { get; set; }
    public required Dictionary<string, object> Metadata { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required User Actor { get; set; }

    public static Activity GameCreated(Room room) => new()
    {
        Type = ActivityType.GameCreated,
        Text = $"{room.Creator.Username} has started a game.",
        Action = "Check it out!",
        Link = $"/game/{room.Id}",
        Metadata = new Dictionary<string, object> { ["room"] = room.Id },
        CreatedAt = room.CreatedAt,
        Actor = room.Creator
    };

    public static Activity GameWon(Room room) => new()
    {
        Type = ActivityType.GameWon,
        Text = $"{room.Game.Result!.Winner!.Username} has just won a game.",
        Action = "See results",
        Link = $"/game/{room.Id}/over",
        Metadata = new Dictionary<string, object> { ["room"] = room.Id },
        CreatedAt = room.Game.Result!.CompletedAt,
        Actor = room.Game.Result!.Winner!
    };

    public ActivityData ToData() => new()
    {   
        Type = Type,
        Text = Text,
        Action = Action,
        Link = Link,
        Metadata = Metadata,
        CreatedAt = CreatedAt,
        Actor = Actor.ToData()
    };
}

