using System.Collections.Immutable;
using Karata.Pebble;
using Karata.Pebble.Interceptors;
using Karata.Pebble.StateActions;
using Karata.Shared.Models;

namespace Karata.Shared.State;

public class TurnState(ImmutableList<Card> turn, ImmutableArray<Interceptor<ImmutableList<Card>>> interceptors)
    : Store<ImmutableList<Card>>(turn, interceptors)
{
    public record Add((Card Card, int Position) Info) : StateAction<ImmutableList<Card>>
    {
        public override ImmutableList<Card> Apply(ImmutableList<Card> state) => state.Insert(Info.Position, Info.Card);
    }

    public record Clear : StateAction<ImmutableList<Card>>
    {
        public override ImmutableList<Card> Apply(ImmutableList<Card> state) => state.Clear();
    }

    public record Remove(Card Card) : StateAction<ImmutableList<Card>>
    {
        public override ImmutableList<Card> Apply(ImmutableList<Card> state) => state.Remove(Card);
    }

    // TODO: CompositeAction
    public record Reorder((Card Card, int Position) Info) : StateAction<ImmutableList<Card>>
    {
        public override ImmutableList<Card> Apply(ImmutableList<Card> state) => state.Remove(Info.Card).Insert(Info.Position, Info.Card);
    }
}