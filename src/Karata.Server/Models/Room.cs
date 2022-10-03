using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Room
{
    public Guid Id { get; set; }
    public virtual User Creator { get; set; } = null!;
    public virtual Game Game { get; set; } = new ();
    public DateTime CreatedAt { get; } = DateTime.Now;
    public byte[]? Hash { get; set; } = null;
    public byte[]? Salt { get; set; } = null;
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    public UIRoom ToUI() => new()
    {
        CreatedAt = CreatedAt,
        Creator = Creator.ToUI(),
        Game = Game.ToUI(),
        Chats = Chats.Select(c => c.ToUI()).ToList()
    };
}