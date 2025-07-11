using System.Reactive.Disposables;
using Karata.Pebble.Interceptors;
using Karata.Pebble.StateActions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karata.Pebble;

/// <summary>
/// A mutable, Flux-inspired state store that applies actions (including lambda-based actions),
/// runs interceptors before and after each change, and notifies subscribers of state updates.
/// </summary>
/// <typeparam name="TState">The type of state to manage; must be a reference type.</typeparam>
public class Store<TState>(TState initial, ILoggerFactory? factory = null) where TState : class
{
    private readonly Lock _lock = new();
    private readonly List<Action<TState>> _listeners = [];
    private readonly List<Interceptor<TState>> _interceptors = [];
    private readonly ILogger _logger = (factory ?? NullLoggerFactory.Instance).CreateLogger<Store<TState>>();

    /// <summary>Gets the current state of the store.</summary>
    public TState State { get; private set; } = initial;

    /// <summary>Applies the specified mutator function to the state as an anonymous action.</summary>
    /// <param name="mutator">The function that produces a new state from the old state.</param>
    public void Mutate(Func<TState, TState> mutator) => Mutate(new AnonymousStateAction<TState>(mutator));

    /// <summary>Applies the specified state action, running interceptors and notifying listeners.</summary>
    /// <param name="action">The <see cref="StateAction{TState}"/> to apply.</param>
    public void Mutate(StateAction<TState> action)
    {
        lock (_lock)
        {
            // Build pipeline
            Interceptor<TState>.Reducer core = Reduce;
            var pipeline = _interceptors
                .AsEnumerable()
                .Reverse()
                .Aggregate(core, (next, ic) => ic.Wrap(next));

            // Execute pipeline
            var updated = pipeline(State, action);
            if (ReferenceEquals(updated, State)) return;
            
            State = updated;
            
            // Notify listeners
            NotifyListeners();
        }
    }

    /// <summary>Registers a listener that will be invoked when the state changes.</summary>
    /// <param name="listener">An action to invoke with the new state.</param>
    /// <returns>An <see cref="IDisposable"/> token to remove the listener.</returns>
    public IDisposable Observe(Action<TState> listener)
    {
        _listeners.Add(listener);
        return Disposable.Create(() => Forget(listener));
    }

    /// <summary>Unregisters a previously registered listener.</summary>
    /// <param name="listener">The listener to remove.</param>
    public void Forget(Action<TState> listener) => _listeners.Remove(listener);

    /// <summary>Adds an interceptor that will run before and after each state change.</summary>
    /// <param name="interceptor">The interceptor to add.</param>
    public void AddInterceptor(Interceptor<TState> interceptor) => _interceptors.Add(interceptor);

    /// <summary>Applies the given action to the state to produce a new state.</summary>
    /// <param name="state">The current state.</param>
    /// <param name="action">The action to apply.</param>
    /// <returns>The new state.</returns>
    protected virtual TState Reduce(TState state, StateAction<TState> action) => action.Apply(state);

    private void NotifyListeners()
    {
        foreach (var listener in _listeners)
        {
            try
            {
                listener.Invoke(State);
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "Failed to call listener.");
            }
        }
    }
}