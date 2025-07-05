using Karata.Server.Data;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController(KarataContext context) : ControllerBase
{
    [HttpGet]
    public async Task<List<ActivityData>> List() =>
        await context.Activities.OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .Select(a => a.ToData())
            .ToListAsync();
}