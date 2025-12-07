using Karata.Kit.Application.Client;
using Karata.Kit.Bot.Services;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Bot.Endpoints;

// TODO: [IEndpointRouteBuilder] This needs to move into Karata.Kit.Bot
public static class BotEndpoints
{
    extension(IEndpointRouteBuilder routes)
    {
        public void MapBotStrategy(string name, IBotStrategy strategy)
        {
            var bot = routes.MapGroup($"/api/bots/{name}");

            bot.MapGet("", () => HandleDetails(strategy));
            bot.MapPost(
                "/hands",
                async (
                    [FromServices] BotSessionManager bots,
                    [FromServices] KarataClient karata,
                    [FromBody] BotInvitation invitation
                ) => await HandleInvitationAsync(bots, karata, invitation, strategy)
            );
        }
    }

    private static Ok<BotData> HandleDetails(IBotStrategy strategy) => TypedResults.Ok(strategy.Data);

    private static async Task<Results<Accepted, NotFound>> HandleInvitationAsync(
        BotSessionManager bots,
        KarataClient karata,
        BotInvitation invitation,
        IBotStrategy strategy
    )
    {
        if (await karata.Rooms.GetAsync(invitation.Room) is not { } room) return TypedResults.NotFound();

        var hand = await bots.StartAsync(strategy, room.Id, invitation.Password);
        return TypedResults.Accepted($"/api/hands/{hand.Id}");
    }
}