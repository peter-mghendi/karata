using Karata.Cards.Data;
using Karata.Kit.Cards.Models;
using Karata.Kit.Platform;
using Karata.Runtime.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Cards.Handlers;

public static class RoomHandHandler
{
    public static async Task<Results<NoContent, BadRequest, ForbidHttpResult, UnauthorizedHttpResult>> CreateHand(
        [FromServices] Client platform,
        [FromServices] CardsContext context,
        [FromServices] CurrentUserService<CardsContext, User> currentUserService,
        [FromRoute] string id,
        [FromBody] HandRequest request
    )
    {
        try
        {
            var user = await currentUserService.RequireAsync();

            if (!Guid.TryParse(id, out var guid)) return TypedResults.BadRequest();
            if (await context.Rooms.FindAsync(guid) is not {} room) return TypedResults.BadRequest();
            if (room.Game.Hands.All(h => h.Player.Id != user.Id)) return TypedResults.Forbid();
            if (room.Game.Hands.Any(h => h.Player.Id == request.UserId)) return TypedResults.NoContent();

            if (room.Game.Status != GameStatus.Lobby) return TypedResults.BadRequest();
            if (room.Game.Hands.Count(h => h.Status is HandStatus.Active) >= 4) return TypedResults.BadRequest();
            if (await ResolveUser(platform, context, request.UserId) is not {} invitee) return TypedResults.BadRequest();

            room.Game.Hands.Add(new Hand { Player = invitee, Status = HandStatus.Invited });

            await context.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
    }

    private static async Task<User?> ResolveUser(Client platform, CardsContext context, string id)
    {
        var local = await context.Users.FindAsync(id);
        if (local is not null) return local;

        var profile = await platform.Profiles.GetAsync(id);
        if (profile is null) return null;
        
        var user = new User { Id = profile.Id, Username = profile.Username };
        var entity = context.Users.Add(user).Entity;
        await context.SaveChangesAsync();
        
        return entity;
    }
}