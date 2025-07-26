using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Server.Engine;
using Karata.Server.Engine.Exceptions;
using Karata.Server.Models;
using Karata.Shared.Models;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;
using static Karata.Shared.Models.CardRequestLevel;
using TestCase = (
    int Identifier,
    Karata.Server.Models.Game Game,
    System.Collections.Generic.List<Karata.Cards.Card> Cards,
    bool ExpectedValid,
    Karata.Shared.Models.GameDelta ExpectedDelta
);

namespace Karata.Server.Tests.Engines;

public class KarataEngineTest
{
#pragma warning disable xUnit1026, xUnit1045

    [Theory]
    [MemberData(nameof(ValidationTestCases))]
    public void ValidateTurnCardsTest(string identifier, Game game, List<Card> cards, bool expectedValidity)
    {
        var engine = new KarataEngine { Game = game, Cards = [..cards] };

        if (expectedValidity)
        {
            engine.EnsureTurnIsValid();
        }
        else
        {
            Assert.ThrowsAny<TurnValidationException>(() => engine.EnsureTurnIsValid());
        }
    }

    [Theory]
    [MemberData(nameof(GenerationTestCases))]
    public void GenerateTurnDeltaTest(string identifier, Game game, List<Card> cards, GameDelta expectedDelta)
    {
        var engine = new KarataEngine { Game = game, Cards = [..cards] };
        var actualDelta = engine.GenerateTurnDelta();

        Assert.Equal(expectedDelta, actualDelta);
    }

#pragma warning restore xUnit1026, xUnit1045

    public static TheoryData<string, Game, List<Card>, bool> ValidationTestCases => GetTestCases()
        .Aggregate(new TheoryData<string, Game, List<Card>, bool>(), (aggregate, @case) =>
        {
            aggregate.Add(@case.Identifier.ToString().PadLeft(2, '0'), @case.Game, @case.Cards, @case.ExpectedValid);
            return aggregate;
        });

    public static TheoryData<string, Game, List<Card>, GameDelta> GenerationTestCases => GetTestCases()
        .Where(@case => @case.ExpectedValid)
        .Aggregate(new TheoryData<string, Game, List<Card>, GameDelta>(), (aggregate, @case) =>
        {
            aggregate.Add(@case.Identifier.ToString().PadLeft(2, '0'), @case.Game, @case.Cards, @case.ExpectedDelta);
            return aggregate;
        });

