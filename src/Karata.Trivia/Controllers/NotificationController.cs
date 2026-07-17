using Karata.Kit.Trivia.Models;
using Karata.Kit.Trivia.Models.Response;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Infrastructure;
using Karata.Trivia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationController(CurrentUserService user, KarataTriviaContext context) : ControllerBase
{
    // GET: api/notifications
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotifications()
    {
        var current = await user.RequireAsync();
        return await context.Notifications
            .Include(n => n.Game)
            .ThenInclude(g => g.Topic)
            .Include(n => n.Game)
            .ThenInclude(g => g.PlayerOne)
            .Include(n => n.Game)
            .ThenInclude(g => g.PlayerTwo)
            .Where(n => n.Recipient.Id == current.Id)
            .OrderByDescending(n => n.ReadAt == null)
            .ThenByDescending(n => n.SentAt)
            .Select(n => n.AsResponse())
            .ToListAsync();
    }

    // PUT: api/notifications/5
    [HttpPut("subscribe")]
    public async Task<IResult> PutNotification([FromBody] NotificationSubscriptionData data)
    {
        var current = await user.RequireAsync();

        // We're storing at most one subscription per user, so delete old ones.
        // Alternatively, I could let the user register multiple subscriptions from different browsers/devices.
        var stale = context.NotificationSubscriptions.Where(e => e.User.Id == current.Id);
        var subscription = new NotificationSubscription
        {
            Url = data.Url!,
            P256dh = data.P256dh!,
            Auth = data.Auth!,
            User = current
        };

        context.NotificationSubscriptions.Add(subscription);
        context.NotificationSubscriptions.RemoveRange(stale);
        await context.SaveChangesAsync();

        return Results.Ok(subscription.AsResponse());
    }

    // PUT: api/notifications/5
    [HttpPut("{id:long}")]
    public async Task<IActionResult> PutNotification(long id) => await context.Notifications.FindAsync(id) switch
    {
        null => NotFound(),
        not { ReadAt: null } => Conflict(),
        { } notification => await MarkNotificationRead(notification)
    };


    private async Task<NoContentResult> MarkNotificationRead(Notification notification)
    {
        notification.ReadAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
        return NoContent();
    }
}