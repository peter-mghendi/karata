using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Karata.App.ViewModels;
using ReactiveUI;

namespace Karata.App.Views;

public partial class HomeView : ReactiveUserControl<HomeViewModel>
{
    private bool _called;

    public HomeView()
    {
        this.WhenActivated(disposables =>
        {
            if (_called) return;
            _called = true;

            if (DataContext is not HomeViewModel vm) return;

            Observable.FromAsync(async _ => await vm.LoadAsync()).Subscribe().DisposeWith(disposables);
        });
        AvaloniaXamlLoader.Load(this);
    }
}