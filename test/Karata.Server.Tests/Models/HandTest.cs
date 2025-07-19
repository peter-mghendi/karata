using Karata.Cards.Extensions;
using Karata.Server.Models;
using Karata.Shared.Models;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Server.Tests.Models;

public class HandTest
{

    [Fact]
    public void TestHandDataAlwaysHasEmptyCards()
    {
        var hand = new Hand
        {
            Status = HandStatus.Connected,
            Player = new User(),
            Cards = [Queen.Of(Hearts)]
        };
        
        Assert.Empty(hand.ToData().Cards);
    }
}