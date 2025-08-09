using Karata.Shared.Models;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Services;

public static class GameTurns
{
    /// <summary>
    ///  Encapsulates logic to determine the next playable turn in a game.
    /// </summary>
    /// <remarks>
    /// - Does not validate against games with insufficient playable turns.
    /// - Assumes that the current turn has already been added, and will start evaluating the following turn.
    /// </remarks>
    public static void Advance(Game game)
    {
        var skip = game.CurrentHand.Turns.LastOrDefault()?.Delta?.Skip ?? 1;

        // process skip turns, counting only connected hands.
        while (skip > 0)
        {
            game.CurrentTurn = game.NextTurn;
            switch (game.CurrentHand.Status)
            {
                case Online:
                case Offline:
                    game.CurrentHand.Turns.Add(new Turn
                    {
                        Type = TurnType.Skip,
                        Hand = game.CurrentHand,
                        CreatedAt = DateTimeOffset.UtcNow
                    });

                    --skip;
                    break;
                case Away:
                    game.CurrentHand.Turns.Add(new Turn
                    {
                        Type = TurnType.Void,
                        Hand = game.CurrentHand,
                        CreatedAt = DateTimeOffset.UtcNow
                    });

                    break;
            }
        }

        // correct to the next available player
        while (game.CurrentHand.Status is Away)
        {
            game.CurrentHand.Turns.Add(new Turn
            {
                Type = TurnType.Void,
                Hand = game.CurrentHand,
                CreatedAt = DateTimeOffset.UtcNow
            });
            game.CurrentTurn = game.NextTurn;
        }
    }
}