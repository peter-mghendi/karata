namespace Karata.Pebble;

public abstract class FluxStore<TState>(Func<TState> initialStateFactory) : Store<TState>(initialStateFactory())
    where TState : class
{
    
    protected void Dispatch<TAction>(TAction action) where TAction : StateAction<TState> => Mutate(action);
}