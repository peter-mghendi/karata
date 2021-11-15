using System.Collections.Generic;
using System.Linq;
using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Web.Engines;
using Karata.Web.Models;
using Xunit;
using static Karata.Cards.Card.CardColor;
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

            /*
             * BASIC OPERATIONS
             */

            // #1 - No cards played - VALID
            var game1 = CreateTestGame();
            var cards1 = new List<Card>();
            var delta1 = new GameDelta { Pick = 1 };
            data.Add((game1, cards1, true, delta1));

            // #2 - Single card of matching suit - VALID
            var game2 = CreateTestGame();
            var cards2 = new List<Card> { Ten.Of(Spades) };
            var delta2 = new GameDelta();
            data.Add((game2, cards2, true, delta2));

            // #3 - Single card of different suit - INVALID
            var game3 = CreateTestGame();
            var cards3 = new List<Card> { Ten.Of(Hearts) };
            var delta3 = new GameDelta();
            data.Add((game3, cards3, false, delta3));

            // #4 - Multiple cards of same face - VALID
            var game4 = CreateTestGame();
            var cards4 = new List<Card> 
            {
                Nine.Of(Hearts),
                Nine.Of(Diamonds),
                Nine.Of(Clubs)
            };
            var delta4 = new GameDelta();
            data.Add((game4, cards4, true, delta4));

            /*
             * JACK
             */

            // #5 - Single Jack of matching suit - VALID
            var game5 = CreateTestGame();
            var cards5 = new List<Card> { Jack.Of(Spades) };
            var delta5 = new GameDelta { Skip = 2 };
            data.Add((game5, cards5, true, delta5));

            // #6 - Multiple Jacks - VALID
            var game6 = CreateTestGame();
            var cards6 = new List<Card> 
            {
                Jack.Of(Spades),
                Jack.Of(Hearts),
                Jack.Of(Diamonds),
                Jack.Of(Clubs)
            };
            var delta6 = new GameDelta { Skip = 5 };
            data.Add((game6, cards6, true, delta6));

            /*
             * KING
             */

            // #7 - Single King - VALID
            var game7 = CreateTestGame();
            var cards7 = new List<Card> { King.Of(Spades) };
            var delta7 = new GameDelta { Reverse = true };
            data.Add((game7, cards7, true, delta7));

            // #8 - Even number of Kings - VALID
            var game8 = CreateTestGame();
            var cards8 = new List<Card>
            {
                King.Of(Spades),
                King.Of(Hearts),
            };
            var delta8 = new GameDelta { Skip = 0 };
            data.Add((game8, cards8, true, delta8));

            /*
             * QUESTIONS
             */

            // #9 - Single Question Card - VALID
            var game9 = CreateTestGame();
            var cards9 = new List<Card> { Queen.Of(Spades) };
            var delta9 = new GameDelta { Pick = 1 };
            data.Add((game9, cards9, true, delta9));

            // #10 - Single Question Card wih answer - VALID
            var game10 = CreateTestGame();
            var cards10 = new List<Card>
            {
                Queen.Of(Spades),
                Four.Of(Spades)
            };
            var delta10 = new GameDelta();
            data.Add((game10, cards10, true, delta10));

            // #11 - Multiple Question cards in valid order - VALID
            var game11 = CreateTestGame();
            var cards11 = new List<Card> 
            {
                Queen.Of(Spades),
                Eight.Of(Spades),
                Eight.Of(Diamonds),
                Queen.Of(Diamonds)
            };
            var delta11 = new GameDelta { Pick = 1 };
            data.Add((game11, cards11, true, delta11));

            // #12 - Multiple Question cards in invalid order - INVALID
            var game12 = CreateTestGame();
            var cards12 = new List<Card>
            {
                Queen.Of(Spades),
                Eight.Of(Diamonds)
            };
            var delta12 = new GameDelta { Pick = 1 };
            data.Add((game12, cards12, false, delta12));

            /*
             * JOKER
             */

            // #13 - Joker as the only card - VALID
            var game13 = CreateTestGame();
            var cards13 = new List<Card> { Black.ColoredJoker() };
            var delta13 = new GameDelta { Give = 5 };
            data.Add((game13, cards13, true, delta13));

            // #14 - Multiple cards involving Joker - VALID
            var game14 = CreateTestGame();
            var cards14 = new List<Card>
            {
                Queen.Of(Spades),
                Eight.Of(Spades),
                Black.ColoredJoker(),
                Red.ColoredJoker()
            };
            var delta14 = new GameDelta { Give = 5 };
            data.Add((game14, cards14, true, delta14));

            // #15 - Multiple cards involving Joker - INVALID
            var game15 = CreateTestGame();
            var cards15 = new List<Card>
            {
                Black.ColoredJoker(),
                Seven.Of(Spades)
            };
            var delta15 = new GameDelta();
            data.Add((game15, cards15, false, delta15));

            // #16 - Joker at the bottom - VALID
            var game16 = CreateTestGame(Black.ColoredJoker(), 5);
            var cards16 = new List<Card>();
            var delta16 = new GameDelta { Pick = 5 };
            data.Add((game16, cards16, true, delta16));

            // #17 - Joker at the bottom - VALID
            var game17 = CreateTestGame(Black.ColoredJoker(), 5);
            var cards17 = new List<Card> { Black.ColoredJoker() };
            var delta17 = new GameDelta { Give = 5 };
            data.Add((game17, cards17, true, delta17));

            // #18 - Joker at the bottom - VALID
            var game18 = CreateTestGame(Black.ColoredJoker(), 5);
            var cards18 = new List<Card> { Ace.Of(Spades) };
            var delta18 = new GameDelta();
            data.Add((game18, cards18, true, delta18));

            // #19 - Joker at the bottom - INVALID
            var game19 = CreateTestGame(Black.ColoredJoker(), 5);
            var cards19 = new List<Card> { Seven.Of(Spades) };
            var delta19 = new GameDelta();
            data.Add((game19, cards19, false, delta19));

            /*
             * OTHER "BOMBS"
             */

            // #20 - "Bomb" at the bottom - VALID
            var game20 = CreateTestGame(Three.Of(Spades), 3);
            var cards20 = new List<Card>();
            var delta20 = new GameDelta { Pick = 3 };
            data.Add((game20, cards20, true, delta20));

            // #21 - "Bomb" at the bottom - INVALID
            var game21 = CreateTestGame(Three.Of(Spades), 3);
            var cards21 = new List<Card> { Seven.Of(Spades) };
            var delta21 = new GameDelta();
            data.Add((game21, cards21, false, delta21));

            // #22 - "Bomb" at the bottom - VALID
            var game22 = CreateTestGame(Three.Of(Spades), 3);
            var cards22 = new List<Card> { Two.Of(Spades) };
            var delta22 = new GameDelta { Give = 2 };
            data.Add((game22, cards22, true, delta22));

            // #23 - "Bomb" at the bottom - INVALID
            var game23 = CreateTestGame(Three.Of(Spades), 3);
            var cards23 = new List<Card> { Three.Of(Diamonds) };
            var delta23 = new GameDelta { Give = 3 };
            data.Add((game23, cards23, true, delta23));

            // #24 - "Bomb" at the bottom - VALID
            var game24 = CreateTestGame(Three.Of(Spades), 3);
            var cards24 = new List<Card> { Black.ColoredJoker() };
            var delta24 = new GameDelta { Give = 5 };
            data.Add((game24, cards24, true, delta24));

            /*
             * ACES
             */

            // #25 - Single Ace - VALID
            var game25 = CreateTestGame();
            var cards25 = new List<Card> { Ace.Of(Diamonds) };
            var delta25 = new GameDelta { HasRequest = true };
            data.Add((game25, cards25, true, delta25));

            // #26 - Two Aces - VALID
            var game26 = CreateTestGame();
            var cards26 = new List<Card> 
            { 
                Ace.Of(Diamonds),
                Ace.Of(Clubs)
            };
            var delta26 = new GameDelta { 
                HasRequest = true, 
                HasSpecificRequest = true 
            };
            data.Add((game26, cards26, true, delta26));

            // #27 - Ace of Spades - VALID
            // #28 - Multiple Aces on top of a "bomb" - VALID
            // #29 - Card request - VALID
            // #30 - Card request - INVALID

            return data;
        }

        private static Game CreateTestGame(Card firstCard = null, uint pick = 0)
        {
            var game = new Game() { Pick = pick };
            game.Pile.Push(firstCard ?? Nine.Of(Spades));
            return game;
        }
    }
}
