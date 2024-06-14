using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Karata.Cards.Tests;

public class DeckTest
{
    private const uint StandardDeckSize = 54;

    [Fact]
    public void DeckPropertyTest()
    {
        var deck = new Deck();
        Assert.Empty(deck);
    }

    [Fact]
    public void StandardDeckTest()
    {
        var deck = Deck.Standard;

        Assert.NotEmpty(deck);
        Assert.Equal(StandardDeckSize, (uint)deck.Count);
        Assert.Equal(deck.Distinct().Count(), deck.Count);
    }

    [Fact]
    public void ShuffleTest()
    {
        var deck = Deck.Standard;

        deck.Shuffle();
        Assert.NotEmpty(deck);
        Assert.Equal(StandardDeckSize, (uint)deck.Count);
        Assert.Equal(deck.Distinct().Count(), deck.Count);
    }

    [Fact]
    public void DealTest()
    {
        var emptyDeck = new Deck();
        var standardDeck = Deck.Standard;

        _ = standardDeck.Deal();

        Assert.Throws<InvalidOperationException>(emptyDeck.Deal);
        Assert.Equal(StandardDeckSize - 1, (uint) standardDeck.Count);
        Assert.Equal(standardDeck.Distinct().Count(), standardDeck.Count);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(10, false)]
    [InlineData(55, true)]
    public void DealManyTest(uint num, bool throws)
    {
        var deck = Deck.Standard;
        List<Card> dealt;

        if (throws)
        {
            Assert.Throws<InvalidOperationException>(() => _ = deck.DealMany(num));
            return;
        }
        else dealt = deck.DealMany(num);

        Assert.Equal(StandardDeckSize - num, (uint)deck.Count);
        Assert.Equal(deck.Distinct().Count(), deck.Count);
        Assert.Equal(num, (uint) dealt.Count);
        Assert.Equal(dealt.Distinct().Count(), dealt.Count);
    }

    [Fact]
    public void TryDealTest()
    {
        var emptyDeck = new Deck();
        var standardDeck = Deck.Standard;

        Assert.Empty(emptyDeck);

        Assert.False(emptyDeck.TryDeal(out var card));
        Assert.Null(card);
        Assert.Empty(emptyDeck);

        Assert.True(standardDeck.TryDeal(out card));
        Assert.NotNull(card);

        Assert.Equal(StandardDeckSize - 1, (uint)standardDeck.Count);
        Assert.Equal(standardDeck.Distinct().Count(), standardDeck.Count);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(10, true)]
    [InlineData(55, false)]
    public void TryDealManyTest(uint num, bool shouldSucceed)
    {
        var deck = Deck.Standard;
        var success = deck.TryDealMany(num, out var dealt);

        Assert.Equal(shouldSucceed, success);
        Assert.NotNull(dealt);

        if (shouldSucceed)
        {
            Assert.Equal(StandardDeckSize - num, (uint)deck.Count);
            Assert.Equal(deck.Distinct().Count(), deck.Count);
            

            Assert.Equal(num, (uint)dealt.Count);
            Assert.Equal(dealt.Distinct().Count(), dealt.Count);
        }
        else 
        {
            Assert.Equal(StandardDeckSize, (uint)deck.Count);
            Assert.Equal(deck.Distinct().Count(), deck.Count);
        }   
    }
}