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
        [InlineData(BlackJoker, Black)]
        [InlineData(RedJoker, Red)]
        public void JokerOfColorTest(CardSuit suit, CardColor color)
        {
            var expectedJoker = new Card(Joker, suit);
            var actualJoker = JokerOfColor(color);

            Assert.Equal(expectedJoker, actualJoker);
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
        [InlineData(Joker, BlackJoker, 0, false)]
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
        [InlineData(Joker, RedJoker, Red, false)]
        [InlineData(Ace, Spades, Black, false)]
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
        [InlineData(Joker, BlackJoker, true)]
        [InlineData(Ace, Spades, false)]
        [InlineData(Three, Spades, false)]
        public void IsJokerTest(CardFace face, CardSuit suit, bool isJoker)
        {
            var card = new Card(face, suit);
            Assert.Equal(isJoker, card.IsJoker());
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
