using System;
using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards.Extensions
{
    public static class CardExtensions
    {
        public static Card Of(this CardFace face, CardSuit suit) => new(face, suit);

        public static Card JokerOfColor(CardColor color) => Joker.Of(color switch
        {
            Black => BlackJoker,
            Red => RedJoker,
            _ => throw new ArgumentException("Invalid color", nameof(color))
        });

        public static string GetName(this Card card) => card.Suit switch
        {
            BlackJoker or RedJoker => $"{card.Suit.ToString()[0..^5]} Joker",
            _ => $"{card.Face} of {card.Suit}"
        };

        public static uint GetRank(this Card card) => card.Face switch
        {
            Joker => 0,
            Ace => 1,
            Two => 2,
            Three => 3,
            Four => 4,
            Five => 5,
            Six => 6,
            Seven => 7,
            Eight => 8,
            Nine => 9,
            Ten => 10,
            Jack => 11,
            Queen => 12,
            King => 13,
            _ => throw new ArgumentException("Invalid face", nameof(card))
        };

        public static CardColor GetColor(this Card card)
            => (card.Suit is Spades or Clubs or BlackJoker) ? Black : Red;

        public static bool IsBomb(this Card card)
            => card.Face is Two or Three || card.IsJoker();

        public static bool IsJoker(this Card card)
            => card.Suit is BlackJoker or RedJoker && card.Face is Joker;

        public static bool IsQuestion(this Card card)
            => card.Face is Queen or Eight;
    }
}