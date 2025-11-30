namespace Karata.Kit.Domain.Models;

public record ChatData
{
    public required string Text { get; set; }
    public required UserData Sender { get; set; }
    public required DateTimeOffset SentAt { get; set; }
}