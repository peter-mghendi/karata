using System.Collections.Immutable;
using Karata.Cards.Extensions;
using Karata.Shared.Engine.Exceptions;
using Karata.Shared.Models;
using static Karata.Cards.Card.CardFace;

namespace Karata.Shared.Engine;

public interface IKarataEngine
{
    /// <summary>
    /// Evaluates a turn, returning a <see cref="GameDelta"/> if the turn is valid, and throwing a
    /// <see cref="TurnValidationException"/> if it is not.
    /// </summary>
    /// <param name="game">THe current game state.</param>
    /// <param name="cards">The proposed cards.</param>
    /// <returns>A <see cref="GameDelta"/> representing the turn's effect on the game.</returns>
    GameDelta EvaluateTurn(GameData game, ImmutableArray<Card> cards);

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