using Karata.Cards.Extensions;
using Xunit;
using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards.Tests.Extensions;

public class CardExtensionsTest
{
    [Fact]
    public void OfTest()
    {
        var card1 = new Card { Face = Ace, Suit = Spades };
        var card2 = Ace.Of(Spades);

        Assert.Equal(card1, card2);
    }

    [Theory]
    [InlineData(Black, BlackJoker, false)]
    [InlineData(Red, RedJoker, false)]
    [InlineData((CardColor)3, default(CardSuit), true)]
    public void ColoredJokerTest(CardColor color, CardSuit suit, bool throws)
    {
        var expectedJoker = new Card { Face = Joker, Suit = suit };

        if (throws)
            Assert.Throws<ArgumentException>(() => _ = color.Joker);
        else
        {
            var actualJoker = color.Joker;
            Assert.Equal(expectedJoker, actualJoker);
        }
    }

    [Theory]
    [InlineData(Ace, Spades, "Ace of Spades")]
    [InlineData(Joker, BlackJoker, "Black Joker")]
    public void GetNameTest(CardFace face, CardSuit suit, string name)
    {
        var card = new Card { Face = face, Suit = suit };
        Assert.Equal(name, card.Name);
    }

    [Theory]
    [InlineData(Ace, Spades, 1, false)]
    [InlineData((CardFace)15, Spades, 0, true)]
    public void GetRankTest(CardFace face, CardSuit suit, uint rank, bool throws)
    {
        var card = new Card { Face = face, Suit = suit };

        if (throws)
        {
            Assert.Throws<ArgumentException>(() => _ = card.Rank);
        }
        else
        {
            Assert.Equal(rank, card.Rank);
        }
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
        var card = new Card { Face = face, Suit = suit };

        if (throws)
            Assert.Throws<ArgumentException>(() => _ = card.Color);
        else
            Assert.Equal(color, card.Color);
    }

    [Theory]
    [InlineData(Nine, Spades, 0)]
    [InlineData(Ace, Diamonds, 1)]
    [InlineData(Ace, Spades, 2)]
    public void GetAceValueTest(CardFace face, CardSuit suit, uint value)
    {
        var card = new Card { Face = face, Suit = suit };
        var actual = card.AceValue;
        Assert.Equal(value, actual);
    }

    [Theory]
    [InlineData(Ace, Spades, 0)]
    [InlineData(Two, Spades, 2)]
    [InlineData(Three, Spades, 3)]
    [InlineData(Joker, BlackJoker, 5)]
    public void GetPickValueTest(CardFace face, CardSuit suit, uint value)
    {
        var card = new Card { Face = face, Suit = suit };
        var actual = card.PickValue;
        Assert.Equal(value, actual);
    }

    [Theory]
    [InlineData(Joker, BlackJoker, true)]
    [InlineData(Ace, Spades, false)]
    [InlineData(Three, Spades, true)]
    public void IsBombTest(CardFace face, CardSuit suit, bool isBomb)
    {
        var card = new Card { Face = face, Suit = suit };
        Assert.Equal(isBomb, card.IsBomb);
    }

    [Theory]
    [InlineData(Ace, Spades, false)]
    [InlineData(Eight, Spades, true)]
    [InlineData(Queen, Spades, true)]
    public void IsQuestionTest(CardFace face, CardSuit suit, bool isQuestion)
    {
        var card = new Card { Face = face, Suit = suit };
        var actual = card.IsQuestion;
        Assert.Equal(isQuestion, actual);
    }
    
    [Theory]
    [InlineData(Two, Hearts, true)]    // Bomb
    [InlineData(Eight, Diamonds, true)] // Question
    [InlineData(Ace, Spades, true)]    // Ace
    [InlineData(Jack, Hearts, true)]   // Jack
    [InlineData(King, Clubs, true)]    // King
    [InlineData(Four, Diamonds, false)] // Neither Bomb, Question, Ace, Jack, King
    public void IsSpecialTest(CardFace face, CardSuit suit, bool isSpecial)
    {
        var card = new Card { Face = face, Suit = suit };
        Assert.Equal(isSpecial, card.IsSpecial);
    }

    [Theory]
    [InlineData(Ace, Nine, false)]
    [InlineData(Nine, Nine, true)]
    public void FaceEqualsTest(CardFace face1, CardFace face2, bool faceEquals)
    {
        var card1 = new Card { Face = face1, Suit = Spades };
        var card2 = new Card { Face = face2, Suit = Spades };
        var actual = card1.FaceEquals(card2);
        Assert.Equal(faceEquals, actual);
    }

    [Theory]
    [InlineData(Spades, Spades, true)]
    [InlineData(Hearts, Spades, false)]
    public void SuitEqualsTest(CardSuit suit1, CardSuit suit2, bool suitEquals)
    {
        var card1 = new Card { Face = Ace, Suit = suit1 };
        var card2 = new Card { Face = Ace, Suit = suit2 };
        var actual = card1.SuitEquals(card2);
        Assert.Equal(suitEquals, actual);
    }
}