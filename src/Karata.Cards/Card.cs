using System;
using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards
{
    public partial record Card(CardSuit Suit, CardFace Face)
    {
        public string Name
        {
            get
            {
                var suit = Suit.ToString();
                return Suit switch
                {
                    BlackJoker or RedJoker => $"{suit[0..^5]} Joker",
                    _ => $"{Face} of {suit}"
                };
            }
        }

        public int Rank
        {
            get => Face switch
            {
                None => 0,
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
                _ => throw new Exception("Invalid card face value")
            };
        }

        public CardColor Color
        {
            get => (Suit is (Spades or Clubs or BlackJoker)) ? Black : Red;
        }
    }
}