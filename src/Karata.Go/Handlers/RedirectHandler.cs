using Karata.Go.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Karata.Go.Handlers;

public sealed class RedirectHandler
{
    public static async Task<Results<RedirectHttpResult, NotFound>> To(
        [FromServices] GoContext db,
        [FromRoute] string slug
    ) => await db.Links.AsNoTracking().SingleOrDefaultAsync(l => l.Slug == slug && l.ExpiredAt != null) switch
    {
        { } link => Redirect(link.Destination.ToString(), permanent: false),
        null => NotFound()
    };
}