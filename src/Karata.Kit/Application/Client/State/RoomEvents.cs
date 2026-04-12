using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Karata.Cards;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Application.Client.State;

public sealed class RoomEvents : IDisposable
{
    private readonly ISubject<RoomData> _addToRoom = Subject.Synchronize(new Subject<RoomData>());

    private readonly ISubject<(long Id, UserData User, HandStatus Status)> _addHandToRoom =
        Subject.Synchronize(new Subject<(long, UserData, HandStatus)>());

    private readonly ISubject<TurnResolution> _turnCommitted = Subject.Synchronize(new Subject<TurnResolution>());
    private readonly ISubject<GameResultData> _endGame = Subject.Synchronize(new Subject<GameResultData>());

    private readonly ISubject<(long HandId, IReadOnlyList<Card> Cards)> _moveCardsFromDeckToHand =
        Subject.Synchronize(new Subject<(long, IReadOnlyList<Card>)>());

    private readonly ISubject<IReadOnlyList<Card>> _moveCardsFromDeckToPile =
        Subject.Synchronize(new Subject<IReadOnlyList<Card>>());

    private readonly ISubject<(long HandId, IReadOnlyList<Card> Cards, bool Visible)> _moveCardsFromHandToPile =
        Subject.Synchronize(new Subject<(long, IReadOnlyList<Card>, bool Visible)>());

    private readonly ISubject<Unit> _turnAcknowledged = Subject.Synchronize(new Subject<Unit>());
    private readonly ISubject<ChatData> _chat = Subject.Synchronize(new Subject<ChatData>());
    private readonly ISubject<SystemMessage> _systemMessage = Subject.Synchronize(new Subject<SystemMessage>());
    private readonly ISubject<Unit> _reclaimPile = Subject.Synchronize(new Subject<Unit>());
    private readonly ISubject<Unit> _removeFromRoom = Subject.Synchronize(new Subject<Unit>());
    private readonly ISubject<long> _removeHandFromRoom = Subject.Synchronize(new Subject<long>());
    private readonly ISubject<UserData> _updateAdministrator = Subject.Synchronize(new Subject<UserData>());
    private readonly ISubject<GameStatus> _updateGameStatus = Subject.Synchronize(new Subject<GameStatus>());

    private readonly ISubject<(long HandId, HandStatus Status)> _updateHandStatus =
        Subject.Synchronize(new Subject<(long, HandStatus)>());

    public IObservable<RoomData> AddToRoom => _addToRoom.AsObservable();
    public IObservable<(long Id, UserData User, HandStatus Status)> AddHandToRoom => _addHandToRoom.AsObservable();
    public IObservable<TurnResolution> TurnCommitted => _turnCommitted.AsObservable();
    public IObservable<GameResultData> EndGame => _endGame.AsObservable();
    public IObservable<(long HandId, IReadOnlyList<Card> Cards)> MoveCardsFromDeckToHand => _moveCardsFromDeckToHand.AsObservable();
    public IObservable<IReadOnlyList<Card>> MoveCardsFromDeckToPile => _moveCardsFromDeckToPile.AsObservable();
    public IObservable<(long HandId, IReadOnlyList<Card> Cards, bool Visible)> MoveCardsFromHandToPile => _moveCardsFromHandToPile.AsObservable();
    public IObservable<Unit> TurnAcknowledged => _turnAcknowledged.AsObservable();
    public IObservable<ChatData> Chat => _chat.AsObservable();
    public IObservable<SystemMessage> SystemMessage => _systemMessage.AsObservable();
    public IObservable<Unit> ReclaimPile => _reclaimPile.AsObservable();
    public IObservable<Unit> RemoveFromRoom => _removeFromRoom.AsObservable();
    public IObservable<long> RemoveHandFromRoom => _removeHandFromRoom.AsObservable();
    public IObservable<UserData> UpdateAdministrator => _updateAdministrator.AsObservable();
    public IObservable<GameStatus> UpdateGameStatus => _updateGameStatus.AsObservable();
    public IObservable<(long HandId, HandStatus Status)> UpdateHandStatus => _updateHandStatus.AsObservable();

    internal void OnAddToRoom(RoomData r) => _addToRoom.OnNext(r);

    internal void OnAddHandToRoom(long id, UserData user, HandStatus status) => _addHandToRoom.OnNext((id, user, status));

    internal void OnTurnCommitted(TurnResolution resolution) => _turnCommitted.OnNext(resolution);
    internal void OnEndGame(GameResultData result) => _endGame.OnNext(result);

    internal void OnMoveCardsFromDeckToHand(long handId, IReadOnlyList<Card> cards) => _moveCardsFromDeckToHand.OnNext((handId, cards));

    internal void OnMoveCardsFromDeckToPile(IReadOnlyList<Card> cards) => _moveCardsFromDeckToPile.OnNext(cards);

    internal void OnMoveCardsFromHandToPile(long handId, IReadOnlyList<Card> cards, bool visible) =>
        _moveCardsFromHandToPile.OnNext((handId, cards, visible));

    internal void OnTurnAcknowledged() => _turnAcknowledged.OnNext(Unit.Default);

    internal void OnChat(ChatData chat) => _chat.OnNext(chat);

    internal void OnSystemMessage(SystemMessage message) => _systemMessage.OnNext(message);

    internal void OnReclaimPile() => _reclaimPile.OnNext(Unit.Default);

    internal void OnRemoveFromRoom() => _removeFromRoom.OnNext(Unit.Default);

    internal void OnRemoveHandFromRoom(long handId) => _removeHandFromRoom.OnNext(handId);

    internal void OnUpdateAdministrator(UserData user) => _updateAdministrator.OnNext(user);

    internal void OnUpdateGameStatus(GameStatus status) => _updateGameStatus.OnNext(status);

    internal void OnUpdateHandStatus(long handId, HandStatus status) => _updateHandStatus.OnNext((handId, status));
    

    // Subjects are IDisposable in practice; ok to just complete for now
    public void Dispose()
    {
        _addToRoom.OnCompleted();
        _addHandToRoom.OnCompleted();
        _turnCommitted.OnCompleted();
        _endGame.OnCompleted();
        _moveCardsFromDeckToHand.OnCompleted();
        _moveCardsFromDeckToPile.OnCompleted();
        _moveCardsFromHandToPile.OnCompleted();
        _turnAcknowledged.OnCompleted();
        _chat.OnCompleted();
        _systemMessage.OnCompleted();
        _reclaimPile.OnCompleted();
        _removeFromRoom.OnCompleted();
        _removeHandFromRoom.OnCompleted();
        _updateAdministrator.OnCompleted();
        _updateGameStatus.OnCompleted();
        _updateHandStatus.OnCompleted();
    }
}