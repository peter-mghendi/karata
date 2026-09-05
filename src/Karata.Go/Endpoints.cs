using Karata.Go.Handlers;

namespace Karata.Go;

public static class Endpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapEndpoints()
        {
            endpoints.MapGet("/to/{slug}", RedirectHandler.To).WithName(nameof(RedirectHandler.To));
            
            var api = endpoints.MapGroup("/api").RequireAuthorization();
            var links = api.MapGroup("/links");
            links.MapPost("", LinkHandler.CreateLink).WithName(nameof(LinkHandler.CreateLink));
            links.MapDelete("/{slug}", LinkHandler.DeleteLink).WithName(nameof(LinkHandler.DeleteLink));
        }
    }
}
