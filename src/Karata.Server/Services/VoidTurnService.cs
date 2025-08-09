using System.Text.Json;
using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Engine;
using Karata.Shared.Engine.Exceptions;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using static Karata.Cards.Card.CardFace;
using static Karata.Shared.Models.CardRequestLevel;
using static Karata.Shared.Models.TurnType;
using static Karata.Shared.Models.GameStatus;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Services;

/// <summary>
/// Orchestrates turn processing: validation, delta generation, state mutation, notifications, and persistence.
/// </summary>
public class VoidTurnService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    KarataContext context,
    Guid room,
    string player
) : RoomAwareService(players, spectators, room, player)
{
    public async Task ExecuteAsync(string voideeId)
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;
        if (room.Administrator.Id != CurrentPlayerId) throw new UnauthorizedActionException();
        if (room.Game.CurrentHand.Player.Id != voideeId) throw new InvalidTurnException();
            
        room.Game.CurrentHand.Turns.Add(new Turn
        {
            Type = TurnType.Void,
            Hand = room.Game.CurrentHand,
            CreatedAt = DateTimeOffset.UtcNow
        });
            
        GameTurns.Advance(room.Game);

        await context.SaveChangesAsync();
        await RoomPlayers.UpdateTurn(room.Game.CurrentTurn);
        await RoomSpectators.UpdateTurn(room.Game.CurrentTurn);
    }
}