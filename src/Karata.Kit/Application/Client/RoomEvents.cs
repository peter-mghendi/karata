using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Karata.Cards;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Application.Client;

public sealed class RoomEvents : IDisposable
{
    private readonly ISubject<RoomData> _addToRoom = Subject.Synchronize(new Subject<RoomData>());

    private readonly ISubject<(long Id, UserData User, HandStatus Status)> _addHandToRoom =
        Subject.Synchronize(new Subject<(long, UserData, HandStatus)>());

    private readonly ISubject<Unit> _endGame = Subject.Synchronize(new Subject<Unit>());

    private readonly ISubject<(UserData User, IReadOnlyList<Card> Cards)> _moveCardsFromDeckToHand =
        Subject.Synchronize(new Subject<(UserData, IReadOnlyList<Card>)>());

    private readonly ISubject<IReadOnlyList<Card>> _moveCardsFromDeckToPile =
        Subject.Synchronize(new Subject<IReadOnlyList<Card>>());

    private readonly ISubject<(UserData User, IReadOnlyList<Card> Cards, bool Visible)> _moveCardsFromHandToPile =
        Subject.Synchronize(new Subject<(UserData, IReadOnlyList<Card>, bool Visible)>());

    private readonly ISubject<Unit> _notifyTurnProcessed = Subject.Synchronize(new Subject<Unit>());
    private readonly ISubject<ChatData> _receiveChat = Subject.Synchronize(new Subject<ChatData>());
    private readonly ISubject<SystemMessage> _receiveSystemMessage = Subject.Synchronize(new Subject<SystemMessage>());
    private readonly ISubject<Unit> _reclaimPile = Subject.Synchronize(new Subject<Unit>());
    private readonly ISubject<Unit> _removeFromRoom = Subject.Synchronize(new Subject<Unit>());
    private readonly ISubject<UserData> _removeHandFromRoom = Subject.Synchronize(new Subject<UserData>());
    private readonly ISubject<Card> _setCurrentRequest = Subject.Synchronize(new Subject<Card>());
    private readonly ISubject<UserData> _updateAdministrator = Subject.Synchronize(new Subject<UserData>());
    private readonly ISubject<GameStatus> _updateGameStatus = Subject.Synchronize(new Subject<GameStatus>());

    private readonly ISubject<(UserData User, HandStatus Status)> _updateHandStatus =
        Subject.Synchronize(new Subject<(UserData, HandStatus)>());

    private readonly ISubject<uint> _updatePick = Subject.Synchronize(new Subject<uint>());
    private readonly ISubject<int> _updateTurn = Subject.Synchronize(new Subject<int>());

    public IObservable<RoomData> AddToRoom => _addToRoom.AsObservable();
    public IObservable<(long Id, UserData User, HandStatus Status)> AddHandToRoom => _addHandToRoom.AsObservable();
    public IObservable<Unit> EndGame => _endGame.AsObservable();
    public IObservable<(UserData User, IReadOnlyList<Card> Cards)> MoveCardsFromDeckToHand => _moveCardsFromDeckToHand.AsObservable();
    public IObservable<IReadOnlyList<Card>> MoveCardsFromDeckToPile => _moveCardsFromDeckToPile.AsObservable();
    public IObservable<(UserData User, IReadOnlyList<Card> Cards, bool Visible)> MoveCardsFromHandToPile => _moveCardsFromHandToPile.AsObservable();
    public IObservable<Unit> NotifyTurnProcessed => _notifyTurnProcessed.AsObservable();
    public IObservable<ChatData> ReceiveChat => _receiveChat.AsObservable();
    public IObservable<SystemMessage> ReceiveSystemMessage => _receiveSystemMessage.AsObservable();
    public IObservable<Unit> ReclaimPile => _reclaimPile.AsObservable();
    public IObservable<Unit> RemoveFromRoom => _removeFromRoom.AsObservable();
    public IObservable<UserData> RemoveHandFromRoom => _removeHandFromRoom.AsObservable();
    public IObservable<Card> SetCurrentRequest => _setCurrentRequest.AsObservable();
    public IObservable<UserData> UpdateAdministrator => _updateAdministrator.AsObservable();
    public IObservable<GameStatus> UpdateGameStatus => _updateGameStatus.AsObservable();
    public IObservable<(UserData User, HandStatus Status)> UpdateHandStatus => _updateHandStatus.AsObservable();
    public IObservable<uint> UpdatePick => _updatePick.AsObservable();
    public IObservable<int> UpdateTurn => _updateTurn.AsObservable();

    internal void OnAddToRoom(RoomData r) => _addToRoom.OnNext(r);

    internal void OnAddHandToRoom(long id, UserData user, HandStatus status) => _addHandToRoom.OnNext((id, user, status));

    internal void OnEndGame() => _endGame.OnNext(Unit.Default);

    internal void OnMoveCardsFromDeckToHand(UserData user, IReadOnlyList<Card> cards) => _moveCardsFromDeckToHand.OnNext((user, cards));

    internal void OnMoveCardsFromDeckToPile(IReadOnlyList<Card> cards) => _moveCardsFromDeckToPile.OnNext(cards);

    internal void OnMoveCardsFromHandToPile(UserData user, IReadOnlyList<Card> cards, bool visible) =>
        _moveCardsFromHandToPile.OnNext((user, cards, visible));

    internal void OnNotifyTurnProcessed() => _notifyTurnProcessed.OnNext(Unit.Default);

    internal void OnReceiveChat(ChatData chat) => _receiveChat.OnNext(chat);

    internal void OnReceiveSystemMessage(SystemMessage message) => _receiveSystemMessage.OnNext(message);

    internal void OnReclaimPile() => _reclaimPile.OnNext(Unit.Default);

    internal void OnRemoveFromRoom() => _removeFromRoom.OnNext(Unit.Default);

    internal void OnRemoveHandFromRoom(UserData user) => _removeHandFromRoom.OnNext(user);

    internal void OnSetCurrentRequest(Card card) => _setCurrentRequest.OnNext(card);

    internal void OnUpdateAdministrator(UserData user) => _updateAdministrator.OnNext(user);

    internal void OnUpdateGameStatus(GameStatus status) => _updateGameStatus.OnNext(status);

    internal void OnUpdateHandStatus(UserData user, HandStatus status) => _updateHandStatus.OnNext((user, status));
    
    internal void OnUpdatePick(uint pick) => _updatePick.OnNext(pick);
    
    internal void OnUpdateTurn(int turn) => _updateTurn.OnNext(turn);

    // Subjects are IDisposable in practice; ok to just complete for now
    public void Dispose()
    {
        _addToRoom.OnCompleted();
        _addHandToRoom.OnCompleted();
        _endGame.OnCompleted();
        _moveCardsFromDeckToHand.OnCompleted();
        _moveCardsFromDeckToPile.OnCompleted();
        _moveCardsFromHandToPile.OnCompleted();
        _notifyTurnProcessed.OnCompleted();
        _receiveChat.OnCompleted();
        _receiveSystemMessage.OnCompleted();
        _reclaimPile.OnCompleted();
        _removeFromRoom.OnCompleted();
        _removeHandFromRoom.OnCompleted();
        _setCurrentRequest.OnCompleted();
        _updateAdministrator.OnCompleted();
        _updateGameStatus.OnCompleted();
        _updateHandStatus.OnCompleted();
        _updatePick.OnCompleted();
        _updateTurn.OnCompleted();
    }
}