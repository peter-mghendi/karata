using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards.Extensions
{
    public static class CardExtensions
    {
        public static Card Of(this CardFace face, CardSuit suit) => new() 
        { 
            Face = face,
            Suit = suit
        };

        public static Card ColoredJoker(this CardColor color)
        {
            if (!Enum.IsDefined(color)) 
                throw new ArgumentException("Invalid color", nameof(color));
            return Joker.Of(color is Black ? BlackJoker : RedJoker);
        }

        public static string GetName(this Card card) => card.Face is Joker 
            ? $"{card.Suit.ToString()[0..^5]} Joker"
            : $"{card.Face} of {card.Suit}";

        public static uint GetRank(this Card card) => !Enum.IsDefined(card.Face) 
            ? throw new ArgumentException("Invalid face", nameof(card)) 
            : (uint)card.Face;

        public static CardColor GetColor(this Card card)
        {
            if (card.Suit is Spades or Clubs or BlackJoker) return Black;
            if (card.Suit is Hearts or Diamonds or RedJoker) return Red;
            throw new ArgumentException("Invalid suit", nameof(card));
        }

        public static uint GetAceValue(this Card card) {
            if (card is not { Face: Ace }) return 0;
            if (card is not { Suit: Spades }) return 1;
            return 2;
        }

        public static uint GetPickValue(this Card card) {
            if (card is { Face: Joker }) return 5;
            if (card is { Face: Two or Three }) return card.GetRank();
            return 0;
        }

        public static bool IsBomb(this Card card)
            => card.Face is Two or Three or Joker;

        public static bool IsQuestion(this Card card)
            => card.Face is Queen or Eight;

        public static bool FaceEquals(this Card thisCard, Card otherCard)
            => thisCard.Face == otherCard.Face;

        public static bool SuitEquals(this Card thisCard, Card otherCard)
            => thisCard.Suit == otherCard.Suit;
    }
}