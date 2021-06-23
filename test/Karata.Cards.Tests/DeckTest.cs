using System.Linq;
using Xunit;
using static Karata.Cards.Shufflers.ShuffleAlgorithm;

namespace Karata.Cards.Tests
{
    public class DeckTest
    {
        [Fact]
        public void DeckPropertyTest()
        {
            var deck = new Deck();
            Assert.Empty(deck.Cards);

            deck = Deck.StandardDeck;
            Assert.NotEmpty(deck.Cards);
            Assert.Equal(54, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);

            deck.Shuffle();
            Assert.NotEmpty(deck.Cards);
            Assert.Equal(54, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);

            deck.Shuffle(OrderByRandom);
            Assert.NotEmpty(deck.Cards);
            Assert.Equal(54, deck.Cards.Count);
            Assert.Equal(deck.Cards.Distinct().Count(), deck.Cards.Count);
        }
    }
}
