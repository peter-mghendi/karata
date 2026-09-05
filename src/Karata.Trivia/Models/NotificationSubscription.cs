namespace Karata.Trivia.Models;

public class NotificationSubscription
{
    public long Id { get; set; }

    public required string Url { get; set; }

    public required string P256dh { get; set; }

    public required string?Auth { get; set; }
    
    public required User User { get; set; }
}