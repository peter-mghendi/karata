using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Kit.Domain.Models;
using Karata.Server.Models;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Server.Tests.Models;

public class HandTest
{

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(50)]
    public void TestHandDataAlwaysHasDefaultCards(int count)
    {
        var hand = new Hand
        {
            Status = HandStatus.Online,
            Player = new User { Id = Guid.CreateVersion7().ToString(), Username = Guid.NewGuid().ToString() },
            Cards = [..Enumerable.Range(0, count).Select(_ => Queen.Of(Hearts))]
        };
        
        Assert.Equal(hand.Cards.Count, count);
        Assert.Single(hand.Cards.Distinct());
        Assert.DoesNotContain(hand.ToData().Cards, card => card != new Card());
    }
}