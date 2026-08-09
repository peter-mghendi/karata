using Karata.Kit.Platform;
using Karata.Kit.Trivia.Models.Request;
using Karata.Kit.Trivia.Models.Response;
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
[Route("api/games")]
public class GameController(TriviaContext context, ILogger<GameController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameResponse>>> GetGames() => await context.Games
        .Select(g => g.AsResponse())
        .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<GameResponse>> CreateGame(
        [FromServices] CurrentUserService<TriviaContext, User> current,
        [FromServices] GameService service,
        [FromServices] Client platform,
        [FromBody] CreateGameRequest request
    )
    {
        var creator = await current.RequireAsync();
        logger.LogInformation("User {UserId} is creating game in topic {TopicId}.", creator.Id, request.TopicId);

        try
        {
            var game = await service.CreateGame(creator, request);
            return CreatedAtAction("GetGame", new { identifier = game.Identifier }, game.AsResponse());
        }
        catch (BadHttpRequestException)
        {
            return BadRequest();
        }
    }

    [HttpGet("{identifier:guid}")]
    public async Task<ActionResult<GameResponse>> GetGame(Guid identifier)
    {
        var game = await context.Games
            .Include(g => g.Topic)
            .Include(g => g.PlayerOne)
            .Include(g => g.PlayerTwo)
            .SingleOrDefaultAsync(g => g.Identifier == identifier);

        if (game is null) return NotFound();

        return game.AsResponse();
    }
}