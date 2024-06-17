using Karata.Cards;
using Karata.Pebble;
using Karata.Pebble.Interceptors;
using Karata.Shared.Models;

namespace Karata.Client.Infrastructure.State;

public class RoomState : Store<RoomData>
{
    public readonly HandData MyHand;

    public RoomState(RoomData data, HandData hand) : base(data)
    {
        MyHand = hand;
        AddInterceptor(new LoggingInterceptor<RoomData>());
    }

    public record AddCardsToPlayerHand(HandData Hand, int Count) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            var hand = Hand with { Cards = [..Hand.Cards, ..Enumerable.Repeat(new Card(), Count)] };
            var hands = state.Game.Hands.Select(h => h == Hand ? hand : h);
            return state with { Game = state.Game with { Hands = hands.ToList() } };
        }
    }

    public record AddCardRangeToHand(HandData Hand, List<Card> Cards) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            var hands = state.Game.Hands.Select(h => h == Hand ? Hand with { Cards = [..Hand.Cards, ..Cards] } : h);
            return state with { Game = state.Game with { Hands = hands.ToList() } };
        }
    }

    public record AddCardRangeToPile(List<Card> Cards) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            var pile = state.Game.Pile;
            foreach (var card in Cards) pile.Push(card);

            return state with { Game = state.Game with { Pile = pile } };
        }
    }

    public record AddHandToRoom(HandData Hand) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            return state with { Game = state.Game with { Hands = [..state.Game.Hands, Hand] } };
        }
    }

    public record ReceiveChat(ChatData Chat) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state) => state with { Chats = state.Chats.Append(Chat).ToList() };
    }

    public record ReclaimPile : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            var pile = state.Game.Pile;
            var cards = pile.Reclaim();


            return state with
            {
                Game = state.Game with { Pile = pile, DeckCount = state.Game.DeckCount + cards.Count }
            };
        }
    }

    public record RemoveCardsFromDeck(int Count) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            return state with { Game = state.Game with { DeckCount = state.Game.DeckCount - Count } };
        }
    }

    public record RemoveCardsFromPlayerHand(HandData Hand, int Count) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            var hand = Hand with { Cards = Hand.Cards[Count..] };
            var hands = state.Game.Hands.Select(h => h == Hand ? hand : h);
            return state with { Game = state.Game with { Hands = hands.ToList() } };
        }
    }

    public record RemoveCardRangeFromHand(HandData Hand, List<Card> Cards) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            var cards = Hand.Cards.Except(Cards).ToList();
            var hands = state.Game.Hands.Select(h => h == Hand ? Hand with { Cards = cards } : h);
            return state with { Game = state.Game with { Hands = hands.ToList() } };
        }
    }

    public record RemoveHandFromRoom(HandData Hand) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state)
        {
            return state with { Game = state.Game with { Hands = state.Game.Hands.Except([Hand]).ToList() } };
        }
    }

    public record SetCurrentRequest(Card Card) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state) =>
            state with { Game = state.Game with { CurrentRequest = Card } };
    }

    public record UpdateTurn(int Turn) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state) => state with
        {
            Game = state.Game with { CurrentTurn = Turn }
        };
    }

    public record UpdateGameStatus(GameStatus Status) : StateAction<RoomData>
    {
        public override RoomData Execute(RoomData state) => state with { Game = state.Game with { Status = Status } };
    }
}