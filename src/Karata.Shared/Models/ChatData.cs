namespace Karata.Shared.Models;

public record ChatData
{
    public required string Text { get; set; }
    public required UserData Sender { get; set; }
    public DateTime Sent { get; set; } = DateTime.UtcNow;
}