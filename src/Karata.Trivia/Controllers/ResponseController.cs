using Karata.Kit.Trivia.Models.Enum;
using Karata.Kit.Trivia.Models.Request;
using Karata.Kit.Trivia.Models.Response;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Hubs;
using Karata.Trivia.Hubs.Clients;
using Karata.Trivia.Infrastructure;
using Karata.Trivia.Models;
using Karata.Trivia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/games/{identifier:guid}/rounds/{index:int}/responses")]
public class ResponseController(
    CurrentUserService current,
    KarataTriviaContext context,
    IHubContext<NotificationHub, INotificationHubClient> hub
) : ControllerBase
{
    // POST: api/games/0085f04e-8a30-449e-91e1-38899b4d3ed5/rounds/3/responses
    [HttpPost]
    public async Task<ActionResult<IEnumerable<ResponseResponse>>> PostResponse(
        Guid identifier,
        int index,
        [FromBody] CreateResponseRequest request
    )
    {
        var game = await GetGameAsync(identifier);
        if (game is null)
        {
            return NotFound();
        }

        var user = await current.RequireAsync();
        if (game.Players.All(player => player.Id != user.Id))
        {
            return BadRequest();
        }

        var round = game.Rounds.SingleOrDefault(r => r.Index == index);
        if (round is null)
        {
            return BadRequest();
        }

        if (round.Responses.Any(r => r.User.Id == user.Id))
        {
            return Conflict();
        }

        var choice = round.Question.Choices.SingleOrDefault(c => c.Id == request.ChoiceId);
        if (choice is null && request.ChoiceId is not 0)
        {
            return BadRequest();
        }

        var response = new Response
        {
            TimeLeft = request.TimeLeft,
            Points = CalculatePoints(round, choice, request.TimeLeft),
            Choice = choice,
            User = user!,
            Round = round,
        };

        round.Responses.Add(response);

        if (round.Index is 6 && user.Id == game.PlayerTwo.Id)
        {
            var notification = new Notification
            {
                Action = NotificationAction.Results,
                SentAt = DateTimeOffset.UtcNow,
                Game = game,
                Recipient = game.PlayerOne
            };
            context.Notifications.Add(notification);

            await context.Entry(game.PlayerOne).Collection(u => u.NotificationSubscriptions).LoadAsync();
            await hub.Clients.User(notification.Recipient.Id).ReceiveNotification(notification.AsResponse());
            await PushNotificationService.SendNotificationAsync(notification);
        }

        await context.SaveChangesAsync();

        // User has responded, do not obfuscate the response.
        return round.Responses.Select(r => r.AsResponse()).ToList();
    }

    private async Task<Game?> GetGameAsync(Guid identifier) => await context.Games
        .Include(g => g.Topic)
        .Include(g => g.PlayerOne)
        .Include(g => g.PlayerTwo)
        .Include(g => g.Rounds)
        .ThenInclude(r => r.Question)
        .ThenInclude(q => q.Choices)
        .Include(g => g.Rounds)
        .ThenInclude(r => r.Responses)
        .ThenInclude(r => r.Choice)
        .Include(g => g.Rounds)
        .ThenInclude(r => r.Responses)
        .ThenInclude(r => r.User)
        .SingleOrDefaultAsync(g => g.Identifier == identifier);


    private static int CalculatePoints(Round round, Choice? choice, int time)
    {
        if (choice is null or { IsCorrect: false })
        {
            return 0;
        }

        var points = time + 10;
        return round.Index + 1 == GameService.GameRounds ? points * 2 : points;
    }
}