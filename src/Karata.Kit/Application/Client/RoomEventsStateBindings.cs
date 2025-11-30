using System.Reactive.Linq;
using Karata.Kit.Application.State;
using Karata.Kit.Domain.Models;
using Karata.Pebble.StateActions;

namespace Karata.Kit.Application.Client;

public static class RoomEventsStateBindings  
{
    extension(RoomEvents events)
    {
        public IDisposable BindRoomState(RoomState room)
        {
            var actions = Observable.Merge<StateAction<RoomData>>(
                events.AddHandToRoom.Select(data => new RoomState.AddHandToRoom(data.Id, data.User, data.Status)),
                events.MoveCardsFromDeckToHand.Select(data => new RoomState.MoveCardsFromDeckToHand(data.User, [..data.Cards])),
                events.MoveCardsFromDeckToPile.Select(cards => new RoomState.MoveCardsFromDeckToPile([..cards])),
                events.MoveCardsFromHandToPile.Select(data => new RoomState.MoveCardsFromHandToPile(data.User, [..data.Cards], data.Visible)),
                events.ReceiveChat.Select(chat => new RoomState.ReceiveChat(chat)),
                events.ReclaimPile.Select(_ => new RoomState.ReclaimPile()),
                events.RemoveHandFromRoom.Select(user => new RoomState.RemoveHandFromRoom(user)),
                events.SetCurrentRequest.Select(card => new RoomState.SetCurrentRequest(card)),
                events.UpdateAdministrator.Select(user => new RoomState.UpdateAdministrator(user)),
                events.UpdateGameStatus.Select(status => new RoomState.UpdateGameStatus(status)),
                events.UpdateHandStatus.Select(data => new RoomState.UpdateHandStatus(data.User, data.Status)),
                events.UpdatePick.Select(pick => new RoomState.UpdatePick(pick)),
                events.UpdateTurn.Select(turn => new RoomState.UpdateTurn(turn))
            );

            return actions.Subscribe(room.Mutate);
        }
    }
}