    /**
     * <summary>Central Pool of test data</summary>
     * <remarks>
     *     Each test case is a tuple of 4 elements:
     *     - Identifier: A unique identifier for the test case
     *     - Game: The game state before the turn
     *     - Cards: The cards played by the player
     *     - ExpectedValid: The expected validity of the turn
     *     - ExpectedDelta: The expected delta of the turn
     *     A Note on ExpectedValidity:
     *     - ExpectedValidity is used to filter out tests cases that are also used for the generation test cases.
     *     A Note on Aces:
     *     - The word "Ace" as used in these tests is misleading - it is used to refer to a single AceValue
     *     <see cref="CardExtensions.GetAceValue" />, rather than any specific Ace card, i.e: [Ace of Spades, Ace of Hearts]
     *     has the same number of "Aces" as [Ace of Hearts, Ace of Diamonds, Ace of Clubs].
     *     - The reason for this is that the engine does not care about the suit of the Ace, only its value.
     * </remarks>
     */
    private static List<TestCase> GetTestCases()
    {
        List<TestCase> cases =
        [
            /*
             * BASIC OPERATIONS
             */
            // When no cards are played, the turn is valid
            (
                Identifier: 1,
                Game: ProvideGame(),
                Cards: [],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Pick = 1 }
            ),

            // When a single card of the same suit as the top card is played, the turn is valid
            (
                Identifier: 2,
                Game: ProvideGame(),
                Cards: [Ten.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Cards = [Ten.Of(Spades)] }
            ),

            // When a single card of a different suit than the top card is played, the turn is invalid
            (
                Identifier: 3,
                Game: ProvideGame(),
                Cards: [Ten.Of(Hearts)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            // When multiple cards of the same face as the top card are played, the turn is valid
            (
                Identifier: 4,
                Game: ProvideGame(),
                Cards: [Nine.Of(Hearts), Nine.Of(Diamonds), Nine.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Cards = [Nine.Of(Hearts), Nine.Of(Diamonds), Nine.Of(Clubs)] }
            ),

            /*
             * JACK
             */

            // When a single Jack is played, the turn is skipped
            (
                Identifier: 5,
                Game: ProvideGame(),
                Cards: [Jack.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Skip = 2, Cards = [Jack.Of(Spades)] }
            ),

            // When n Jacks are played, n + 1 turns are skipped
            (
                Identifier: 6,
                Game: ProvideGame(),
                Cards: [Jack.Of(Spades), Jack.Of(Hearts), Jack.Of(Diamonds), Jack.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    Skip = 5,
                    Cards = [Jack.Of(Spades), Jack.Of(Hearts), Jack.Of(Diamonds), Jack.Of(Clubs)]
                }
            ),

            /*
             * KING
             */

            // When a single King is played, the game is reversed
            (
                Identifier: 7,
                Game: ProvideGame(),
                Cards: [King.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Reverse = true, Cards = [King.Of(Spades)] }
            ),

            // When an even number of Kings are played, the player plays again
            (
                Identifier: 8,
                Game: ProvideGame(),
                Cards: [King.Of(Spades), King.Of(Hearts)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Skip = 0, Cards = [King.Of(Spades), King.Of(Hearts)] }
            ),

            /*
             * QUESTIONS
             */

            // When a single Question Card is played without an answer, the player picks up a card
            (
                Identifier: 9,
                Game: ProvideGame(),
                Cards: [Queen.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Pick = 1, Cards = [Queen.Of(Spades)] }
            ),

            // When a single Question Card is played with an answer, the play continues
            (
                Identifier: 10,
                Game: ProvideGame(),
                Cards: [Queen.Of(Spades), Four.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Cards = [Queen.Of(Spades), Four.Of(Spades)] }
            ),

            // When multiple Question Cards are played in valid order without an answer, the player picks up a card
            (
                Identifier: 11,
                Game: ProvideGame(),
                Cards: [Queen.Of(Spades), Eight.Of(Spades), Eight.Of(Diamonds), Queen.Of(Diamonds)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    Pick = 1,
                    Cards = [Queen.Of(Spades), Eight.Of(Spades), Eight.Of(Diamonds), Queen.Of(Diamonds)],
                }
            ),

            // When multiple Question Cards are played in invalid order, the turn is invalid
            (
                Identifier: 12,
                Game: ProvideGame(),
                Cards: [Queen.Of(Spades), Eight.Of(Diamonds)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            /*
             * JOKER
             */

            // When a Joker is played as the only card, the next player picks up 5 cards
            (
                Identifier: 13,
                Game: ProvideGame(),
                Cards: [Black.ColoredJoker()],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Give = 5, Cards = [Black.ColoredJoker()] }
            ),

            // When a Joker is played as an answer, the next player picks up 5 cards
            (
                Identifier: 14,
                Game: ProvideGame(),
                Cards: [Queen.Of(Spades), Eight.Of(Spades), Black.ColoredJoker(), Red.ColoredJoker()],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    Give = 5,
                    Cards = [Queen.Of(Spades), Eight.Of(Spades), Black.ColoredJoker(), Red.ColoredJoker()],
                }
            ),

            // When a non-Joker is played on top of a Joker in the same turn, the turn is invalid
            (
                Identifier: 15,
                Game: ProvideGame(),
                Cards: [Black.ColoredJoker(), Seven.Of(Spades)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            // If the previous player played a Joker, the next player picks up 5 cards
            (
                Identifier: 16,
                Game: ProvideGame(top: Black.ColoredJoker(), pick: 5),
                Cards: [],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Pick = 5 }
            ),

            // A player can "forward" a Joker to the next player using another Joker
            (
                Identifier: 17,
                Game: ProvideGame(top: Black.ColoredJoker(), pick: 5),
                Cards: [Black.ColoredJoker()],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Give = 5, Cards = [Black.ColoredJoker()] }
            ),

            // A player can "block" a Joker using an Ace
            (
                Identifier: 18,
                Game: ProvideGame(top: Black.ColoredJoker(), pick: 5),
                Cards: [Ace.Of(Diamonds)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Cards = [Ace.Of(Diamonds)] }
            ),

            // A player cannot "block" a Joker using any other card
            (
                Identifier: 19,
                Game: ProvideGame(top: Black.ColoredJoker(), pick: 5),
                Cards: [Seven.Of(Spades)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            /*
             * OTHER "BOMBS"
             */

            // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
            // or suit, Ace or Joker
            (
                Identifier: 20,
                Game: ProvideGame(top: Three.Of(Spades), pick: 3),
                Cards: [],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Pick = 3 }
            ),

            // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
            // or suit, Ace or Joker
            (
                Identifier: 21,
                Game: ProvideGame(Three.Of(Spades), 3),
                Cards: [Seven.Of(Spades)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
            // or suit, Ace or Joker
            (
                Identifier: 22,
                Game: ProvideGame(top: Three.Of(Spades), pick: 3),
                Cards: [Two.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Give = 2, Cards = [Two.Of(Spades)] }
            ),

            // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
            // or suit, Ace or Joker
            (
                Identifier: 23,
                Game: ProvideGame(top: Three.Of(Spades), pick: 3),
                Cards: [Three.Of(Diamonds)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Give = 3, Cards = [Three.Of(Diamonds)] }
            ),

            // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
            // or suit, Ace or Joker
            (
                Identifier: 24,
                Game: ProvideGame(top: Three.Of(Spades), pick: 3),
                Cards: [Black.ColoredJoker()],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Give = 5, Cards = [Black.ColoredJoker()] }
            ),

            // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
            // or suit, Ace or Joker
            (
                Identifier: 25,
                Game: ProvideGame(top: Three.Of(Spades), pick: 3),
                Cards: [Ace.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RequestLevel = SuitRequest, Cards = [Ace.Of(Spades)] }
            ),

            /*
             * ACES
             */

            // An ace can be played on top of any card, and the player can then request a card
            (
                Identifier: 26,
                Game: ProvideGame(),
                Cards: [Ace.Of(Diamonds)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RequestLevel = SuitRequest, Cards = [Ace.Of(Diamonds)] }
            ),

            // Two aces can be played on top of any card, and the player can then request a specific card
            (
                Identifier: 27,
                Game: ProvideGame(),
                Cards: [Ace.Of(Diamonds), Ace.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RequestLevel = CardRequest, Cards = [Ace.Of(Diamonds), Ace.Of(Clubs)] }
            ),

            // An ace of spades can be played on top of any card, and the player can then request a specific card
            (
                Identifier: 28,
                Game: ProvideGame(),
                Cards: [Ace.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RequestLevel = CardRequest, Cards = [Ace.Of(Spades)] }
            ),

            // When the requested card is played, the turn is valid and the request is removed
            (
                Identifier: 29,
                Game: ProvideGame(top: Ace.Of(Spades), request: None.Of(Spades)),
                Cards: [Six.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RemoveRequestLevels = 1, Cards = [Six.Of(Spades)] }
            ),

            // When the requested card is played, the turn is valid and the request is removed
            (
                Identifier: 30,
                Game: ProvideGame(top: Ace.Of(Spades), request: Nine.Of(Spades)),
                Cards: [Nine.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RemoveRequestLevels = 2, Cards = [Nine.Of(Spades)] }
            ),

            // When a card of a different suit from a specifically requested card is played, the turn is invalid
            (
                Identifier: 31,
                Game: ProvideGame(top: Ace.Of(Spades), request: Nine.Of(Spades)),
                Cards: [Nine.Of(Diamonds)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            // When a card of a different face from a specifically requested card is played, the turn is invalid
            (
                Identifier: 32,
                Game: ProvideGame(top: Ace.Of(Spades), request: Nine.Of(Spades)),
                Cards: [Four.Of(Spades)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            // When a card of a different suit from the requested suit is played, the turn is invalid
            (
                Identifier: 33,
                Game: ProvideGame(top: Ace.Of(Diamonds), request: None.Of(Spades)),
                Cards: [Nine.Of(Diamonds)],
                ExpectedValid: false,
                ExpectedDelta: new GameDelta()
            ),

            // When a single Ace is played on a suit request, the request is removed
            (
                Identifier: 34,
                Game: ProvideGame(top: Ace.Of(Diamonds), request: None.Of(Spades)),
                Cards: [Ace.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RemoveRequestLevels = 1, Cards = [Ace.Of(Clubs)] }
            ),

            // When a card is requested and none is played, the player picks up a card
            (
                Identifier: 35,
                Game: ProvideGame(top: Ace.Of(Diamonds), request: None.Of(Spades)),
                Cards: [],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Pick = 1 }
            ),

            // When a specific card is requested and a single Ace is played, the request is reduced to a suit request
            (
                Identifier: 36,
                Game: ProvideGame(top: Ace.Of(Hearts), request: Nine.Of(Spades)),
                Cards: [Ace.Of(Diamonds)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RemoveRequestLevels = 1, Cards = [Ace.Of(Diamonds)] }
            ),

            // When a specific card is requested and two Aces are played, the request is removed
            (
                Identifier: 37,
                Game: ProvideGame(top: Ace.Of(Hearts), request: Nine.Of(Spades)),
                Cards: [Ace.Of(Diamonds), Ace.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RemoveRequestLevels = 2, Cards = [Ace.Of(Diamonds), Ace.Of(Clubs)] }
            ),

            // When a specific card is requested and an Ace of Spades is played, the request is removed
            (
                Identifier: 38,
                Game: ProvideGame(top: Ace.Of(Hearts), request: Nine.Of(Spades)),
                Cards: [Ace.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { RemoveRequestLevels = 2, Cards = [Ace.Of(Spades)] }
            ),

            // When a specific card is requested and three Aces are played, the request is removed and the player can
            // request a suit
            (
                Identifier: 39,
                Game: ProvideGame(top: Ace.Of(Hearts), request: Nine.Of(Spades)),
                Cards: [Ace.Of(Diamonds), Ace.Of(Spades)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    RemoveRequestLevels = 2,
                    RequestLevel = SuitRequest,
                    Cards = [Ace.Of(Diamonds), Ace.Of(Spades)]
                }
            ),

            // When a specific card is requested and four aces are played, the request is removed and the player can
            // request a card
            (
                Identifier: 40,
                Game: ProvideGame(top: Ace.Of(Hearts), request: Nine.Of(Spades)),
                Cards: [Ace.Of(Diamonds), Ace.Of(Spades), Ace.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    RemoveRequestLevels = 2,
                    RequestLevel = CardRequest,
                    Cards = [Ace.Of(Diamonds), Ace.Of(Spades), Ace.Of(Clubs)]
                }
            ),

            // When an ace is requested is played, the request is removed and the player can request a suit.
            (
                Identifier: 41,
                Game: ProvideGame(top: Ace.Of(Spades), pick: 0, request: None.Of(Hearts)),
                Cards: [Ace.Of(Hearts)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    RemoveRequestLevels = 1,
                    RequestLevel = SuitRequest,
                    Cards = [Ace.Of(Hearts)]
                }
            ),

            // When an ace is requested is played, the request is removed and the player can request a suit.
            (
                Identifier: 42,
                Game: ProvideGame(top: Ace.Of(Spades), pick: 0, request: Ace.Of(Hearts)),
                Cards: [Ace.Of(Hearts)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    RemoveRequestLevels = 2,
                    RequestLevel = SuitRequest,
                    Cards = [Ace.Of(Hearts)]
                }
            ),

            // When an ace is requested is played followed by additional aces, the request is removed and the player can
            // request a card.
            (
                Identifier: 43,
                Game: ProvideGame(top: Ace.Of(Spades), pick: 0, request: Ace.Of(Hearts)),
                Cards: [Ace.Of(Hearts), Ace.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    RemoveRequestLevels = 2,
                    RequestLevel = CardRequest,
                    Cards = [Ace.Of(Hearts), Ace.Of(Clubs)]
                }
            ),

            // When a question card is requested and is played followed by  aces, the request is removed and the
            // player can request a card.
            (
                Identifier: 44,
                Game: ProvideGame(top: Ace.Of(Spades), pick: 0, request: Queen.Of(Hearts)),
                Cards: [Queen.Of(Hearts), Ace.Of(Hearts), Ace.Of(Clubs)],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta
                {
                    RemoveRequestLevels = 2,
                    RequestLevel = CardRequest,
                    Cards = [Queen.Of(Hearts), Ace.Of(Hearts), Ace.Of(Clubs)]
                }
            ),

            // When a card has been requested and no cards are played, the player picks a card.
            (
                Identifier: 45,
                Game: ProvideGame(top: Ace.Of(Spades), pick: 0, request: Ace.Of(Hearts)),
                Cards: [],
                ExpectedValid: true,
                ExpectedDelta: new GameDelta { Pick = 1 }
            ),
        ];

        // Quick check to make sure there are no duplicate identifiers
        Assert.Distinct(cases.Select(@case => @case.Identifier));
        return cases;
    }

    private static Game ProvideGame(Card? top = null, uint pick = 0, Card? request = null)
    {
        var game = new Game { Pick = pick, Request = request };
        game.Pile.Push(top ?? Nine.Of(Spades));
        return game;
    }
}