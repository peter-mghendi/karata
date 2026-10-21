using Karata.Kit.Trivia.Models;
using Karata.Runtime.Infrastructure;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Models;
using Karata.Trivia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/games/{identifier:guid}/rounds/{index:int}/responses")]
public class ResponseController(CurrentUserService<TriviaContext, User> current, TriviaContext context) : ControllerBase
{
    // TODO: [Notifications] Send notification
    // POST: api/games/0085f04e-8a30-449e-91e1-38899b4d3ed5/rounds/3/responses
    [HttpPost]
    public async Task<ActionResult<IEnumerable<ResponseData>>> PostResponse(
        [FromRoute] Guid identifier,
        [FromRoute] int index,
        [FromBody] ResponseRequest request
    )
    {
        var game = await GetGameAsync(identifier);
        if (game is null) return NotFound();

        var user = await current.RequireAsync();
        if (game.Players.All(player => player.Id != user.Id)) return BadRequest();

        var round = game.Rounds.SingleOrDefault(r => r.Index == index);
        if (round is null) return BadRequest();

        if (round.Responses.Any(r => r.User.Id == user.Id)) return Conflict();

        var choice = round.Question.Choices.SingleOrDefault(c => c.Id == request.ChoiceId);
        if (choice is null && request.ChoiceId is not 0) return BadRequest();

        var response = new Response
        {
            TimeLeft = request.TimeLeft,
            Score = CalculatePoints(round, choice, request.TimeLeft),
            SubmittedAt = DateTimeOffset.UtcNow,
            GameId = game.Id,
            Choice = choice,
            User = user,
            Round = round,
        };

        round.Responses.Add(response);
        await context.SaveChangesAsync();

        // User has responded, do not obfuscate the response.
        return round.Responses.Select(r => r.ToData()).ToList();
    }

    private async Task<Game?> GetGameAsync(Guid id) => await context.Games
        .Include(g => g.Topic)
        .Include(g => g.Players)
        .Include(g => g.Rounds)
        .ThenInclude(r => r.Question)
        .ThenInclude(q => q.Choices)
        .Include(g => g.Rounds)
        .ThenInclude(r => r.Responses)
        .ThenInclude(r => r.Choice)
        .Include(g => g.Rounds)
        .ThenInclude(r => r.Responses)
        .ThenInclude(r => r.User)
        .SingleOrDefaultAsync(g => g.Id == id);


    private static int CalculatePoints(Round round, Choice? choice, int time) => choice switch
    {
        { IsCorrect: true } when round.Index + 1 == GameService.GameRounds => (time + 10) * 2,
        { IsCorrect: true } => time + 10,
        null or { IsCorrect: false } => 0
    };
}