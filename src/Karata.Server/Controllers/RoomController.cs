using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Karata.Server.Data;
using System.Security.Claims;

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
    public IEnumerable<UIRoom> List() => new List<UIRoom>();

    [HttpGet("{link}")]
    public async Task<ActionResult<UIRoom>> Get(string id)
    {
        // TODO: Check if the room has a password, and if one is provided, check if it's correct
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        return room.ToUI();
    }

    [HttpPost]
    public async Task<ActionResult<UIRoom>> Post([FromServices] UserManager<User> userManager)
    {
        var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        var room = new Room { Creator = user };
    
        // if (!string.IsNullOrWhiteSpace(password))
        // {
        //     room.Salt = PasswordService.GenerateSalt();
        //     room.Hash = _passwordService.HashPassword(Encoding.UTF8.GetBytes(password), room.Salt);
        // }
        
        _ = _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = room.Id.ToString() }, room.ToUI());
    }
}
