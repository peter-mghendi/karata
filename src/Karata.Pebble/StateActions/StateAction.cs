namespace Karata.Pebble.StateActions;

public abstract record StateAction<TState>
{
    public abstract TState Apply(TState state);
}