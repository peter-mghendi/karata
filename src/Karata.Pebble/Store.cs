using Karata.Pebble.Interceptors;

namespace Karata.Pebble;

public class Store<TState>(TState state) : IStore<TState> where TState : class
{
    private readonly List<Action<TState>> _listeners = [];
    private readonly List<IInterceptor<TState>> _interceptors = [];

    public TState State { get; private set; } = state;

    public void Mutate(Func<TState, TState> mutator)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.BeforeChange(State);
        }
        State = mutator(State);

        foreach (var interceptor in _interceptors)
        {
            interceptor.AfterChange(State);
        }
        NotifyListeners();
    }
    
    public void Mutate(StateAction<TState> action) => Mutate(state => Reduce(state, action));

    public void Observe(Action<TState> listener) => _listeners.Add(listener);
    
    public void Forget(Action<TState> listener) => _listeners.Remove(listener);

    protected virtual TState Reduce(TState state, StateAction<TState> action) => action.Execute(state);

    protected void AddInterceptor(IInterceptor<TState> interceptor) => _interceptors.Add(interceptor);

    private void NotifyListeners()
    {
        foreach (var listener in _listeners)
        {
            listener.Invoke(State);
        }
    }
}