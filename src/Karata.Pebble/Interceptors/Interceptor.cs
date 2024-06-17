namespace Karata.Pebble.Interceptors;

public abstract class Interceptor<TState> : IInterceptor<TState>
{
    public virtual void BeforeChange(TState state) { }
    public virtual void AfterChange(TState state) { }
}