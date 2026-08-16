using Karata.Cards.Data;
using Karata.Kit.Cards.Models;
using Karata.Kit.Configuration;
using Karata.Kit.Platform;
using Karata.Runtime.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Karata.Kit.Cards.Models.GameStatus;
using static Karata.Kit.Cards.Models.RoomVisibility;

namespace Karata.Cards.Handlers;

public static class RoomHandler
{
    public static async Task<Ok<List<RoomData>>> ListRooms([FromServices] CardsContext context)
    {
        // TODO: [Legacy] Hardcoding these conditions in for now because this endpoint is only used to find joinable games.
        var rooms = await context.Rooms
            .Where(room => room.Game.Status == Lobby)
            .Where(room => room.Visibility == Public)
            .Where(room => room.Game.Hands.Count < 4)
            .OrderByDescending(room => room.CreatedAt)
            .Take(5)
            .Select(room => room.ToData())
            .ToListAsync();

        return TypedResults.Ok(rooms);
    }

    public static async Task<Results<Ok<RoomData>, BadRequest, NotFound>> GetRoom(
        [FromServices] CardsContext context,
        [FromRoute] string id
    )
    {
        if (!Guid.TryParse(id, out var guid)) return TypedResults.BadRequest();
        var room = await context.Rooms.FindAsync(guid);

        if (room == null) return TypedResults.NotFound();
        return TypedResults.Ok(room.ToData());
    }

    public static async Task<Results<CreatedAtRoute<RoomData>, UnauthorizedHttpResult>> CreateRoom(
        [FromServices] Client platform,
        [FromServices] CardsContext context,
        [FromServices] CurrentUserService<CardsContext, User> current,
        [FromKeyedServices(nameof(Configuration.Web))] HostConfiguration web,
        [FromBody] RoomRequest request
    )
    {
        try
        {
            var user = await current.RequireAsync();
            var room = new Room { Visibility = request.Visibility, Administrator = user, Creator = user, CreatedAt = DateTimeOffset.UtcNow };

            room.Game.Hands.Add(new Hand { Player = user, Status = HandStatus.Invited });

            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            if (room.Visibility is Public) await platform.Activity.CreateAsync(
                new()
                {
                    Text = $"{room.Creator.Username} has started a game.",
                    Actions = [new("Check it out!", new Uri($"{web.Host}/game/{room.Id}"), "primary")],
                    Metadata = new() { ["room"] = room.Id.ToString() },
                    OccurredAt = room.CreatedAt
                }
            );

            return TypedResults.CreatedAtRoute(room.ToData(), nameof(GetRoom), new { id = room.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
    }
}