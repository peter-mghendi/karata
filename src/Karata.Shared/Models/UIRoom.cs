namespace Karata.Shared.Models;

public class UIRoom
{
    public string? InviteLink { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public UIUser? Creator { get; set; } 
    public UIGame Game { get; set; } = new ();
    public ICollection<UIChat> Chats { get; set; } = new List<UIChat>();
}