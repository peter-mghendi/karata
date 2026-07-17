using Karata.Kit.Cards.Models;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController(CurrentUserService current, KarataTriviaContext context) : ControllerBase
{
    // GET: api/users
    [HttpGet]
    public async Task<IEnumerable<UserData>> GetUsers()
    {
        var user = await current.RequireAsync();
        return await context.Users
            .Where(u => u.Id != user.Id)
            .Select(u => u.AsUserData())
            .ToListAsync();
    }

    // GET: api/topics/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserData>> GetUser(string id) => await context.Users.FindAsync(id) switch
    {
        { } user => user.AsUserData(),
        null => NotFound(),
    };
}