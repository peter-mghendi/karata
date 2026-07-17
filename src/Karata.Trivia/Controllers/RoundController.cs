using Karata.Kit.Trivia.Models.Response;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Infrastructure;
using Karata.Trivia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/games/{identifier:guid}/rounds")]
public class RoundController(CurrentUserService current, KarataTriviaContext context) : ControllerBase
{
    // GET: api/games/0085f04e-8a30-449e-91e1-38899b4d3ed5/rounds
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoundResponse>>> GetRounds(Guid identifier)
    {
        var user = await current.RequireAsync();
        var game = await GetGameAsync(identifier);
        if (game is null)
        {
            return BadRequest();
        }

        return game.Rounds
            .Select(round => CreateObfuscatedResponse(round, user))
            .OrderBy(r => r.Index)
            .ToList();
    }

    // GET: api/games/0085f04e-8a30-449e-91e1-38899b4d3ed5/rounds/3
    [HttpGet("{index:int}")]
    public async Task<ActionResult<RoundResponse>> GetRound(Guid identifier, int index)
    {
        var user = await current.RequireAsync();
        var game = await GetGameAsync(identifier);
        if (game is null)
        {
            return NotFound();
        }

        var round = game.Rounds.SingleOrDefault(r => r.Index == index);
        if (round is null)
        {
            return BadRequest();
        }

        return CreateObfuscatedResponse(round, user);
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

    private RoundResponse CreateObfuscatedResponse(Round round, User user)
    {
        // Obfuscate correct answer if we do not have a response for this user yet.
        // I will regret this code tomorrow
        // UPDATE: Yep.
        var response = round.AsResponse();
        return round.Responses.All(r => r.User.Id != user.Id)
            ? response with
            {
                Responses = response.Responses.Select(r => r.Choice is not null
                        ? r with { Choice = r.Choice with { IsCorrect = false } }
                        : r
                    )
                    .ToList(),
                Question = response.Question with
                {
                    Choices = response.Question.Choices.Select(c => c with { IsCorrect = false }).ToList()
                }
            }
            : response;
    }
}