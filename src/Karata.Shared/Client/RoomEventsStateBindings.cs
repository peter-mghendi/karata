using System.Reactive.Linq;
using Karata.Pebble.StateActions;
using Karata.Shared.Models;
using Karata.Shared.State;

namespace Karata.Shared.Client;

public static class RoomEventsStateBindings  
{
    public static IDisposable BindRoomState(this RoomEvents e, RoomState room)
    {
        var actions = Observable.Merge<StateAction<RoomData>>(
            e.AddHandToRoom.Select(x => new RoomState.AddHandToRoom(x.Id, x.User, x.Status)),
            e.MoveCardsFromDeckToHand.Select(x => new RoomState.MoveCardsFromDeckToHand(x.User, [..x.Cards])),
            e.MoveCardsFromDeckToPile.Select(cs => new RoomState.MoveCardsFromDeckToPile([..cs])),
            e.MoveCardsFromHandToPile.Select(x => new RoomState.MoveCardsFromHandToPile(x.User, [..x.Cards], x.Visible)),
            e.ReceiveChat.Select(m => new RoomState.ReceiveChat(m)),
            e.ReclaimPile.Select(_ => new RoomState.ReclaimPile()),
            e.RemoveHandFromRoom.Select(u => new RoomState.RemoveHandFromRoom(u)),
            e.SetCurrentRequest.Select(c => new RoomState.SetCurrentRequest(c)),
            e.UpdateAdministrator.Select(a => new RoomState.UpdateAdministrator(a)),
            e.UpdateGameStatus.Select(s => new RoomState.UpdateGameStatus(s)),
            e.UpdateHandStatus.Select(x => new RoomState.UpdateHandStatus(x.User, x.Status)),
            e.UpdatePick.Select(n => new RoomState.UpdatePick(n)),
            e.UpdateTurn.Select(t => new RoomState.UpdateTurn(t))
        );

        return actions.Subscribe(room.Mutate);
    }
}