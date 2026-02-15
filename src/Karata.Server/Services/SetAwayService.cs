using Karata.Kit.Domain.Models;
using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support.Exceptions;
using Microsoft.AspNetCore.SignalR;
using static Karata.Kit.Domain.Models.HandStatus;

namespace Karata.Server.Services;

/// <summary>
/// Orchestrates turn processing: validation, delta generation, state mutation, notifications, and persistence.
/// </summary>
public class SetAwayService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    KarataContext context,
    Guid room,
    string player
) : LiveRoomAwareService(players, spectators, room, player)
{
    public async Task ExecuteAsync(long voideeId)
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var hand = room.Game.Hands.Single(h => h.Id == voideeId);
        
        if (CallerPlayerId != room.Administrator.Id) throw new UnauthorizedActionException();

        hand.Status = Away;
        await RecomputeTurn(room, hand);
        await RedelegateAdministration(room, hand.Player);

        await RoomPlayers.UpdateHandStatus(hand.Id, hand.Status);
        await RoomSpectators.UpdateHandStatus(hand.Id, hand.Status);
        await context.SaveChangesAsync();
    }

    private async Task RecomputeTurn(Room room, Hand player)
    {
        if (room.Game.CurrentHand.Id != player.Id) return;
        if (room.Game.Hands.Count(hand => hand.Status is Online or Offline) <= 1) return;
        
        room.Game.CurrentHand.Turns.Add(new Turn
        {
            Type = TurnType.Void,
            Hand = room.Game.CurrentHand,
            CreatedAt = DateTimeOffset.UtcNow
        });
        
        GameTurns.Advance(room.Game);
        await RoomPlayers.UpdateTurn(room.Game.CurrentTurn);
        await RoomSpectators.UpdateTurn(room.Game.CurrentTurn);
    }

    private async Task RedelegateAdministration(Room room, User user)
    {
        if (room.Administrator.Id != user.Id) return;
        if (room.NextEligibleAdministrator is not {} administrator) return;
        
        room.Administrator = administrator;
        await RoomPlayers.UpdateAdministrator(room.Administrator);
        await RoomSpectators.UpdateAdministrator(room.Administrator);
    }
}