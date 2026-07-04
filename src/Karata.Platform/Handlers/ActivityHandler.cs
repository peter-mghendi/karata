using System.Security.Claims;
using Karata.Kit.Platform.Models;
using Karata.Platform.Data;
using Karata.Platform.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Karata.Platform.Handlers;

public sealed class ActivityHandler
{
    public static async Task<Ok<List<ActivityData>>> ListActivity([FromServices] KarataPlatformContext context)
    {
        var activity = await context.Activity.OrderByDescending(a => a.OccurredAt)
            .Take(50)
            .ToListAsync();

        var subjects = activity.Select(a => a.Subject);
        var users = await context.Users.Where(u => subjects.Contains(u.Id)).ToDictionaryAsync(u => u.Id);
        var data = activity.Select(a => new ActivityData
        {
            Id = a.Id,
            Actor = users[a.Subject].ToData(),
            Application = new ApplicationData(a.Application),
            Text = a.Text,
            Actions = a.Actions,
            Metadata = a.Metadata,
            OccurredAt = a.OccurredAt
        }).ToList();

        return Ok(data);
    }

    public static async Task<Ok<ActivityData>> CreateActivity
    (
        [FromServices] IHttpContextAccessor http,
        [FromServices] KarataPlatformContext context,
        [FromBody] ActivityRequest request
    )
    {
        var principal = http.HttpContext!.User;
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var application = principal.FindFirstValue("azp")!;

        var activity = new Activity
        {
            Id = Guid.CreateVersion7(),
            Subject = subject,
            Application = application,
            Text = request.Text,
            Actions = request.Actions,
            Metadata = request.Metadata,
            OccurredAt = request.OccurredAt,
            RecordedAt = DateTimeOffset.UtcNow
        };

        activity = context.Add(activity).Entity;
        await context.SaveChangesAsync();
        
        var user = await context.Users.FindAsync(activity.Subject);
        var data = new ActivityData
        {
            Id = activity.Id,
            Actor = user!.ToData(),
            Application = new ApplicationData(activity.Application),
            Text = activity.Text,
            Actions = activity.Actions,
            Metadata = activity.Metadata,
            OccurredAt = activity.OccurredAt
        };

        return Ok(data);
    }
}