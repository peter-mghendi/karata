using Karata.Kit.Trivia.Models;
using Karata.Kit.Trivia.Models.Response;
using Karata.Runtime.Infrastructure;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationController(
    CurrentUserService<TriviaContext, User> current, TriviaContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetNotifications()
    {
        var user = await current.RequireAsync();
        return await context.Notifications
            .Include(n => n.Game)
            .ThenInclude(g => g.Topic)
            .Include(n => n.Game)
            .ThenInclude(g => g.PlayerOne)
            .Include(n => n.Game)
            .ThenInclude(g => g.PlayerTwo)
            .Where(n => n.Recipient.Id == user.Id)
            .OrderByDescending(n => n.ReadAt == null)
            .ThenByDescending(n => n.SentAt)
            .Select(n => n.AsResponse())
            .ToListAsync();
    }

    [HttpPut("subscribe")]
    public async Task<IResult> UpsertNotificationSubscription([FromBody] NotificationSubscriptionData data)
    {
        var user = await current.RequireAsync();
        var stale = context.NotificationSubscriptions.Where(e => e.User.Id == user.Id);
        var subscription = new NotificationSubscription
        {
            Url = data.Url!,
            P256dh = data.P256dh!,
            Auth = data.Auth!,
            User = user
        };

        context.NotificationSubscriptions.RemoveRange(stale);
        context.NotificationSubscriptions.Add(subscription);
        await context.SaveChangesAsync();

        return Results.Ok(subscription.AsResponse());
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> MarkNotificationRead(long id) => await context.Notifications.FindAsync(id) switch
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