using Karata.Kit.Bot.Models;
using Karata.Kit.Bot.Services;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Cards;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Karata.Runtime.Bot.Endpoints;

public static class BotEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public void MapBotStrategy(string name, IBotStrategy strategy)
        {
            var bot = routes.MapGroup($"/api/bots/{name}");

            bot.MapGet("", () => HandleDetails(strategy));
            bot.MapPost(
                "/games",
                async (
                    [FromServices] BotSessionManager bots,
                    [FromServices] Client cards,
                    [FromBody] BotInvitation invitation
                ) => await HandleInvitationAsync(bots, cards, invitation, strategy)
            );
        }
    }

    private static Ok<BotData> HandleDetails(IBotStrategy strategy) => TypedResults.Ok(strategy.Data);

    private static async Task<Results<Accepted, NotFound>> HandleInvitationAsync(
        BotSessionManager bots,
        Client cards,
        BotInvitation invitation,
        IBotStrategy strategy
    )
    {
        if (await cards.Rooms.GetAsync(invitation.Room) is not { } room) return TypedResults.NotFound();

        await bots.StartAsync(strategy, room.Id, invitation.Password);
        return TypedResults.Accepted($"/api/rooms/{room.Id}");
    }
}