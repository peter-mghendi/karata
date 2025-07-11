using Karata.Cards;
using Karata.Pebble;
using Karata.Pebble.StateActions;
using Karata.Shared.Models;

namespace Karata.Client.Infrastructure.State;

public class RoomState(RoomData data, string username, ILoggerFactory? logging = null) : Store<RoomData>(data, logging)
{
    public HandData MyHand => State.Game.Hands.Single(h => h.User.Email == username);

    public record AddHandToRoom(UserData User, HandStatus Status) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hand = new HandData { User = User, Cards = [], Status = Status };
            return state with { Game = state.Game with { Hands = [..state.Game.Hands, hand] } };
        }
    }

    public record MoveCardCountFromDeckToHand(UserData User, int Count) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = state.Game.Hands.
                Select(h => h.User == User ? h with { Cards = [..h.Cards, ..Enumerable.Repeat(new Card(), Count)] } : h);
            return state with
            {
                Game = state.Game with { Hands = hands.ToList(), DeckCount = state.Game.DeckCount - Count }
            };
        }
    }
    
    public record MoveCardsFromDeckToHand(UserData User, List<Card> Cards) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = state.Game.Hands
                .Select(h => h.User == User ? h with { Cards = [..h.Cards, ..Cards] } : h);

            return state with
            {
                Game = state.Game with { Hands = hands.ToList(), DeckCount = state.Game.DeckCount - Cards.Count }
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
                Game = state.Game with { DeckCount = state.Game.DeckCount - Cards.Count, Pile = pile }
            };
        }
    }

    public record MoveCardsFromHandToPile(UserData User, List<Card> Cards, bool Mine) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var hands = Mine switch
            {
                true => state.Game.Hands.Select(h => h.User == User ? h with { Cards = h.Cards.Except(Cards).ToList() } : h),
                false => state.Game.Hands.Select(h => h.User == User ? h with { Cards = h.Cards[Cards.Count..] } : h)
            };
            
            var pile = state.Game.Pile;
            foreach (var card in Cards) pile.Push(card);

            return state with { Game = state.Game with { Hands = hands.ToList(), Pile = pile } };
        }
    }

    public record ReceiveChat(ChatData Chat) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) => state with { Chats = state.Chats.Append(Chat).ToList() };
    }

    public record ReclaimPile : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            var pile = state.Game.Pile;
            var cards = pile.Reclaim();
            
            return state with
            {
                Game = state.Game with { Pile = pile, DeckCount = state.Game.DeckCount + cards.Count }
            };
        }
    }

    public record RemoveHandFromRoom(UserData User) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state)
        {
            return state with { Game = state.Game with { Hands = state.Game.Hands.Where(h => h.User != User).ToList() } };
        }
    }

    public record SetCurrentRequest(Card Card) : StateAction<RoomData>
    {
        public override RoomData Apply(RoomData state) =>
            state with { Game = state.Game with { Request = Card } };
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
                Game = state.Game with { 
                    Hands = [..state.Game.Hands.Select(h => h.User == User ? h with { Status = Status } : h)]
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