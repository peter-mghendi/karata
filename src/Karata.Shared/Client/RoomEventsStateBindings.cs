using System.Reactive.Linq;
using Karata.Pebble.StateActions;
using Karata.Shared.Models;
using Karata.Shared.State;

namespace Karata.Shared.Client;

public static class RoomEventsStateBindings  
{
    public static IDisposable BindRoomState(this RoomEvents e, RoomState room, Func<RoomState, HandData>? selector = null)
    {
        var actions = Observable.Merge<StateAction<RoomData>>(
            e.AddHandToRoom.Select(x => new RoomState.AddHandToRoom(x.Id, x.User, x.Status)),
            e.MoveCardCountFromDeckToHand.Select(x => new RoomState.MoveCardCountFromDeckToHand(x.User, x.Count)),
            e.MoveCardsFromDeckToPile.Select(cs => new RoomState.MoveCardsFromDeckToPile([..cs])),
            e.MoveCardsFromHandToPile.Select(x => new RoomState.MoveCardsFromHandToPile(x.User, [..x.Cards], selector is not null && selector(room).Player == x.User)),
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

        if (selector is not null)
        {
            actions = actions.Merge(e.MoveCardsFromDeckToHand.Select(cs => new RoomState.MoveCardsFromDeckToHand(selector(room).Player, [..cs])));
        }

        return actions.Subscribe(room.Mutate);
    }
}