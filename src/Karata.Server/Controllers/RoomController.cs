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
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly KarataContext _context;

    public RoomController(ILogger<RoomController> logger, KarataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IEnumerable<RoomData> List() => []; // TODO: Before implementing, figure out how much stuff is loaded.

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomData>> Get(string id)
    {
        // TODO: Check if the room has a password (or if current user is already a member), and if one is provided, check if it's correct
        if (!Guid.TryParse(id, out var guid)) return BadRequest();
        var room = await _context.Rooms.FindAsync(guid);
        
        if (room == null) return NotFound();
        return room.ToData();
    }

    [HttpPost]
    public async Task<ActionResult<RoomData>> Post([FromServices] UserManager<User> userManager)
    {
        var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var room = new Room { Creator = user };
        room.Game.Hands.Add(new Hand { Player = user });
    
        // if (!string.IsNullOrWhiteSpace(password))
        // {
        //     room.Salt = IPasswordService.GenerateSalt();
        //     room.Hash = _passwordService.HashPassword(Encoding.UTF8.GetBytes(password), room.Salt);
        // }
        
        _ = _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = room.Id }, room.ToData());
    }
}