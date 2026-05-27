using System.Collections.Immutable;
using Karata.Cards.Extensions;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;
using Karata.Kit.Support;
using static Karata.Cards.Card.CardFace;

namespace Karata.Kit.Bot.Strategy;

public sealed class RandomValidBotStrategy(IKarataEngine engine) : IBotStrategy
{
    private static readonly Random Random = new();

    public string Name => "Random valid bot";

    public string Description => "A bot that plays a random valid turn.";

    public Task<TurnPlan> PlanAsync(RoomData room, CancellationToken ct = default)
    {
        var cards = room.Game.CurrentHand.Cards;
        var move = cards
            .Permutations()
            .OrderBy(_ => Random.Next())
            .FirstOrDefault(candidates => engine.TestTurn(room.Game, [..candidates]))
            ?.ToImmutableArray() ?? [];


        return Task.FromResult(
            new TurnPlan(
                Move: move,
                RequestFactory: specific => cards.Except(move).FirstOrDefault() switch
                {
                    { } candidate when specific => candidate,
                    { } candidate => None.Of(candidate.Suit),
                    null => null,
                },
                LastCardStatusFactory: _ => true
            )
        );
    }
}