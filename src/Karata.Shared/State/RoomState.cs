using System.Collections.Immutable;
using Karata.Pebble;
using Karata.Pebble.Interceptors;
using Karata.Pebble.StateActions;
using Karata.Shared.Models;

namespace Karata.Shared.State;

public class RoomState(RoomData data, ImmutableArray<Interceptor<RoomData>> interceptors)
    : Store<RoomData>(data, interceptors)
{
    public record AddHandToRoom(long Id, UserData User, HandStatus Status) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hand = new HandData { Id = Id, Player = User, Cards = [], Status = Status };
            return state with { Game = state.Game with { Hands = [..state.Game.Hands, hand] } };
        }
    }

    public record MoveCardCountFromDeckToHand(UserData User, int Count) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = state.Game.Hands.Select(h =>
                h.Player == User ? h with { Cards = [..h.Cards, ..Enumerable.Repeat(new Card(), Count)] } : h);
            return state with
            {
                Game = state.Game with { Hands = [..hands], Deck = new Deck(state.Game.Deck.SkipLast(Count)) }
            };
        }
    }

    public record MoveCardsFromDeckToHand(UserData User, List<Card> Cards) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = state.Game.Hands
                .Select(h => h.Player == User ? h with { Cards = [..h.Cards, ..Cards] } : h);

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

    public record MoveCardsFromHandToPile(UserData User, List<Card> Cards, bool Mine) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = Mine switch
            {
                true => state.Game.Hands.Select(h => h.Player == User ? h with { Cards = [..h.Cards.Except(Cards)] } : h),
                false => state.Game.Hands.Select(h => h.Player == User ? h with { Cards = h.Cards[Cards.Count..] } : h)
            };

            var pile = state.Game.Pile;
            foreach (var card in Cards) pile.Push(card);

            return state with { Game = state.Game with { Hands = hands.ToList(), Pile = pile } };
        }
    }

    public record ReceiveChat(ChatData Chat) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Chats = [..state.Chats, Chat] };
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

    public record RemoveHandFromRoom(UserData User) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            return state with { Game = state.Game with { Hands = [..state.Game.Hands.Where(h => h.Player != User)] } };
        }
    }

    public record SetCurrentRequest(Card Card) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Game = state.Game with { Request = Card } };
    }

    public record UpdateAdministrator(UserData Administrator) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Administrator = Administrator };
    }

    public record UpdateGameStatus(GameStatus Status) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Game = state.Game with { Status = Status } };
    }

    public record UpdateHandStatus(UserData User, HandStatus Status) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            return state with
            {
                Game = state.Game with
                {
                    Hands = [..state.Game.Hands.Select(h => h.Player == User ? h with { Status = Status } : h)]
                }
            };
        }
    }

    public record UpdatePick(uint Pick) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with
        {
            Game = state.Game with { Pick = Pick }
        };
    }

    public record UpdateTurn(int Turn) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with
        {
            Game = state.Game with { CurrentTurn = Turn }
        };
    }
}