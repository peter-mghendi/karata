using System.Linq;
using Xunit;

namespace Karata.Cards.Tests
{
    public class PileTest
    {
        [Fact]
        public void PilePropertyTest()
        {
            var pile = new Pile();
            Assert.Empty(pile.Cards);

            pile.Cards = Deck.StandardDeck.Cards;
            Assert.NotEmpty(pile.Cards);
            Assert.Equal(54, pile.Cards.Count);
            Assert.Equal(pile.Cards.Distinct().Count(), pile.Cards.Count);

            pile.RemoveAllButTop();
            Assert.Single<Card>(pile.Cards);
        }
    }
}
