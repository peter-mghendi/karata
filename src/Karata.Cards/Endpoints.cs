using Karata.Cards.Hubs;
using Karata.Cards.Handlers;

namespace Karata.Cards;

public static class Endpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapEndpoints()
        {
            var api = endpoints.MapGroup("/api");

            var rooms = api.MapGroup("/rooms");
            rooms.MapMethods("", [HttpMethods.Query], RoomHandler.QueryRooms)
                .WithName(nameof(RoomHandler.QueryRooms))
                .RequireAuthorization();
            rooms.MapPost("", RoomHandler.CreateRoom).WithName(nameof(RoomHandler.CreateRoom)).RequireAuthorization();
            rooms.MapGet("{id}", RoomHandler.GetRoom).WithName(nameof(RoomHandler.GetRoom));

            var hands = api.MapGroup("/rooms/{id}/hands");
            hands.MapPost("", RoomHandHandler.CreateHand).WithName(nameof(RoomHandHandler.CreateHand)).RequireAuthorization();

            var turns = api.MapGroup("/rooms/{id}/turns");
            turns.MapGet("", TurnHandler.ListTurns).WithName(nameof(TurnHandler.ListTurns)).RequireAuthorization();
        }

        // TODO: [Leg   acy] Use MessagePack
        public void MapHubs()
        {
            endpoints.MapHub<PlayerHub>("/hubs/game/play", options => options.AllowStatefulReconnects = true);
            endpoints.MapHub<ReplayerHub>("/hubs/game/replay", options => options.AllowStatefulReconnects = true);
            endpoints.MapHub<SpectatorHub>("/hubs/game/spectate", options => options.AllowStatefulReconnects = true);
        }
    }
}