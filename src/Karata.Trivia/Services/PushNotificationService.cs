using System.Text.Json;
using Karata.Trivia.Extensions;
using Karata.Trivia.Models;
using WebPush;

namespace Karata.Trivia.Services;

public static class PushNotificationService
{
    // TODO: Config
    private const string PublicKey =
        "BLC8GOevpcpjQiLkO7JmVClQjycvTCYWm6Cq_a7wJZlstGTVZvwGFFHMYfXt6Njyvgx_GlXJeo5cSiZ1y4JOx1o";

    private const string PrivateKey = "OrubzSz3yWACscZXjFQrrtDwCKg-TGFuWhluQ2wLXDo";
    
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task SendNotificationAsync(Notification notification)
    {
        var subscription = notification.Recipient.NotificationSubscriptions.SingleOrDefault();
        if (subscription is null) return;
        
        var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
        var vapidDetails = new VapidDetails("mailto:<someone@example.com>", PublicKey, PrivateKey);
        var webPushClient = new WebPushClient();

        try
        {
            var payload = JsonSerializer.Serialize(notification.AsResponse(), Options);
            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync("Error sending push notification: " + ex.Message);
        }
    }
}