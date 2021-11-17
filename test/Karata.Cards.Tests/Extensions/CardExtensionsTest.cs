using System;
using Karata.Cards.Extensions;
using Xunit;
using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards.Tests
{
    public class CardExtensionsTest
    {
        [Fact]
        public void OfTest()
        {
            var card1 = new Card(Ace, Spades);
            var card2 = Ace.Of(Spades);

            Assert.Equal(card1, card2);
        }

        [Theory]
        [InlineData(Black, BlackJoker, false)]
        [InlineData(Red, RedJoker, false)]
        [InlineData((CardColor)3, default(CardSuit), true)]
        public void ColoredJokerTest(CardColor color, CardSuit suit, bool throws)
        {
            var expectedJoker = new Card(Joker, suit);

            if (throws)
                Assert.Throws<ArgumentException>(() => _ = color.ColoredJoker());
            else
            {
                var actualJoker = color.ColoredJoker();
                Assert.Equal(expectedJoker, actualJoker);
            }
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
        [InlineData((CardFace)15, Spades, default(uint), true)]
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
        [InlineData(Nine, Spades, 0)]
        [InlineData(Ace, Diamonds, 1)]
        [InlineData(Ace, Spades, 2)]
        public void GetAceValueTest(CardFace face, CardSuit suit, uint value)
        {
            var card = new Card(face, suit);
            var actual = card.GetAceValue();
            Assert.Equal(value, actual);
        }

        [Theory]
        [InlineData(Ace, Spades, 0)]
        [InlineData(Two, Spades, 2)]
        [InlineData(Three, Spades, 3)]
        [InlineData(Joker, BlackJoker, 5)]
        public void GetPickValueTest(CardFace face, CardSuit suit, uint value)
        {
            var card = new Card(face, suit);
            var actual = card.GetPickValue();
            Assert.Equal(value, actual);
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
            var actual = card.IsQuestion();
            Assert.Equal(isQuestion, actual);
        }

        [Theory]
        [InlineData(Ace, Nine, false)]
        [InlineData(Nine, Nine, true)]
        public void FaceEqualsTest(CardFace face1, CardFace face2, bool faceEquals)
        {
            var card1 = new Card(face1, Spades);
            var card2 = new Card(face2, Spades);
            var actual = card1.FaceEquals(card2);
            Assert.Equal(faceEquals, actual);
        }

        [Theory]
        [InlineData(Spades, Spades, true)]
        [InlineData(Hearts, Spades, false)]
        public void SuitEqualsTest(CardSuit suit1, CardSuit suit2, bool suitEquals)
        {
            var card1 = new Card(Ace, suit1);
            var card2 = new Card(Ace, suit2);
            var actual = card1.SuitEquals(card2);
            Assert.Equal(suitEquals, actual);
        }
    }
}
