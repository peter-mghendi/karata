using System.Collections.Immutable;

namespace Karata.Pebble.StateActions;

public record CompositeStateAction<TState>(ImmutableList<StateAction<TState>> Actions) : StateAction<TState>
{
    public override TState Apply(TState state) => Actions.Aggregate(state, (aggregate, action) => action.Apply(aggregate));
}
