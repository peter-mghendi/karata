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

namespace Karata.Web.Tests.Engines;

public class KarataEngineTest
{
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

    // Central pool of test data
    // There must be a better way to do this
    private static IEnumerable<(Game Game, List<Card> Cards, bool ExpectedValidity, GameDelta ExpectedDelta)> GetBaseData()
    {
        var data = new List<(Game, List<Card>, bool, GameDelta)>();

        /*
         * BASIC OPERATIONS
         */
        
        // When no cards are played, the turn is valid
        var game1 = CreateTestGame();
        var cards1 = new List<Card>();
        var delta1 = new GameDelta { Pick = 1 };
        data.Add((game1, cards1, true, delta1));

        // When a single card of the same suit as the top card is played, the turn is valid
        var game2 = CreateTestGame();
        var cards2 = new List<Card> { Ten.Of(Spades) };
        var delta2 = new GameDelta();
        data.Add((game2, cards2, true, delta2));

        // When a single card of a different suit than the top card is played, the turn is invalid
        var game3 = CreateTestGame();
        var cards3 = new List<Card> { Ten.Of(Hearts) };
        var delta3 = new GameDelta();
        data.Add((game3, cards3, false, delta3));

        // When multiple cards of the same face as the top card are played, the turn is valid
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
        
        // When a single Jack is played, the turn is skipped
        var game5 = CreateTestGame();
        var cards5 = new List<Card> { Jack.Of(Spades) };
        var delta5 = new GameDelta { Skip = 2 };
        data.Add((game5, cards5, true, delta5));

        // When n Jacks are played, n + 1 turns are skipped
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
        
        // When a single King is played, the game is reversed
        var game7 = CreateTestGame();
        var cards7 = new List<Card> { King.Of(Spades) };
        var delta7 = new GameDelta { Reverse = true };
        data.Add((game7, cards7, true, delta7));
        
        // When an even number of Kings are played, the player plays again
        var game8 = CreateTestGame();
        var cards8 = new List<Card>
        {
            King.Of(Spades),
            King.Of(Hearts)
        };
        var delta8 = new GameDelta { Skip = 0 };
        data.Add((game8, cards8, true, delta8));

        /*
         * QUESTIONS
         */

        // When a single Question Card is played without an answer, the player picks up a card
        var game9 = CreateTestGame();
        var cards9 = new List<Card> { Queen.Of(Spades) };
        var delta9 = new GameDelta { Pick = 1 };
        data.Add((game9, cards9, true, delta9));

        // When a single Question Card is played with an answer, the play continues
        var game10 = CreateTestGame();
        var cards10 = new List<Card>
        {
            Queen.Of(Spades),
            Four.Of(Spades)
        };
        var delta10 = new GameDelta();
        data.Add((game10, cards10, true, delta10));
        
        // When multiple Question Cards are played in valid order without an answer, the player picks up a card
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
        
        // When multiple Question Cards are played in invalid order, the turn is invalid
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

        // When a Joker is played as the only card, the next player picks up 5 cards
        var game13 = CreateTestGame();
        var cards13 = new List<Card> { Black.ColoredJoker() };
        var delta13 = new GameDelta { Give = 5 };
        data.Add((game13, cards13, true, delta13));

        // When a Joker is played as an answer, the next player picks up 5 cards
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
        
        // When a non-Joker is played on top of a Joker in the same turn, the turn is invalid
        var game15 = CreateTestGame();
        var cards15 = new List<Card>
        {
            Black.ColoredJoker(),
            Seven.Of(Spades)
        };
        var delta15 = new GameDelta();
        data.Add((game15, cards15, false, delta15));
        
        // If the previous player played a Joker, the next player picks up 5 cards
        var game16 = CreateTestGame(Black.ColoredJoker(), 5);
        var cards16 = new List<Card>();
        var delta16 = new GameDelta { Pick = 5 };
        data.Add((game16, cards16, true, delta16));
        
        // A player can "forward" a Joker to the next player using another Joker
        var game17 = CreateTestGame(Black.ColoredJoker(), 5);
        var cards17 = new List<Card> { Black.ColoredJoker() };
        var delta17 = new GameDelta { Give = 5 };
        data.Add((game17, cards17, true, delta17));

        // A player can "block" a Joker using an Ace
        var game18 = CreateTestGame(Black.ColoredJoker(), 5);
        var cards18 = new List<Card> { Ace.Of(Diamonds) };
        var delta18 = new GameDelta();
        data.Add((game18, cards18, true, delta18));

        // A player cannot "block" a Joker using any other card
        var game19 = CreateTestGame(Black.ColoredJoker(), 5);
        var cards19 = new List<Card> { Seven.Of(Spades) };
        var delta19 = new GameDelta();
        data.Add((game19, cards19, false, delta19));

        /*
         * OTHER "BOMBS"
         */

        // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
        // or suit, Ace or Joker
        var game20 = CreateTestGame(Three.Of(Spades), 3);
        var cards20 = new List<Card>();
        var delta20 = new GameDelta { Pick = 3 };
        data.Add((game20, cards20, true, delta20));

        // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
        // or suit, Ace or Joker
        var game21 = CreateTestGame(Three.Of(Spades), 3);
        var cards21 = new List<Card> { Seven.Of(Spades) };
        var delta21 = new GameDelta();
        data.Add((game21, cards21, false, delta21));

        // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
        // or suit, Ace or Joker
        var game22 = CreateTestGame(Three.Of(Spades), 3);
        var cards22 = new List<Card> { Two.Of(Spades) };
        var delta22 = new GameDelta { Give = 2 };
        data.Add((game22, cards22, true, delta22));
        
        // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
        // or suit, Ace or Joker
        var game23 = CreateTestGame(Three.Of(Spades), 3);
        var cards23 = new List<Card> { Three.Of(Diamonds) };
        var delta23 = new GameDelta { Give = 3 };
        data.Add((game23, cards23, true, delta23));
        
        // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
        // or suit, Ace or Joker
        var game24 = CreateTestGame(Three.Of(Spades), 3);
        var cards24 = new List<Card> { Black.ColoredJoker() };
        var delta24 = new GameDelta { Give = 5 };
        data.Add((game24, cards24, true, delta24));

        // If the previous player played a "bomb", the player has to pick up the cards or play a "bomb" of the same face
        // or suit, Ace or Joker
        var game25 = CreateTestGame(Three.Of(Spades), 3);
        var cards25 = new List<Card> { Ace.Of(Spades) };
        var delta25 = new GameDelta { HasRequest = true };
        data.Add((game25, cards25, true, delta25));

        /*
         * ACES
         */
        
        // An ace can be played on top of any card, and the player can then request a card
        var game26 = CreateTestGame();
        var cards26 = new List<Card> { Ace.Of(Diamonds) };
        var delta26 = new GameDelta { HasRequest = true };
        data.Add((game26, cards26, true, delta26));
        
        // Two aces can be played on top of any card, and the player can then request a specific card
        var game27 = CreateTestGame();
        var cards27 = new List<Card>
        {
            Ace.Of(Diamonds),
            Ace.Of(Clubs)
        };
        var delta27 = new GameDelta
        {
            HasRequest = true,
            HasSpecificRequest = true
        };
        data.Add((game27, cards27, true, delta27));
        
        // An ace of spades can be played on top of any card, and the player can then request a specific card
        var game28 = CreateTestGame();
        var cards28 = new List<Card> { Ace.Of(Spades) };
        var delta28 = new GameDelta
        {
            HasRequest = true,
            HasSpecificRequest = true
        };
        data.Add((game28, cards28, true, delta28));
        
        // When the requested card is played, the turn is valid
        var game29 = CreateTestGame(Ace.Of(Spades), request: Nine.Of(Spades));
        var cards29 = new List<Card> { Nine.Of(Spades) };
        var delta29 = new GameDelta();
        data.Add((game29, cards29, true, delta29));
        
        // When a card of a different suit from a specifically requested card is played, the turn is invalid
        var game30 = CreateTestGame(Ace.Of(Spades), request: Nine.Of(Spades));
        var cards30 = new List<Card> { Nine.Of(Diamonds) };
        var delta30 = new GameDelta();
        data.Add((game30, cards30, false, delta30));
        
        // When a card of a different face from a specifically requested card is played, the turn is invalid
        var game31 = CreateTestGame(Ace.Of(Spades), request: Nine.Of(Spades));
        var cards31 = new List<Card> { Four.Of(Spades) };
        var delta31 = new GameDelta();
        data.Add((game31, cards31, false, delta31));
        
        // When a card of a different suit from the requested suit is played, the turn is invalid
        var game32 = CreateTestGame(Ace.Of(Diamonds), request: None.Of(Spades));
        var cards32 = new List<Card> { Nine.Of(Diamonds) };
        var delta32 = new GameDelta();
        data.Add((game32, cards32, false, delta32));
        
        // When a card of the requested suit is played, the turn is valid
        var game33 = CreateTestGame(Ace.Of(Diamonds), request: None.Of(Spades));
        var cards33 = new List<Card> { Ace.Of(Clubs) };
        var delta33 = new GameDelta { HasRequest = true };
        data.Add((game33, cards33, true, delta33));
        
        // When a card is requested and none is played, the player picks up a card
        var game34 = CreateTestGame(Ace.Of(Diamonds), request: None.Of(Spades));
        var cards34 = new List<Card>();
        var delta34 = new GameDelta { RemovesPreviousRequest = false, Pick = 1 };
        data.Add((game34, cards34, true, delta34));

        // // When a specific card is requested and a single Ace is played, the request is reduced to a suit request
        // var game35 = CreateTestGame(Ace.Of(Hearts), request: Nine.Of(Spades));
        // var cards35 = new List<Card> { Ace.Of(Diamonds) };
        // var delta35 = new GameDelta { HasRequest = true };
        // data.Add((game35, cards35, true, delta35));
        //
        // // When a specific card is requested and a two Aces are played, the request is removed
        // var game36 = CreateTestGame(Ace.Of(Hearts), request: Nine.Of(Spades));
        // var cards36 = new List<Card>
        // {
        //     Ace.Of(Diamonds),
        //     Ace.Of(Clubs)
        // };
        // var delta36 = new GameDelta
        // {
        //     RemovesPreviousRequest = true,
        //     // RemovesPreviousSpecificRequest = true
        // };
        // data.Add((game36, cards36, true, delta36));
        //
        // // When a specific card is requested and an Ace of Spades is played, the request is removed
        // var game37 = CreateTestGame(Ace.Of(Hearts), request: Nine.Of(Spades));
        // var cards37 = new List<Card> { Ace.Of(Spades) };
        // var delta37 = new GameDelta
        // {
        //     RemovesPreviousRequest = true,
        //     // RemovesPreviousSpecificRequest = true
        // };
        // data.Add((game37, cards37, true, delta37));

        return data;
    }

    private static Game CreateTestGame(Card firstCard = null, uint pick = 0, Card request = null)
    {
        var game = new Game { Pick = pick, CurrentRequest = request };
        game.Pile.Push(firstCard ?? Nine.Of(Spades));
        return game;
    }
}