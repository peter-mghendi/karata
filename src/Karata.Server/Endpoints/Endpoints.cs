using Karata.Server.Handlers;
using Karata.Server.Hubs;

namespace Karata.Server.Endpoints;

public static class Endpoints
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapEndpoints()
        {
            var api = app.MapGroup("/api");

            var activity = api.MapGroup("/activity");
            activity.MapGet("", ActivityHandler.ListActivity).WithName(nameof(ActivityHandler.ListActivity));

            var rooms = api.MapGroup("/rooms");
            rooms.MapGet("", RoomHandler.ListRooms).WithName(nameof(RoomHandler.ListRooms)).RequireAuthorization();
            rooms.MapPost("", RoomHandler.CreateRoom).WithName(nameof(RoomHandler.CreateRoom)).RequireAuthorization();
            rooms.MapGet("{id}", RoomHandler.GetRoom).WithName(nameof(RoomHandler.GetRoom));

            var turns = api.MapGroup("/rooms/{id}/turns");
            turns.MapGet("", TurnHandler.ListTurns).WithName(nameof(TurnHandler.ListTurns)).RequireAuthorization();
        }

        public void MapHubs()
        {
            app.MapHub<PlayerHub>("/hubs/game/play", options => options.AllowStatefulReconnects = true);
            app.MapHub<SpectatorHub>("/hubs/game/spectate", options => options.AllowStatefulReconnects = true);
        }
    }
}