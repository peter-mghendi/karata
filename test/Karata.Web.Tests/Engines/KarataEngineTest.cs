using System;
using System.Collections.Generic;
using System.Linq;
using Karata.Cards;
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
        private static List<(Game Game, List<Card> Cards, bool ExpectedValidity, GameDelta ExpectedDelta)> GetBaseData()
        {
            var data = new List<(Game, List<Card>, bool, GameDelta)>();

            // #1 - No cards played - VALID
            var game1 = new Game();
            game1.Pile.Push(new(Spades, Nine));
            data.Add((game1, new(), true, new GameDelta() with { Pick = 1 }));

            return data;
        }
    }
}
