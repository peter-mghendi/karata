using Karata.Kit.Trivia.Models;
using Karata.Kit.Trivia.Models.Response;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class NotificationService(RestClient client)
{
    public async Task<List<NotificationResponse>> GetNotifications()
    {
        var response = await client.GetAsync<List<NotificationResponse>>("notifications");
        return response!;
    }

    public async Task MarkNotificationRead(long id)
    {
        _ = await client.PutJsonAsync($"notifications/{id}", new { });
    }

    public async Task Subscribe(NotificationSubscriptionData data)
    {
        _ = await client.PutJsonAsync("notifications/subscribe", data);
    }
}