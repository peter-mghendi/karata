using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Karata.Shared.Models;
using Karata.Server.Services;
using Microsoft.AspNetCore.Identity;
using Karata.Server.Data;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IRoomService _roomService;
    private readonly KarataContext _context;

    public RoomController(ILogger<RoomController> logger, IRoomService roomService, KarataContext context)
    {
        _logger = logger;
        _roomService = roomService;
        _context = context;
    }

    [HttpGet]
    public IEnumerable<UIRoom> List() => new List<UIRoom>();

    [HttpGet("{link}")]
    public async Task<UIRoom> Get(string link)
    {
        // TODO: Check if the room has a password, and if one is provided, check if it's correct
        var room = await _roomService.FindByInviteLinkAsync(link);
        return room.ToUI();
    }

    [HttpPost]
    public async Task<ActionResult<UIRoom>> Post([FromServices] UserManager<User> userManager)
    {
        var userId = User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();
    
        // if (!string.IsNullOrWhiteSpace(password))
        // {
        //     room.Salt = PasswordService.GenerateSalt();
        //     room.Hash = _passwordService.HashPassword(Encoding.UTF8.GetBytes(password), room.Salt);
        // }

        var room = await _roomService.CreateAsync(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { link = room.InviteLink }, room.ToUI());
    }
}
