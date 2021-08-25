using Xunit;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;
using static Karata.Cards.Card;

namespace Karata.Cards.Tests
{
    public class CardTest
    {
        [Theory]
        [InlineData(Ace, Spades, "Ace of Spades", 1, Black)]
        [InlineData(None, RedJoker, "Red Joker", 0, Red)]
        public void CardPropertyTest(CardFace face, CardSuit suit, string name, uint rank, CardColor color)
        {
            var card = new Card(face, suit);
            Assert.Equal(name, card.Name);
            Assert.Equal(rank, card.Rank);
            Assert.Equal(color, card.Color);
        }
    }
}
