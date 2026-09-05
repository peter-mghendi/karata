using System.Security.Claims;
using Karata.Go.Data;
using Karata.Go.Models;
using Karata.Kit.Go.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Karata.Go.Handlers;

file static class SlugGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int Length = 4;

    public static string Generate() => Random.Shared.GetString(Alphabet, Length);
}

public sealed class LinkHandler
{
    private const int MaxAttempts = 10;

    public static async Task<Results<CreatedAtRoute<LinkData>, InternalServerError>> CreateLink
    (
        [FromServices] ClaimsPrincipal principal,
        [FromServices] GoContext db,
        [FromServices] ILogger<LinkHandler> logger,
        [FromBody] LinkRequest request
    )
    {
        var application = principal.FindFirstValue("azp")!;
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await db.Users.FindAsync(subject);

        for (var attempts = 1L; attempts <= MaxAttempts; attempts++)
        {
            var link = new Link
            {
                Slug = SlugGenerator.Generate(),
                Destination = request.Destination,
                Attempts = attempts,
                Application = application,
                Creator = user!,
                CreatedAt = DateTimeOffset.UtcNow
            };

            try
            {
                db.Links.Add(link);
                await db.SaveChangesAsync();
                return CreatedAtRoute(link.ToData(), nameof(RedirectHandler.To), new() { ["slug"] = link.Slug });
            }
            catch (DbUpdateException)
            {
                if (!await db.Links.AsNoTracking().AnyAsync(l => l.Slug == link.Slug)) throw;
                db.Entry(link).State = EntityState.Detached;
            }
        }

        logger.LogWarning("Slug generation exhausted after {Attempts} attempts", MaxAttempts);
        return InternalServerError();
    }


    public static async Task<Results<ForbidHttpResult, NoContent>> DeleteLink
    (
        [FromServices] ClaimsPrincipal principal,
        [FromServices] GoContext db,
        [FromRoute] string slug
    )
    {
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var link = await db.Links
            .AsNoTracking()
            .Include(link => link.Creator)
            .SingleOrDefaultAsync(l => l.Slug == slug && l.DeletedAt != null);

        if (link is null) return NoContent();
        if (link.Creator.Id != subject) Forbid();

        link.DeletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();
        return NoContent();
    }
}