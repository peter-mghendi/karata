using Karata.Cards.Data;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Cards.Handlers;

public static class ActivityHandler
{
    public static async Task<Ok<List<ActivityData>>> ListActivity([FromServices] KarataContext context)
    {
        var activity = await context.Activities.OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .Select(a => a.ToData())
            .ToListAsync();

        return TypedResults.Ok(activity);
    }
}