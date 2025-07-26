using Karata.Server.Data;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Karata.Server.Controllers;

[ApiController]
[Route("api/rooms/{id}/turns")]
public class TurnController(KarataContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TurnData>>> Get(string id)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest();
        var room = await context.Rooms.FindAsync(guid);

        if (room == null) return NotFound();
        
        
        
        return room.Game.Hands.SelectMany(h => h.Turns).Select(t => t.ToData()).ToList();
    }
}

