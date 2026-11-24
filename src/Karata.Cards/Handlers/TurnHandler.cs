using System.Security.Claims;
using Karata.Cards.Data;
using Karata.Kit.Cards.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Cards.Handlers;

public static class TurnHandler
{
    [HttpGet]
    public static async Task<Results<Ok<List<TurnData>>, BadRequest, NotFound>> ListTurns(
        [FromServices] CardsContext context,
        ClaimsPrincipal principal,
        string id
    )
    {
        if (!Guid.TryParse(id, out var guid)) return TypedResults.BadRequest();
        if (await context.Rooms.FindAsync(guid) is not {} room) return TypedResults.NotFound();
        
        var caller = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        var turns = room.Game.Hands
            .SelectMany(hand => hand.Turns)
            .Where(turn => Visible(turn) || turn.Type is not TurnType.Fail)
            .Select(turn => Visible(turn) ? turn.ToData() : Redacted(turn))
            .ToList();

        return TypedResults.Ok(turns);

        bool Visible(Turn turn) => room.Game.Status is GameStatus.Over || turn.Hand.Player.Id == caller;
    }

    private static TurnData Redacted(Turn turn)
    {
        var data = turn.ToData();
        data.CardsPicked = [..Enumerable.Repeat(new Card(), turn.CardsPicked.Count)];
        return data;
    }
}