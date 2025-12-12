using Karata.Kit.Domain.Models;

namespace Karata.Server.Models;

public class Chat
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required User Sender { get; set; }
    public required DateTimeOffset SentAt { get; set;  }

    public ChatData ToData() => new()
    {
        Text = Text,
        Sender = Sender.ToData(),
        SentAt = SentAt
    };
    
    public static implicit operator ChatData(Chat chat) => chat.ToData();
}