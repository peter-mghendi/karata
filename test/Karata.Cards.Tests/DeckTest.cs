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
            var deck = Deck.StandardDeck;
            _ = deck.Deal();

            Assert.Equal(StandardDeckSize - 1, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public void DealManyTest(int num)
        {
            var deck = Deck.StandardDeck;
            var dealt = deck.DealMany(num);

            Assert.Equal(StandardDeckSize - num, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);
            Assert.Equal(num, dealt.Count);
            Assert.Equal(dealt.Distinct().Count(), dealt.Count);
        }
    }
}
