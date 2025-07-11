using System.Security.Claims;
using Karata.Server.Data;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomController(KarataContext context) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomData>> Get(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest();
        var room = await context.Rooms.FindAsync(guid);

        if (room == null) return NotFound();
        return room.ToData();
    }

    [HttpPost]
    public async Task<ActionResult<RoomData>> Post([FromServices] UserManager<User> userManager)
    {
        var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var room = new Room { Administrator = user, Creator = user, CreatedAt = DateTimeOffset.UtcNow };
        room.Game.Hands.Add(new Hand { Player = user, Status = HandStatus.Disconnected});

        // if (!string.IsNullOrWhiteSpace(password))
        // {
        //     room.Salt = IPasswordService.GenerateSalt();
        //     room.Hash = _passwordService.HashPassword(Encoding.UTF8.GetBytes(password), room.Salt);
        // }

        context.Rooms.Add(room);
        context.Activities.Add(Activity.GameCreated(room));

        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = room.Id }, room.ToData());
    }
}