using Karata.Kit.Common.Models;
using Karata.Kit.Platform;
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

// TODO: [Legacy]: Change this to a Minimal API handler
[Authorize]
[ApiController]
[Route("api/games")]
public class GameController(TriviaContext context, ILogger<GameController> logger) : ControllerBase
{
    private const int MaxPageSize = 20;

    [HttpGet]
    public async Task<ActionResult<Page<GameData>>> GetGames([FromQuery] GameQuery query)
    {
        var limit = Math.Clamp(query.Limit, 1, MaxPageSize);
        var page = Math.Max(query.Page, 1);

        IQueryable<Game> games = context.Games;
        
        var total = await games.CountAsync();
        var items = await games
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(r => r.ToData())
            .ToListAsync();
        
        return new Page<GameData>(page, items.Count, total, items);
    }

    [HttpPost]
    public async Task<ActionResult<GameData>> CreateGame(
        [FromServices] CurrentUserService<TriviaContext, User> current,
        [FromServices] GameService service,
        [FromServices] Client platform,
        [FromBody] GameRequest request
    )
    {
        var creator = await current.RequireAsync();
        logger.LogInformation("User {UserId} is creating game in topic {TopicId}.", creator.Id, request.TopicId);

        try
        {
            var game = await service.CreateGame(creator, request);
            return CreatedAtAction("GetGame", new { id = game.Id }, game.ToData());
        }
        catch (BadHttpRequestException)
        {
            return BadRequest();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameData>> GetGame(Guid id)
    {
        var game = await context.Games
            .Include(g => g.Topic)
            .Include(g => g.Players)
            .SingleOrDefaultAsync(g => g.Id == id);

        return game switch
        {
            null => NotFound(),
            _ => game.ToData()
        };
    }
}