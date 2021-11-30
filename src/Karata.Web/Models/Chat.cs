using Karata.Web.Models.UI;
namespace Karata.Web.Models;

public class Chat
{
    public int Id { get; set; }
    public string Text { get; set; }
    public virtual User Sender { get; set; }
    public DateTime Sent { get; } = DateTime.Now;

    public UIChat ToUI() => new()
    {
        Text = Text,
        Sender = Sender.ToUI(),
        Sent = Sent
    };
}