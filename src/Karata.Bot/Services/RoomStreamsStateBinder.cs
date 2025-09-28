using System.Collections.Immutable;
using Karata.Cards;
using Karata.Pebble.Interceptors;
using Karata.Shared.Models;
using Karata.Shared.State;

namespace Karata.Bot.Services;

using System.Reactive.Disposables;

public sealed class RoomStreamsStateBinder : IDisposable
{
    private readonly CompositeDisposable _subs = new();
    private RoomState _room; // TODO: Add "Reset" method to Pebble and make these readonly
    private TurnState _turn; // TODO: Add "Reset" method to Pebble and make these readonly

    public RoomStreamsStateBinder(RoomState room, TurnState turn, RoomStreams streams, ILoggerFactory log, string player)
    {
        _room = room; 
        _turn = turn;

        streams.AddToRoom.Subscribe(r =>
        {
            _room = new RoomState(r, [
                new LoggingInterceptor<RoomData>(log),
                new TimingInterceptor<RoomData>(log)
            ]);
            _turn = new TurnState([], [
                new LoggingInterceptor<ImmutableList<Card>>(log),
                new TimingInterceptor<ImmutableList<Card>>(log)
            ]);
        }).AddTo(_subs);
        streams.AddHandToRoom.Subscribe(x => _room.Mutate(new RoomState.AddHandToRoom(x.id, x.user, x.status))).AddTo(_subs);
        streams.MoveCardCountFromDeckToHand.Subscribe(x => _room.Mutate(new RoomState.MoveCardCountFromDeckToHand(x.user, x.count))).AddTo(_subs);
        streams.MoveCardsFromDeckToHand.Subscribe(c => _room.Mutate(new RoomState.MoveCardsFromDeckToHand(MyHand().Player, [..c]))).AddTo(_subs);
        streams.MoveCardsFromDeckToPile.Subscribe(c => _room.Mutate(new RoomState.MoveCardsFromDeckToPile([..c]))).AddTo(_subs);
        streams.MoveCardsFromHandToPile.Subscribe(x => _room.Mutate(new RoomState.MoveCardsFromHandToPile(x.user, [..x.cards], MyHand().Player == x.user))).AddTo(_subs);
        streams.NotifyTurnProcessed.Subscribe(_ => _turn.Mutate(new TurnState.Clear())).AddTo(_subs);
        streams.ReceiveChat.Subscribe(m => _room.Mutate(new RoomState.ReceiveChat(m))).AddTo(_subs);
        streams.ReclaimPile.Subscribe(_ => _room.Mutate(new RoomState.ReclaimPile())).AddTo(_subs);
        streams.RemoveHandFromRoom.Subscribe(u => _room.Mutate(new RoomState.RemoveHandFromRoom(u))).AddTo(_subs);
        streams.SetCurrentRequest.Subscribe(c => _room.Mutate(new RoomState.SetCurrentRequest(c))).AddTo(_subs);
        streams.UpdateAdministrator.Subscribe(a => _room.Mutate(new RoomState.UpdateAdministrator(a))).AddTo(_subs);
        streams.UpdateGameStatus.Subscribe(s => _room.Mutate(new RoomState.UpdateGameStatus(s))).AddTo(_subs);
        streams.UpdateHandStatus.Subscribe(x => _room.Mutate(new RoomState.UpdateHandStatus(x.user, x.status))).AddTo(_subs);
        streams.UpdatePick.Subscribe(n => _room.Mutate(new RoomState.UpdatePick(n))).AddTo(_subs);
        streams.UpdateTurn.Subscribe(t => _room.Mutate(new RoomState.UpdateTurn(t))).AddTo(_subs);
        return;

        HandData MyHand() => _room.State.Game.Hands.Single(h => h.Player.Id == player);
    }

    public void Dispose() => _subs.Dispose();
}