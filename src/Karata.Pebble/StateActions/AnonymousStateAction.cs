namespace Karata.Pebble.StateActions;

public sealed record AnonymousStateAction<TState>(Func<TState, TState> Mutator) : StateAction<TState>
{
    public override TState Apply(TState state) => Mutator(state);
}
