using System.Collections.Immutable;
using Karata.Cards;
using Karata.Kit.Domain.Models;
using Karata.Pebble;
using Karata.Pebble.Interceptors;
using Karata.Pebble.StateActions;

namespace Karata.Kit.Application.Store;

public class RoomStore(RoomData data, ImmutableArray<Interceptor<RoomData>> interceptors)
    : Store<RoomData>(data, interceptors)
{
    public record AddHandToRoom(long Id, UserData User, HandStatus Status) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hand = new HandData { Id = Id, Player = User, Cards = [], Status = Status, IsLastCard = false };
            return state with { Game = state.Game with { Hands = [..state.Game.Hands, hand] } };
        }
    }

    public record TurnCommitted(GameData Game) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Game = Game };
    }

    public record MoveCardsFromDeckToHand(long HandId, List<Card> Cards) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = state.Game.Hands
                .Select(h => h.Id == HandId ? h with { Cards = [..h.Cards, ..Cards] } : h);

            return state with
            {
                Game = state.Game with { Hands = [..hands], Deck = new Deck(state.Game.Deck.SkipLast(Cards.Count)) }
            };
        }
    }

    public record MoveCardsFromDeckToPile(List<Card> Cards) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var pile = state.Game.Pile;
            foreach (var card in Cards) pile.Push(card);

            return state with
            {
                Game = state.Game with { Deck = new Deck(state.Game.Deck.SkipLast(Cards.Count)), Pile = pile }
            };
        }
    }

    public record MoveCardsFromHandToPile(long HandId, List<Card> Cards, bool Visible) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = Visible switch
            {
                true => state.Game.Hands.Select(h => h.Id == HandId ? h with { Cards = [..h.Cards.Except(Cards)] } : h),
                false => state.Game.Hands.Select(h => h.Id == HandId ? h with { Cards = h.Cards[Cards.Count..] } : h)
            };

            var pile = state.Game.Pile;
            foreach (var card in Cards) pile.Push(card);

            return state with { Game = state.Game with { Hands = hands.ToList(), Pile = pile } };
        }
    }

    public record Chat(ChatData ChatData) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Chats = [..state.Chats, ChatData] };
    }

    public record ReclaimPile : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var deck = state.Game.Deck;
            var pile = state.Game.Pile;
            var cards = pile.Reclaim();

            foreach (var card in cards) state.Game.Deck.Push(card);

            return state with
            {
                Game = state.Game with { Pile = pile, Deck = deck }
            };
        }
    }

    public record RemoveHandFromRoom(long HandId) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            return state with { Game = state.Game with { Hands = [..state.Game.Hands.Where(h => h.Id != HandId)] } };
        }
    }

    public record UpdateAdministrator(UserData Administrator) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Administrator = Administrator };
    }

    public record UpdateGameStatus(GameData Game) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Game = Game };
    }

    public record UpdateHandStatus(long HandId, HandStatus Status) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            return state with
            {
                Game = state.Game with
                {
                    Hands = [..state.Game.Hands.Select(h => h.Id == HandId ? h with { Status = Status } : h)]
                }
            };
        }
    }
}