using System.Text;
using Karata.Kit.Domain.Models;
using Karata.Server.Data;
using Karata.Server.Infrastructure.Security;
using Karata.Server.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Karata.Kit.Domain.Models.GameStatus;

namespace Karata.Server.Handlers;

public static class RoomHandler
{
    public static async Task<Ok<List<RoomData>>> ListRooms([FromServices] KarataContext context)
    {
        // TODO: Hardcoding these conditions in for now because this endpoint is only used to find joinable games.
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
        [FromServices] KarataContext context,
        string id
    )
    {
        if (!Guid.TryParse(id, out var guid)) return TypedResults.BadRequest();
        var room = await context.Rooms.FindAsync(guid);

        if (room == null) return TypedResults.NotFound();
        return TypedResults.Ok(room.ToData());
    }

    public static async Task<Results<CreatedAtRoute<RoomData>, UnauthorizedHttpResult>> CreateRoom(
        [FromServices] KarataContext context,
        [FromServices] CurrentUserService currentUserService,
        [FromServices] IPasswordService passwordService,
        [FromBody] RoomRequest request
    )
    {
        try
        {
            var user = await currentUserService.RequireAsync();

            var room = new Room { Administrator = user, Creator = user, CreatedAt = DateTimeOffset.UtcNow };
            room.Game.Hands.Add(new Hand { Player = user, Status = HandStatus.Offline });

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                room.Salt = IPasswordService.GenerateSalt();
                room.Hash = passwordService.HashPassword(Encoding.UTF8.GetBytes(request.Password), room.Salt!);
            }

            context.Rooms.Add(room);
            context.Activities.Add(Activity.GameCreated(room));

            await context.SaveChangesAsync();
            return TypedResults.CreatedAtRoute(room.ToData(), nameof(GetRoom), new { id = room.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
    }
}