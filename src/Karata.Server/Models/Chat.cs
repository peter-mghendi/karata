using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Chat
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required User Sender { get; set; }
    public DateTime Sent { get; } = DateTime.Now;

    public ChatData ToData() => new()
    {
        Text = Text,
        Sender = Sender.ToData(),
        Sent = Sent
    };
}