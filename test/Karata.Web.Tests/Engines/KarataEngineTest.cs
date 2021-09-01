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
            var game1 = CreateTestGame();
            var cards1 = new List<Card>();
            var delta1 = new GameDelta() { Pick = 1 };
            data.Add((game1, cards1, true, delta1));

            // #2 - Single card of matching suit - VALID
            var game2 = CreateTestGame();
            var cards2 = new List<Card>() { Ten.Of(Spades) };
            var delta2 = new GameDelta();
            data.Add((game2, cards2, true, delta2));

            // #3 - Single card of different suit - INVALID
            var game3 = CreateTestGame();
            var cards3 = new List<Card>() { Ten.Of(Hearts) };
            var delta3 = new GameDelta();
            data.Add((game3, cards3, false, delta3));

            // #4 - Multiple cards of same face - VALID
            var game4 = CreateTestGame();
            var cards4 = new List<Card>() {
                Nine.Of(Hearts),
                Nine.Of(Diamonds),
                Nine.Of(Clubs)
            };
            var delta4 = new GameDelta();
            data.Add((game4, cards4, true, delta4));

            // #5 - Single Jack of matching suit - VALID
            var game5 = CreateTestGame();
            var cards5 = new List<Card>() { Jack.Of(Spades) };
            var delta5 = new GameDelta() { Skip = 2 };
            data.Add((game5, cards5, true, delta5));

            // #6 - Multiple Jacks - VALID
            var game6 = CreateTestGame();
            var cards6 = new List<Card>() {
                Jack.Of(Spades),
                Jack.Of(Hearts),
                Jack.Of(Diamonds),
                Jack.Of(Clubs)
            };
            var delta6 = new GameDelta() { Skip = 5 };
            data.Add((game6, cards6, true, delta6));

            // #7 - Single King - VALID
            var game7 = CreateTestGame();
            var cards7 = new List<Card>() { King.Of(Spades) };
            var delta7 = new GameDelta { Reverse = true };
            data.Add((game7, cards7, true, delta7));

            // #8 - Even number of Kings - VALID
            var game8 = CreateTestGame();
            var cards8 = new List<Card>() {
                King.Of(Spades),
                King.Of(Hearts),
            };
            var delta8 = new GameDelta { Skip = 0 };
            data.Add((game8, cards8, true, delta8));

            // #9 - Single Question Card - VALID
            var game9 = CreateTestGame();
            var cards9 = new List<Card>() { Queen.Of(Spades) };
            var delta9 = new GameDelta { Pick = 1 };
            data.Add((game9, cards9, true, delta9));

            // #10 - Single Question Card wih answer - VALID
            var game10 = CreateTestGame();
            var cards10 = new List<Card>() {
                Queen.Of(Spades),
                Four.Of(Spades)
            };
            var delta10 = new GameDelta();
            data.Add((game10, cards10, true, delta10));

            // #11 - Multiple Question cards in valid order - VALID
            var game11 = CreateTestGame();
            var cards11 = new List<Card> {
                Queen.Of(Spades),
                Eight.Of(Spades),
                Eight.Of(Diamonds),
                Queen.Of(Diamonds)
            };
            var delta11 = new GameDelta() { Pick = 1 };
            data.Add((game11, cards11, true, delta11));

            // #12 - Multiple Question cards in invalid order - INVALID
            var game12 = CreateTestGame();
            var cards12 = new List<Card>() {
                Queen.Of(Spades),
                Eight.Of(Diamonds)
            };
            var delta12 = new GameDelta() { Pick = 1 };
            data.Add((game12, cards12, false, delta12));

            return data;
        }

        private static Game CreateTestGame()
        {
            var game = new Game();
            game.Pile.Push(Nine.Of(Spades));
            return game;
        }
    }
}
