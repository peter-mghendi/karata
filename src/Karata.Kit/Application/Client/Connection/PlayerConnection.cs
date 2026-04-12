using System.Collections.Concurrent;
using Karata.Cards;
using Karata.Kit.Application.Client.State;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Kit.Application.Client.Connection;

public sealed partial class PlayerConnection(Uri host) : IUserConnection<PlayerConnection.SessionParameters, PlayerConnection.Session>
{
    public sealed record SessionParameters(
        Func<Task<string?>> OnRequestPassword,
        Func<bool, Task<Card?>> OnRequestCard,
        Func<Task<bool>> OnRequestLastCard
    ) : IUserConnection.ISessionParameters;

    private const string HubPath = "/hubs/game/play";

    public HubConnection? Hub { get; private set; }

    public ConcurrentDictionary<Guid, Session> _sessions { get; } = new();
    public required Func<Task<string?>> AccessTokenProvider { get; init; }

    public async Task StartAsync(CancellationToken ct = default)
    {
        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(host, HubPath), o => o.AccessTokenProvider = AccessTokenProvider)
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
        Hub.On<Guid, ChatData>(nameof(RoomEvents.Chat), (roomId, chat) =>
        {
            Route(roomId, session => session.Events.OnChat(chat));
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
        Hub.On<Guid>(nameof(RoomEvents.TurnAcknowledged), roomId => Route(roomId, session =>
        {
            session.Events.OnTurnAcknowledged();
        }));
        Hub.On<Guid, TurnResolution>(nameof(RoomEvents.TurnCommitted), (roomId, resolution) =>
        {
            Route(roomId, session => session.Events.OnTurnCommitted(resolution));
        });
        Hub.On<Guid, UserData>(nameof(RoomEvents.UpdateAdministrator), (roomId, user) =>
        {
            Route(roomId, session => session.Events.OnUpdateAdministrator(user));
        });
        Hub.On<Guid, GameStatus>(nameof(RoomEvents.UpdateGameStatus), (roomId, status) =>
        {
            Route(roomId, session => session.Events.OnUpdateGameStatus(status));
        });
        Hub.On<Guid, long, HandStatus>(nameof(RoomEvents.UpdateHandStatus), (roomId, handId, status) =>
        {
            Route(roomId, session => session.Events.OnUpdateHandStatus(handId, status));
        });

        Hub.Reconnecting += ex =>
        {
            Console.WriteLine("SignalR reconnecting: " + ex?.Message);
            return Task.CompletedTask;
        };

        Hub.Reconnected += id =>
        {
            Console.WriteLine("SignalR reconnected");
            return Task.CompletedTask;
        };

        Hub.Closed += ex =>
        {
            Console.WriteLine("SignalR closed: " + ex?.Message);
            return Task.CompletedTask;
        };

        await Hub.StartAsync(ct);
    }

    public Session Spawn(Guid roomId, SessionParameters parameters)
    {
        if (Hub is null) throw new InvalidOperationException("Hub not initialized");
        
        var session = new Session(roomId, Hub);
        if (!_sessions.TryAdd(roomId, session)) throw new InvalidOperationException("Already joined");
        
        
        // TODO: [Regression] Scope this to session
        Hub.On<string?>("PromptPasscode", parameters.OnRequestPassword);
        Hub.On<bool, Card?>("PromptCardRequest", parameters.OnRequestCard);
        Hub.On<bool>("PromptLastCardRequest", parameters.OnRequestLastCard);
        
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
        if (_sessions.TryGetValue(roomId, out var session)) action(session);
    }
}