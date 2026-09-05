using Karata.Kit.Platform.Models;
using Karata.Platform.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Karata.Platform.Handlers;

public sealed class ProfileHandler
{
    public static async Task<Ok<IEnumerable<UserData>>> ListProfiles([FromServices] PlatformContext context)
    {
        var profiles = await context.Users.AsNoTracking().ToArrayAsync();

        return Ok(from profile in profiles select profile.ToData());
    }

    public static async Task<Results<Ok<UserData>, NotFound>> GetProfile
    (
        [FromServices] IHttpContextAccessor http,
        [FromServices] PlatformContext context,
        [FromRoute] string identifier
    ) => await context.Users.AsNoTracking().FirstOrDefaultAsync(profile => profile.Id == identifier || profile.Username == identifier) switch
    {
        { } profile => Ok(profile.ToData()),
        null => NotFound()
    };
}