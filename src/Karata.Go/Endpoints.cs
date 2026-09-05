namespace Karata.Go;

public static class Endpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapEndpoints()
        {
            var api = endpoints.MapGroup("/api").RequireAuthorization();
        }
    }
}
