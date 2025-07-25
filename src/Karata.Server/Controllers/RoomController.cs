using System.Security.Claims;
using System.Text;
using Karata.Server.Data;
using Karata.Server.Services;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Server.Controllers;

[ApiController]
[Route("api/rooms")]
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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RoomData>> Post(
        [FromServices] IPasswordService passwordService,
        [FromServices] UserManager<User> userManager,
        [FromBody] RoomRequest request
    )
    {
        var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var room = new Room { Administrator = user, Creator = user, CreatedAt = DateTimeOffset.UtcNow };
        room.Game.Hands.Add(new Hand { Player = user, Status = HandStatus.Disconnected});

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            room.Salt = IPasswordService.GenerateSalt();
            room.Hash = passwordService.HashPassword(Encoding.UTF8.GetBytes(request.Password), room.Salt!);
        }

        context.Rooms.Add(room);
        context.Activities.Add(Activity.GameCreated(room));

        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = room.Id }, room.ToData());
    }
}

