using Karata.Kit.Cards.Models;
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
[Route("api/users")]
public class UserController(CurrentUserService<TriviaContext, User> current, TriviaContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<UserData>> GetUsers()
    {
        var user = await current.RequireAsync();
        return await context.Users
            .Where(u => u.Id != user.Id)
            .Select(u => u.AsUserData())
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserData>> GetUser(string id) => await context.Users.FindAsync(id) switch
    {
        { } user => user.AsUserData(),
        null => NotFound(),
    };
}