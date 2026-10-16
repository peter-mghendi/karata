using Karata.Platform.Handlers;

namespace Karata.Platform;

public static class Endpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapEndpoints()
        {
            var api = endpoints.MapGroup("/api")
                .RequireAuthorization();

            var activity = api.MapGroup("/activity");
            activity.MapGet("", ActivityHandler.ListActivity)
                .WithName(nameof(ActivityHandler.ListActivity))
                .AllowAnonymous();
            activity.MapPost("", ActivityHandler.CreateActivity)
                .WithName(nameof(ActivityHandler.CreateActivity));

            var profiles = api.MapGroup("/profiles");
            profiles.MapGet("", ProfileHandler.ListProfiles)
                .WithName(nameof(ProfileHandler.ListProfiles));
            profiles.MapGet("/{identifier}", ProfileHandler.GetProfile)
                .WithName(nameof(ProfileHandler.GetProfile));
        }
    }
}