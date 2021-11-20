using System.Linq;
using Xunit;

namespace Karata.Cards.Tests;

public class PileTest
{
    [Fact]
    public void PilePropertyTest()
    {
        var pile = new Pile();
        Assert.Empty(pile);
    }

    [Fact]
    public void ReclaimTest()
    {
        var pile = new Pile(Deck.StandardDeck.ToList());
        pile.Reclaim();

        Assert.Single<Card>(pile);
    }
}