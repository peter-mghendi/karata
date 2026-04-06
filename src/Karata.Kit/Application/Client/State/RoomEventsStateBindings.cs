using System.Reactive.Linq;
using Karata.Kit.Application.Store;
using Karata.Kit.Domain.Models;
using Karata.Pebble.StateActions;

namespace Karata.Kit.Application.Client.State;

public static class RoomEventsStateBindings  
{
    extension(RoomEvents events)
    {
        public IDisposable BindRoomState(RoomStore room)
        {
            var actions = Observable.Merge<StateAction<RoomData>>(
                events.AddHandToRoom.Select(data => new RoomStore.AddHandToRoom(data.Id, data.User, data.Status)),
                events.TurnCommitted.Select(resolution => new RoomStore.TurnCommitted(resolution)),
                events.MoveCardsFromDeckToHand.Select(data => new RoomStore.MoveCardsFromDeckToHand(data.HandId, [..data.Cards])),
                events.MoveCardsFromDeckToPile.Select(cards => new RoomStore.MoveCardsFromDeckToPile([..cards])),
                events.MoveCardsFromHandToPile.Select(data => new RoomStore.MoveCardsFromHandToPile(data.HandId, [..data.Cards], data.Visible)),
                events.Chat.Select(chat => new RoomStore.Chat(chat)),
                events.ReclaimPile.Select(_ => new RoomStore.ReclaimPile()),
                events.RemoveHandFromRoom.Select(user => new RoomStore.RemoveHandFromRoom(user)),
                events.UpdateAdministrator.Select(administrator => new RoomStore.UpdateAdministrator(administrator)),
                events.UpdateGameStatus.Select(status => new RoomStore.UpdateGameStatus(status)),
                events.UpdateHandStatus.Select(data => new RoomStore.UpdateHandStatus(data.HandId, data.Status))
            );

            return actions.Subscribe(room.Mutate);
        }
    }
}