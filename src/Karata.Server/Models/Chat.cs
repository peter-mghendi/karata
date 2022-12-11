using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Chat
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public virtual User Sender { get; set; } = null!;
    public DateTime Sent { get; } = DateTime.Now;

    public UIChat ToUI() => new()
    {
        Text = Text,
        Sender = Sender.ToUI(),
        Sent = Sent
    };
}