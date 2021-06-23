using Xunit;

namespace Karata.Cards.Tests
{
    public class HandTest
    {
        [Fact]
        public void HandPropertyTest()
        {
            var hand = new Hand();
            Assert.Empty(hand.Cards);
        }
    }
}
