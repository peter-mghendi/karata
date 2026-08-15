using System.Text;
using Karata.Cards.Data;
using Karata.Cards.Services;
using Karata.Kit.Cards.Models;
using Karata.Kit.Platform;
using Karata.Kit.Platform.Models;
using Karata.Runtime.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Karata.Kit.Cards.Models.GameStatus;

namespace Karata.Cards.Routing.Handlers;

public static class RoomHandler
{
    public static async Task<Ok<List<RoomData>>> ListRooms([FromServices] CardsContext context)
    {
        // TODO: [Legacy] Hardcoding these conditions in for now because this endpoint is only used to find joinable games.
        var rooms = await context.Rooms
            .Where(room => room.Game.Status == Lobby)
            .Where(room => room.Game.Hands.Count < 4)
            .OrderByDescending(room => room.CreatedAt)
            .Take(5)
            .Select(room => room.ToData())
            .ToListAsync();

        return TypedResults.Ok(rooms);
    }

    public static async Task<Results<Ok<RoomData>, BadRequest, NotFound>> GetRoom(
        [FromServices] CardsContext context,
        string id
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
        [FromServices] CurrentUserService<CardsContext, User> currentUserService,
        [FromServices] IPasswordService passwordService,
        [FromBody] RoomRequest request
    )
    {
        try
        {
            var user = await currentUserService.RequireAsync();
            var hand = new Hand { Player = user, Status = HandStatus.Invited };
            var room = new Room { Administrator = user, Creator = user, CreatedAt = DateTimeOffset.UtcNow };
            room.Game.Hands.Add(hand);

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                room.Salt = IPasswordService.GenerateSalt();
                room.Hash = passwordService.HashPassword(Encoding.UTF8.GetBytes(request.Password), room.Salt!);
            }

            context.Rooms.Add(room);
            await context.SaveChangesAsync();
            
            // TODO: [Legacy] Fix this hardcoded URL. 
            var activity = new ActivityRequest
            {
                Text = $"{room.Creator.Username} has started a game.",
                Actions = [new("Check it out!", new Uri($"https://localhost:7240/game/{room.Id}"), "primary")],
                Metadata = new() { ["room"] = room.Id.ToString() },
                OccurredAt = room.CreatedAt
            };
            await platform.Activity.CreateAsync(activity);

            return TypedResults.CreatedAtRoute(room.ToData(), nameof(GetRoom), new { id = room.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
    }
}