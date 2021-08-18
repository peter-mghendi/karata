using System;
using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards
{
    public partial record Card(CardSuit Suit, CardFace Face)
    {
        public string Name => Suit switch
        {
            BlackJoker or RedJoker => $"{Suit.ToString()[0..^5]} Joker",
            _ => $"{Face} of {Suit}"
        };

        public int Rank => Face switch
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

        public CardColor Color => (Suit is Spades or Clubs or BlackJoker) ? Black : Red;

        public bool IsBomb => Face is Two or Three || IsJoker;

        public bool IsJoker => Suit is BlackJoker or RedJoker && Face is None;

        public bool IsQuestion => Face is Queen or Eight;
    }
}