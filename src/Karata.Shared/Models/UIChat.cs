namespace Karata.Shared.Models;

public class UIChat
{
    public string Text { get; set; } = string.Empty;
    public UIUser Sender { get; set; } = null!;
    public DateTime Sent { get; set; } = DateTime.UtcNow;
}