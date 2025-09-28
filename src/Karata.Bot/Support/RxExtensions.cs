using System.Reactive.Disposables;

namespace Karata.Bot.Services;

static class RxExtensions
{
    public static void AddTo(this IDisposable d, CompositeDisposable bag) => bag.Add(d);
}