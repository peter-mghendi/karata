using Karata.Bot.Services;
using Karata.Shared.Client;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Bot.Routes;

public static class BotRoutes
{
    public static void MapBotRoutes(this WebApplication routes)
    {
        var api = routes.MapGroup("/api");
        api.MapPost("/hands", InvitationHandler);
    }

    private static async Task<IResult> InvitationHandler(
        [FromServices] BotSessionManager bots,
        [FromServices] KarataClient karata,
        [FromBody] BotInvitation invitation
    )
    {
        // TODO: Handle room not found
        var room = await karata.Rooms.GetAsync(invitation.Room);
        var hand = await bots.StartAsync(room.Id, invitation.Password);
        return Results.Accepted($"/api/hands/{hand.Id}");
    }
}