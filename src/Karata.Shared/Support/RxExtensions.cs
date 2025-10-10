using System.Reactive.Disposables;

namespace Karata.Shared.Support;

public static class RxExtensions
{
    public static void AddTo(this IDisposable d, CompositeDisposable bag) => bag.Add(d);
}