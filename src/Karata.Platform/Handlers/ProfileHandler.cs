using Karata.Kit.Platform.Models;
using Karata.Platform.Data;
using Karata.Platform.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Karata.Platform.Handlers;

file static class ProfileExtensions
{
    extension(User profile)
    {
        // TODO [HACK] Until I get round to building a proper avatar system
        public string Avatar => $"https://api.dicebear.com/10.x/glyphs/svg?seed={profile.Username}";
    }
}

public sealed class ProfileHandler
{
    public static async Task<Ok<IEnumerable<ProfileData>>> ListProfiles([FromServices] PlatformContext context)
    {
        var profiles = await context.Users.AsNoTracking().ToArrayAsync();

        return Ok(from profile in profiles select new ProfileData(profile.Id, profile.Username, profile.Avatar));
    }

    public static async Task<Results<Ok<ProfileData>, NotFound>> GetProfile
    (
        [FromServices] IHttpContextAccessor http,
        [FromServices] PlatformContext context,
        [FromRoute] string identifier
    ) => await context.Users.AsNoTracking().FirstOrDefaultAsync(profile => profile.Id == identifier || profile.Username == identifier) switch
    {
        { } profile => Ok(new ProfileData(profile.Id, profile.Username, profile.Avatar)),
        null => NotFound()
    };
}