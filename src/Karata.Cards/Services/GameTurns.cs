using Karata.Kit.Cards.Models;
using static Karata.Kit.Cards.Models.HandStatus;

namespace Karata.Cards.Services;

public static class GameTurns
{
    extension(Game game)
    {
        public void AdvanceTurn() => Advance(game);
    }
    
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
                case Active:
                    game.CurrentHand.Turns.Add(new Turn
                    {
                        Type = TurnType.Skip,
                        Hand = game.CurrentHand,
                        CreatedAt = DateTimeOffset.UtcNow
                    });

                    --skip;
                    break;
                case Inactive:
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
        while (game.CurrentHand.Status is Inactive)
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