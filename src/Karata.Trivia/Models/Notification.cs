using Karata.Kit.Trivia.Models.Enum;

namespace Karata.Trivia.Models;

public class Notification
{
    public long Id { get; set; }
    
    public NotificationAction Action { get; set; }
    
    public DateTimeOffset SentAt { get; set; }
    
    public DateTimeOffset? ReadAt { get; set; }

    public required Game Game { get; set; }

    public required User Recipient { get; set; }
}