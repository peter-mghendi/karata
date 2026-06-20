using Karata.Cards.Data;
using Karata.Cards.Hubs;
using Karata.Cards.Hubs.Clients;
using Karata.Cards.Models;
using Karata.Cards.Support.Exceptions;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Cards.Services;

/// <summary>
/// Orchestrates turn processing: validation, delta generation, state mutation, notifications, and persistence.
/// </summary>
public class VoidTurnService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    KarataContext context,
    Guid roomId,
    string player
) : LiveRoomAwareService(players, spectators, roomId, player)
{
    public async Task ExecuteAsync(long voideeId)
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;
        if (room.Game.CurrentHand.Player.Id != room.Administrator.Id) throw new UnauthorizedActionException();
        if (room.Game.CurrentHand.Id != voideeId) throw new InvalidTurnException();
            
        room.Game.CurrentHand.Turns.Add(new Turn
        {
            Type = TurnType.Void,
            Hand = room.Game.CurrentHand,
            CreatedAt = DateTimeOffset.UtcNow
        });
            
        room.Game.AdvanceTurn();
        await context.SaveChangesAsync();

        foreach (var data in from hand in room.Game.Hands select (Hand: hand, Game: Enrich.ForUser(room.Game, hand)))
            await Hand(data.Hand).TurnCommitted(RoomId, data.Game);
        await RoomSpectators.TurnCommitted(RoomId, room.Game);
    }
}