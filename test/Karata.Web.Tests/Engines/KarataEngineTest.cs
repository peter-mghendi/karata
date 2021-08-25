using System;
using System.Collections.Generic;
using System.Linq;
using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Web.Engines;
using Karata.Web.Models;
using Xunit;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;
using static Karata.Web.Models.Game;

namespace Karata.Web.Tests.Engines
{
    public class KarataEngineTest
    {
        [Theory]
        [MemberData(nameof(ValidationData))]
        public void ValidateTurnCardsTest(Game game, List<Card> cards, bool expectedValidity)
        {
            var engine = new KarataEngine();
            var actualValidity = engine.ValidateTurnCards(game, cards);
            Assert.Equal(expectedValidity, actualValidity);
        }

        [Theory]
        [MemberData(nameof(GenerationData))]
        public void GenerateTurnDeltaTest(Game game, List<Card> cards, GameDelta expectedDelta)
        {
            var engine = new KarataEngine();
            var actualDelta = engine.GenerateTurnDelta(game, cards);
            Assert.Equal(expectedDelta, actualDelta);
        }

        public static TheoryData<Game, List<Card>, bool> ValidationData => GetBaseData()
            .Aggregate(new TheoryData<Game, List<Card>, bool>(), (aggregate, datum) =>
            {
                aggregate.Add(datum.Game, datum.Cards, datum.ExpectedValidity);
                return aggregate;
            });

        public static TheoryData<Game, List<Card>, GameDelta> GenerationData => GetBaseData()
            .Where(datum => datum.ExpectedValidity)
            .Aggregate(new TheoryData<Game, List<Card>, GameDelta>(), (aggregate, datum) =>
            {
                aggregate.Add(datum.Game, datum.Cards, datum.ExpectedDelta);
                return aggregate;
            });

        // Central pool of test data
        // There must be a better way to do this
        private static List<(Game Game, List<Card> Cards, bool ExpectedValidity, GameDelta ExpectedDelta)> GetBaseData()
        {
            var data = new List<(Game, List<Card>, bool, GameDelta)>();

            // #1 - No cards played - VALID
            var game1 = new Game();
            game1.Pile.Push(Nine.Of(Spades));
            var cards1 = new List<Card>();
            var delta1 = new GameDelta() with { Pick = 1 };
            data.Add((game1, cards1, true, delta1));

            // #2 - Single card of matching suit - VALID
            var game2 = new Game();
            game2.Pile.Push(Nine.Of(Spades));
            var cards2 = new List<Card>() { Ten.Of(Spades) };
            var delta2 = new GameDelta();
            data.Add((game2, cards2, true, delta2));

            // #3 - Single card of different suit - INVALID
            var game3 = new Game();
            game3.Pile.Push(Nine.Of(Spades));
            var cards3 = new List<Card>() { Ten.Of(Hearts) };
            var delta3 = new GameDelta();
            data.Add((game3, cards3, false, delta3));

            // #4 - Multiple cards of same face - VALID
            var game4 = new Game();
            game4.Pile.Push(Nine.Of(Spades));
            var cards4 = new List<Card>() {
                Nine.Of(Hearts),
                Nine.Of(Diamonds),
                Nine.Of(Clubs)
            };
            var delta4 = new GameDelta();
            data.Add((game4, cards4, true, delta4));

            return data;
        }
    }
}
