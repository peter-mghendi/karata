namespace Karata.Pebble;

public abstract record StateAction<TState>
{
    public abstract TState Execute(TState state);
}