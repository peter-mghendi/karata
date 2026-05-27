using System.Collections.Concurrent;
using Karata.Cards;
using Karata.Kit.Application.Client.State;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Kit.Application.Client.Connection;

public sealed partial class SpectatorConnection(Uri host) : IUserConnection<SpectatorConnection.SessionParameters, SpectatorConnection.Session>
{
    private const string HubPath = "/hubs/game/spectate";
    
    public HubConnection? Hub { get; private set; }

    private ConcurrentDictionary<Guid, Session> Sessions { get; } = new();

    public async Task StartAsync(CancellationToken ct = default)
    {
        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(host, HubPath))
            .WithAutomaticReconnect()
            .AddJsonProtocol() // TODO: [Legacy] Use MessagePack
            .Build();

        Hub.On<Guid, RoomData>(nameof(RoomEvents.AddToRoom), (roomId, room) =>
        {
            Route(roomId, session => session.Events.OnAddToRoom(room));
        });
        Hub.On<Guid, long, UserData, HandStatus>(nameof(RoomEvents.AddHandToRoom), (roomId, handId, user, status) =>
        {
            Route(roomId, session => session.Events.OnAddHandToRoom(handId, user, status));
        });
        Hub.On<Guid, GameResultData>(nameof(RoomEvents.EndGame), (roomId, result) =>
        {
            Route(roomId, session => session.Events.OnEndGame(result));
        });
        Hub.On<Guid, long, IReadOnlyList<Card>>(nameof(RoomEvents.MoveCardsFromDeckToHand), (roomId, handId, cards) =>
        {
            Route(roomId, session => session.Events.OnMoveCardsFromDeckToHand(handId, cards));
        });
        Hub.On<Guid, List<Card>>(nameof(RoomEvents.MoveCardsFromDeckToPile), (roomId, cards) =>
        {
            Route(roomId, session => session.Events.OnMoveCardsFromDeckToPile(cards));
        });
        Hub.On<Guid, long, List<Card>, bool>(nameof(RoomEvents.MoveCardsFromHandToPile), (roomId, handId, cards, visible) =>
        {
            Route(roomId, session => session.Events.OnMoveCardsFromHandToPile(handId, cards, visible));
        });
        Hub.On<Guid, SystemMessage>(nameof(RoomEvents.SystemMessage), (roomId, message) =>
        {
            Route(roomId, session => session.Events.OnSystemMessage(message));
        });
        Hub.On<Guid>(nameof(RoomEvents.ReclaimPile), roomId =>
        {
            Route(roomId, session => session.Events.OnReclaimPile());
        });
        Hub.On<Guid>(nameof(RoomEvents.RemoveFromRoom), roomId =>
        {
            Route(roomId, session => session.Events.OnRemoveFromRoom());
        });
        
        Hub.On<Guid, long>(nameof(RoomEvents.RemoveHandFromRoom), (roomId, handId) =>
        {
            Route(roomId, session => session.Events.OnRemoveHandFromRoom(handId));
        });
        Hub.On<Guid, GameData>(nameof(RoomEvents.TurnCommitted), (roomId, game) =>
        {
            Route(roomId, session => session.Events.OnTurnCommitted(game));
        });
        Hub.On<Guid, UserData>(nameof(RoomEvents.UpdateAdministrator), (roomId, user) =>
        {
            Route(roomId, session => session.Events.OnUpdateAdministrator(user));
        });
        Hub.On<Guid, GameData>(nameof(RoomEvents.UpdateGameStatus), (roomId, game) =>
        {
            Route(roomId, session => session.Events.OnUpdateGameStatus(game));
        });
        Hub.On<Guid, long, HandStatus>(nameof(RoomEvents.UpdateHandStatus), (roomId, handId, status) =>
        {
            Route(roomId, session => session.Events.OnUpdateHandStatus(handId, status));
        });

        Hub.Reconnecting += ex =>
        {
            Console.WriteLine($"SignalR reconnecting: {ex?.Message}");
            return Task.CompletedTask;
        };

        Hub.Reconnected += id =>
        {
            Console.WriteLine($"SignalR reconnected: {id}");
            return Task.CompletedTask;
        };

        Hub.Closed += ex =>
        {
            Console.WriteLine("SignalR closed: " + ex?.Message);
            return Task.CompletedTask;
        };
        
        await Hub.StartAsync(ct);
    }

    public Session Spawn(Guid roomId, SessionParameters _)
    {
        if (Hub is null) throw new InvalidOperationException("Hub not initialized");
        
        var session = new Session(roomId, Hub);
        if (!Sessions.TryAdd(roomId, session)) throw new InvalidOperationException("Already joined");

        return session;
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (Hub is null) return;
        await Hub.StopAsync(ct);
        await Hub.DisposeAsync();
    }

    private void Route(Guid roomId, Action<Session> action)
    {
        if (Sessions.TryGetValue(roomId, out var session)) action(session);
    }
}