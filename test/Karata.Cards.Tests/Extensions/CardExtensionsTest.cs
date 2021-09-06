using Karata.Cards.Extensions;
using Xunit;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;
using static Karata.Cards.Extensions.CardExtensions;

namespace Karata.Cards.Tests
{
    public class CardExtensionsTest
    {
        [Fact]
        public void JokerOfColorTest()
        {
            var blackJoker1 = new Card(Joker, BlackJoker);
            var blackJoker2 = JokerOfColor(Black);

            var redJoker1 = new Card(Joker, RedJoker);
            var redJoker2 = JokerOfColor(Red);

            Assert.Equal(blackJoker1, blackJoker2);
            Assert.Equal(redJoker1, redJoker2);
        }

        [Fact]
        public void OfTest()
        {
            var card1 = new Card(Ace, Spades);
            var card2 = Ace.Of(Spades);

            Assert.Equal(card1, card2);
        }
    }
}
