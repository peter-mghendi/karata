using Karata.Kit.Trivia.Models;
using Karata.Runtime.Infrastructure;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/games/{identifier:guid}/rounds")]
public class RoundController(CurrentUserService<TriviaContext, User> current, TriviaContext context) : ControllerBase
{
    // GET: api/games/0085f04e-8a30-449e-91e1-38899b4d3ed5/rounds
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoundData>>> GetRounds(Guid identifier)
    {
        var user = await current.RequireAsync();
        var game = await GetGameAsync(identifier);
        if (game is null) return BadRequest();

        return game.Rounds
            .Select(round => CreateRedactedResponse(round, user))
            .OrderBy(r => r.Index)
            .ToList();
    }

    // GET: api/games/0085f04e-8a30-449e-91e1-38899b4d3ed5/rounds/3
    [HttpGet("{index:int}")]
    public async Task<ActionResult<RoundData>> GetRound(Guid identifier, int index)
    {
        var user = await current.RequireAsync();
        var game = await GetGameAsync(identifier);
        if (game is null) return NotFound();

        var round = game.Rounds.SingleOrDefault(r => r.Index == index);
        if (round is null) return BadRequest();

        return CreateRedactedResponse(round, user);
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

    private static RoundData CreateRedactedResponse(Round round, User user)
    {
        var data = round.ToData();
        if (round.Responses.Any(response => response.User.Id == user.Id)) return data;

        return data with
        {
            Question = data.Question with
            {
                Choices = [..data.Question.Choices.Select(choice => choice with { IsCorrect = false })]
            },
            Responses = [..data.Responses.Select(response => response with { Choice = null, Points = 0 })]
        };
    }
}