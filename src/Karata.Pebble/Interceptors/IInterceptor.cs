namespace Karata.Pebble.Interceptors;

public interface IInterceptor<TState>
{
    void BeforeChange(TState state);
    void AfterChange(TState state);
}