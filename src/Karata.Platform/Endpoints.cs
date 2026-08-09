using Karata.Kit.Platform.Models;
using Karata.Platform.Data;
using Karata.Platform.Handlers;
using Karata.Platform.Models;
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
            activity.MapPost("", ActivityHandler.CreateActivity).WithName(nameof(ActivityHandler.CreateActivity))
                .RequireAuthorization();

            // TODO [HTTP QUERY]: Change this to MapQuery once ASP.NET Core has support
            api.MapGet("/profiles", ProfileHandler.ListProfiles)
                .WithName(nameof(ProfileHandler.ListProfiles))
                .RequireAuthorization();

            api.MapGet("/profiles/{identifier}", ProfileHandler.GetProfile)
                .WithName(nameof(ProfileHandler.GetProfile))
                .RequireAuthorization();
        }
    }
}
