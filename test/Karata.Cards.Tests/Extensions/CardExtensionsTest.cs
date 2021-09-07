using System;
using Karata.Cards.Extensions;
using Xunit;
using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;
using static Karata.Cards.Extensions.CardExtensions;

namespace Karata.Cards.Tests
{
    public class CardExtensionsTest
    {
        [Theory]
        [InlineData(Black, BlackJoker, false)]
        [InlineData(Red, RedJoker, false)]
        [InlineData((CardColor)3, default(CardSuit), true)]
        public void JokerOfColorTest(CardColor color, CardSuit suit, bool throws)
        {
            var expectedJoker = new Card(Joker, suit);

            if (throws)
                Assert.Throws<ArgumentException>(() => _ = JokerOfColor(color));
            else
            {
                var actualJoker = JokerOfColor(color);
                Assert.Equal(expectedJoker, actualJoker);
            }
        }

        [Fact]
        public void OfTest()
        {
            var card1 = new Card(Ace, Spades);
            var card2 = Ace.Of(Spades);

            Assert.Equal(card1, card2);
        }

        [Theory]
        [InlineData(Ace, Spades, "Ace of Spades")]
        [InlineData(Joker, BlackJoker, "Black Joker")]
        public void GetNameTest(CardFace face, CardSuit suit, string name)
        {
            var card = new Card(face, suit);
            Assert.Equal(name, card.GetName());
        }

        [Theory]
        [InlineData(Ace, Spades, 1, false)]
        [InlineData((CardFace)14, Spades, default(uint), true)]
        public void GetRankTest(CardFace face, CardSuit suit, uint rank, bool throws)
        {
            var card = new Card(face, suit);

            if (throws)
                Assert.Throws<ArgumentException>(() => _ = card.GetRank());
            else
                Assert.Equal(rank, card.GetRank());
        }

        [Theory]
        [InlineData(Joker, BlackJoker, Black, false)]
        [InlineData(Joker, RedJoker, Red, false)]
        [InlineData(Ace, Spades, Black, false)]
        [InlineData(Ace, Hearts, Red, false)]
        [InlineData(Ace, Clubs, Black, false)]
        [InlineData(Ace, Diamonds, Red, false)]
        [InlineData(Ace, (CardSuit)7, default(CardColor), true)]
        public void GetColorTest(CardFace face, CardSuit suit, CardColor color, bool throws)
        {
            var card = new Card(face, suit);

            if (throws)
                Assert.Throws<ArgumentException>(() => _ = card.GetColor());
            else
                Assert.Equal(color, card.GetColor());
        }

        [Theory]
        [InlineData(Joker, BlackJoker, true)]
        [InlineData(Ace, Spades, false)]
        [InlineData(Three, Spades, true)]
        public void IsBombTest(CardFace face, CardSuit suit, bool isBomb)
        {
            var card = new Card(face, suit);
            Assert.Equal(isBomb, card.IsBomb());
        }

        [Theory]
        [InlineData(Ace, Spades, false)]
        [InlineData(Eight, Spades, true)]
        [InlineData(Queen, Spades, true)]
        public void IsQuestionTest(CardFace face, CardSuit suit, bool isQuestion)
        {
            var card = new Card(face, suit);
            Assert.Equal(isQuestion, card.IsQuestion());
        }
    }
}
