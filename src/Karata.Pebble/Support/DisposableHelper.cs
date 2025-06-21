namespace Karata.Pebble.Support;

// TODO: [Future] To avoid maintaining this, explore using System.Reactive's Disposable.Create
public static class DisposableHelper
{
    public static IDisposable Create(Action dispose) => new AnonymousDisposable(dispose);

    private sealed class AnonymousDisposable(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));

        public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}