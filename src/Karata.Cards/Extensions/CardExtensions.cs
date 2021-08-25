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

        public static Card Joker(CardColor color) => None.Of(color switch
        {
            Black => BlackJoker,
            Red => RedJoker,
            _ => throw new ArgumentException("Invalid color", nameof(color))
        });
    }
}