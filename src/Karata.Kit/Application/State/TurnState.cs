using System.Collections.Immutable;
using Karata.Cards;
using Karata.Pebble;
using Karata.Pebble.Interceptors;
using Karata.Pebble.StateActions;

namespace Karata.Kit.Application.State;

public class TurnState(ImmutableList<Card> turn, ImmutableArray<Interceptor<ImmutableList<Card>>> interceptors)
    : Store<ImmutableList<Card>>(turn, interceptors)
{
    public record Insert((Card Card, int Position) Info) : StateAction<ImmutableList<Card>>
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

    public record Reorder((Card Card, int Position) Info)
        : CompositeStateAction<ImmutableList<Card>>([new Remove(Info.Card), new Insert(Info)]);
}