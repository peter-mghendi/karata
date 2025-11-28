using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards.Extensions
{
    public static class CardExtensions
    {
        extension(CardFace face)
        {
            public Card Of(CardSuit suit) => new() { Face = face, Suit = suit };
        }

        extension(CardColor color)
        {
            public Card Joker => !Enum.IsDefined(color)
                ? throw new ArgumentException("Invalid color", nameof(color))
                : Joker.Of(color is Black ? BlackJoker : RedJoker);
        }

        extension(Card card)
        {
            public string Name => card.Face is Joker
                ? $"{card.Suit.ToString()[0..^5]} Joker"
                : $"{card.Face} of {card.Suit}";

            public uint Rank =>
                !Enum.IsDefined(card.Face)
                    ? throw new ArgumentException("Invalid face", nameof(card))
                    : (uint)card.Face;

            public CardColor Color =>
                card.Suit switch
                {
                    Spades or Clubs or BlackJoker => Black,
                    Hearts or Diamonds or RedJoker => Red,
                    _ => throw new ArgumentException("Invalid suit", nameof(card))
                };

            public uint AceValue =>
                card switch
                {
                    { Face: Ace, Suit: Spades } => 2,
                    { Face: Ace } => 1,
                    _ => 0,
                };

            public uint PickValue =>
                card switch
                {
                    { Face: Two or Three } => card.Rank,
                    { Face: Joker } => 5,
                    _ => 0
                };

            public bool IsBomb => card.Face is Two or Three or Joker;

            public bool IsQuestion => card.Face is Queen or Eight;

            public bool IsSpecial => card.IsBomb || card.IsQuestion || card.Face is Ace or Jack or King;

            public bool FaceEquals(Card otherCard) => card.Face == otherCard.Face;

            public bool SuitEquals(Card otherCard) => card.Suit == otherCard.Suit;
        }
    }
}