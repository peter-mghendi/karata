using Karata.Kit.Platform.Models;
using Karata.Platform.Data;
using Karata.Platform.Handlers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Karata.Platform;

public static class Endpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapEndpoints()
        {
            var api = endpoints.MapGroup("/api");
            
            var activity = api.MapGroup("/activity");
            activity.MapGet("", ActivityHandler.ListActivity).WithName(nameof(ActivityHandler.ListActivity));
            activity.MapPost("", ActivityHandler.CreateActivity).WithName(nameof(ActivityHandler.CreateActivity)).RequireAuthorization();
            
            api.MapGet(
                    "/profiles",
                    async ([FromServices] KarataPlatformContext context) =>
                    {
                        var profiles = await context.Users.AsNoTracking().ToArrayAsync();
                        return Ok(profiles.Select(profile => new ProfileData(profile.Id, profile.Username,
                            $"https://api.dicebear.com/10.x/glyphs/svg?seed={profile.Username}")));
                    })
                .WithName("ListProfiles")
                .RequireAuthorization();
            
            api.MapGet(
                    "/profiles/{username}",
                    async Task<Results<Ok<ProfileData>, NotFound>> ([FromServices] KarataPlatformContext context,
                        string username) =>
                    {
                        var profile = await context.Users.AsNoTracking()
                            .FirstOrDefaultAsync(profile => profile.Username == username);
                        if (profile is null) return NotFound();

                        return Ok(new ProfileData(profile.Id, profile.Username,
                            $"https://api.dicebear.com/10.x/glyphs/svg?seed={profile.Username}"));
                    })
                .WithName("GetProfile")
                .RequireAuthorization();
        }
    }
}