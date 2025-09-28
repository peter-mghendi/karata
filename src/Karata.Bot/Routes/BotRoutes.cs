using System.Text.Json;
using Karata.Bot.Infrastructure.Security;
using Karata.Bot.Services;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Bot.Routes;

public static class BotRoutes
{
    public static void MapBotRoutes(this WebApplication routes)
    {
        var api = routes.MapGroup("/api");

        api.MapPost("/hands", async (
            [FromServices] BotSessionManager bots,
            [FromServices] IHttpClientFactory factory,
            [FromServices] ILogger<Program> logger,
            [FromServices] KeycloakAccessTokenProvider tokens,
            [FromBody] BotInvitation invitation
        ) =>
        {
            using var http = factory.CreateClient("KarataClient");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms/{invitation.Room}");
            request.Headers.Add("Authorization", $"Bearer {await tokens.GetAccessTokenAsync()}");

            var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var room = JsonSerializer.Deserialize<RoomData>(body, options) ?? throw new BadHttpRequestException("Invalid room data.");

            var hand = await bots.StartAsync(room.Id.ToString(), invitation.Password);
            return Results.Accepted($"/api/hands/{hand.Id}");
        });
    }
}