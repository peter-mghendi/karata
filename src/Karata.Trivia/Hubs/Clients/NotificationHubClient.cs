using Karata.Kit.Trivia.Models.Response;

namespace Karata.Trivia.Hubs.Clients;

public interface INotificationHubClient
{
    Task ReceiveNotification(NotificationResponse response);
}