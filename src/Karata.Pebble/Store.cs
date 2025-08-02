using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Karata.Pebble.Interceptors;
using Karata.Pebble.StateActions;

namespace Karata.Pebble;

/// <summary>
/// A mutable, Flux-inspired state store that applies actions (including lambda-based actions),
/// runs interceptors before and after each change, and notifies subscribers of state updates.
/// </summary>
/// <typeparam name="TState">The type of state to manage; must be a reference type.</typeparam>
public class Store<TState>(TState initial, ImmutableArray<Interceptor<TState>> interceptors) : IDisposable where TState : class
{
    private readonly Lock _lock = new();
    private readonly BehaviorSubject<TState> _subject = new(initial);
    private readonly Interceptor<TState>.Reducer _pipeline = BuildPipeline(interceptors);

    /// <summary>Gets the current state of the store.</summary>
    public TState State { get; private set; } = initial;
    
    /// <summary>
    /// Exposes state changes as an observable sequence. Subscribers will immediately receive the current state.
    /// </summary>
    public IObservable<TState> Changes => _subject.AsObservable();

    /// <summary>Applies the specified mutator function to the state as an anonymous action.</summary>
    /// <param name="mutator">The function that produces a new state from the old state.</param>
    public void Mutate(Func<TState, TState> mutator) => Mutate(new AnonymousStateAction<TState>(mutator));

    /// <summary>Applies the specified state action, running interceptors and notifying listeners.</summary>
    /// <param name="action">The <see cref="StateAction{TState}"/> to apply.</param>
    public void Mutate(StateAction<TState> action)
    {
        lock (_lock)
        {
            var updated = _pipeline(State, action);
            if (ReferenceEquals(updated, State)) return;
            
            State = updated;
            _subject.OnNext(State);
        }
    }

    /// <summary>Applies the given action to the state to produce a new state.</summary>
    /// <param name="state">The current state.</param>
    /// <param name="action">The action to apply.</param>
    /// <returns>The new state.</returns>
    private static TState Reduce(TState state, StateAction<TState> action) => action.Apply(state);
    
    private static Interceptor<TState>.Reducer BuildPipeline(IEnumerable<Interceptor<TState>> interceptors) => 
        interceptors.Reverse().Aggregate((Interceptor<TState>.Reducer)Reduce, (next, ic) => ic.Wrap(next));

    public void Dispose() => _subject.Dispose();
}