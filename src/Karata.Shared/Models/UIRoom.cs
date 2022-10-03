namespace Karata.Shared.Models;

public class UIRoom
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public UIUser Creator { get; set; } = null!;
    public UIGame Game { get; set; } = new ();
    public ICollection<UIChat> Chats { get; set; } = new List<UIChat>();
}