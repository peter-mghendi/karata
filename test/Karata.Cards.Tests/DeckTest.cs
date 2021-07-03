using System;
using System.Collections.Generic;
using System.Linq;
using Karata.Cards.Shufflers;
using Xunit;
using static Karata.Cards.Shufflers.ShuffleAlgorithm;

namespace Karata.Cards.Tests
{
    public class DeckTest
    {
        private const int StandardDeckSize = 54;

        [Fact]
        public void DeckPropertyTest()
        {
            var deck = new Deck();
            Assert.Empty(deck.Cards);
        }

        [Fact]
        public void StandardDeckTest()
        {
            var deck = Deck.StandardDeck;

            Assert.NotEmpty(deck.Cards);
            Assert.Equal(StandardDeckSize, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);
        }

        [Theory]
        [InlineData(FisherYates)]
        [InlineData(OrderByRandom)]
        public void ShuffleAlgorithmTest(ShuffleAlgorithm shuffleAlgorithm)
        {
            var deck = Deck.StandardDeck;

            deck.Shuffle(shuffleAlgorithm);
            Assert.NotEmpty(deck.Cards);
            Assert.Equal(StandardDeckSize, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);
        }

        [Fact]
        public void DealTest()
        {
            var emptyDeck = new Deck();
            var standardDeck = Deck.StandardDeck;

            _ = standardDeck.Deal();

            Assert.Throws<InvalidOperationException>(emptyDeck.Deal);
            Assert.Equal(StandardDeckSize - 1, standardDeck.Cards.Count);
            Assert.Equal(standardDeck.Cards.Distinct().Count(), standardDeck.Cards.Count);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(10, false)]
        [InlineData(55, true)]
        public void DealManyTest(int num, bool throws)
        {
            var deck = Deck.StandardDeck;
            List<Card> dealt;

            if (throws)
            {
                Assert.Throws<InvalidOperationException>(() => _ = deck.DealMany(num));
                return;
            }
            else dealt = deck.DealMany(num);

            Assert.Equal(StandardDeckSize - num, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);
            Assert.Equal(num, dealt.Count);
            Assert.Equal(dealt.Distinct().Count(), dealt.Count);
        }

        [Fact]
        public void TryDealTest()
        {
            var emptyDeck = new Deck();
            var standardDeck = Deck.StandardDeck;

            Assert.Empty(emptyDeck.Cards);

            Assert.False(emptyDeck.TryDeal(out var card));
            Assert.Null(card);
            Assert.Empty(emptyDeck.Cards);

            Assert.True(standardDeck.TryDeal(out card));
            Assert.NotNull(card);

            Assert.Equal(StandardDeckSize - 1, standardDeck.Cards.Count);
            Assert.Equal(standardDeck.Cards.Distinct().Count(), standardDeck.Cards.Count);

        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(10, true)]
        [InlineData(55, false)]
        public void TryDealManyTest(int num, bool shouldSucceed)
        {
            var deck = Deck.StandardDeck;
            var success = deck.TryDealMany(num, out var dealt);

            Assert.Equal(shouldSucceed, success);

            if (shouldSucceed)
            {
                Assert.Equal(StandardDeckSize - num, deck.Cards.Count);
                Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);

                Assert.NotNull(dealt);

                Assert.Equal(num, dealt.Count);
                Assert.Equal(dealt.Distinct().Count(), dealt.Count);
            }
            else 
            {
                Assert.Equal(StandardDeckSize, deck.Cards.Count);
                Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);

                Assert.Null(dealt);
            }   
        }
    }
}
