using Karata.Cards.Data;
using Karata.Kit.Cards.Models;
using Karata.Kit.Common.Models;
using Karata.Kit.Configuration;
using Karata.Kit.Platform;
using Karata.Runtime.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Karata.Kit.Cards.Models.RoomVisibility;

namespace Karata.Cards.Handlers;

public static class RoomHandler
{
    private const int MaxPageSize = 20;

    public static async Task<Ok<Page<RoomData>>> QueryRooms(
        [FromServices] CardsContext context,
        [FromBody] RoomQuery query
    )
    {
        var limit = Math.Clamp(query.Limit, 1, MaxPageSize);
        var page = Math.Max(query.Page, 1);

        IQueryable<Room> rooms = context.Rooms;

        if (query.Filter is { } filter)
        {
            if (filter.GameStatuses is { Length: > 0 })
                rooms = rooms.Where(r => filter.GameStatuses.Contains(r.Game.Status));

            if (filter.Visibilities is { Length: > 0 })
                rooms = rooms.Where(r => filter.Visibilities.Contains(r.Visibility));

            if (filter.MinHandCount is { } min)
                rooms = rooms.Where(r => r.Game.Hands.Count >= min);

            if (filter.MaxHandCount is { } max)
                rooms = rooms.Where(r => r.Game.Hands.Count <= max);

            if (filter.Creators is { Length: > 0 })
                rooms = rooms.Where(r => filter.Creators.Contains(r.Creator.Id));

            if (filter.Administrators is { Length: > 0 })
                rooms = rooms.Where(r => filter.Administrators.Contains(r.Administrator.Id));
        }

        var total = await rooms.CountAsync();
        var items = await rooms
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(r => r.ToData())
            .ToListAsync();

        return TypedResults.Ok(new Page<RoomData>(page, items.Count, total, items));
    }

    public static async Task<Results<Ok<RoomData>, BadRequest, NotFound>> GetRoom(
        [FromServices] CardsContext context,
        [FromRoute] string id
    )
    {
        if (!Guid.TryParse(id, out var guid)) return TypedResults.BadRequest();
        var room = await context.Rooms.FindAsync(guid);

        if (room == null) return TypedResults.NotFound();
        return TypedResults.Ok(room.ToData());
    }

    public static async Task<Results<CreatedAtRoute<RoomData>, UnauthorizedHttpResult>> CreateRoom(
        [FromServices] Client platform,
        [FromServices] CardsContext context,
        [FromServices] CurrentUserService<CardsContext, User> current,
        [FromKeyedServices(nameof(Configuration.Web))]
        HostConfiguration web,
        [FromBody] RoomRequest request
    )
    {
        try
        {
            var user = await current.RequireAsync();
            var room = new Room
            {
                Visibility = request.Visibility, Administrator = user, Creator = user, CreatedAt = DateTimeOffset.UtcNow
            };

            room.Game.Hands.Add(new Hand { Player = user, Status = HandStatus.Invited });

            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            if (room.Visibility is Public)
                await platform.Activity.CreateAsync(
                    new()
                    {
                        Text = $"{room.Creator.Username} has started a game.",
                        Actions = [new("Check it out!", new Uri($"{web.Host}/cards/game/{room.Id}"), "primary")],
                        Metadata = new() { ["room"] = room.Id.ToString() },
                        OccurredAt = room.CreatedAt
                    }
                );

            return TypedResults.CreatedAtRoute(room.ToData(), nameof(GetRoom), new { id = room.Id });
        }
        catch (UnauthorizedAccessException)
        {
            return TypedResults.Unauthorized();
        }
    }
}