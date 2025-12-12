using Karata.Kit.Domain.Models;
using Karata.Server.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Server.Handlers;

public static class TurnHandler
{
    [HttpGet]
    public static async Task<Results<Ok<List<TurnData>>, BadRequest, NotFound>> ListTurns([FromServices] KarataContext context, string id)
    {
        if (!Guid.TryParse(id, out var guid)) return TypedResults.BadRequest();
        if (await context.Rooms.FindAsync(guid) is not {} room) return TypedResults.NotFound();

        var turns = room.Game.Hands.SelectMany(h => h.Turns).Select(t => t.ToData()).ToList();
        return TypedResults.Ok(turns);
    }
}

