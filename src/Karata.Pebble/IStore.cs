namespace Karata.Pebble;

public interface IStore<TState> where TState : class
{
    TState State { get; }

    void Mutate(Func<TState, TState> mutator);
    void Observe(Action<TState> listener);
    void Forget(Action<TState> listener);
}