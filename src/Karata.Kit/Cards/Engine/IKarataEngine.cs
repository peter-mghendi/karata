using System.Collections.Immutable;
using Karata.Kit.Cards.Models;
using Karata.Pips;
using Karata.Pips.Extensions;
using static Karata.Pips.Card.CardFace;

namespace Karata.Kit.Cards.Engine;

public interface IKarataEngine
{
    /// <summary>The name of the engine.</summary>
    string Name { get; }
    
    /// <summary>
    /// Evaluates a turn, returning a <see cref="TurnEvaluation"/> that indicates whether the turn is valid and what effect it has on the game state.
    /// </summary>
    /// <param name="game">The current game state.</param>
    /// <param name="cards">The proposed cards.</param>
    /// <returns>
    /// A <see cref="TurnEvaluation"/> representing the turn's validity and effect on the game.
    /// </returns>
    TurnEvaluation EvaluateTurn(GameData game, ImmutableArray<Card> cards);

    public bool TestTurn(GameData game, ImmutableArray<Card> cards) => EvaluateTurn(game, cards) switch
    {
        TurnDelta delta => true,
        TurnProblem problem => false,
    };

    /// <summary>
    /// Helper method to determine whether a card matches the request.
    /// </summary>
    /// <param name="request">The <see cref="GameData.Request"/> to check.</param>
    /// <param name="match">The card to check against the request.</param>
    /// <returns>True if  the request matches, false otherwise.</returns>
    protected static bool RequestMatches(Card? request, Card match) => request switch
    {
        null => true,
        { Face: None } => match.SuitEquals(request),
        { Face: not None } => match.SuitEquals(request) && match.FaceEquals(request),
    };
}