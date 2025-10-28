using System;
using Karata.App.ViewModels;
using Karata.App.Views;
using ReactiveUI;

namespace Karata.App;

public class AppViewLocator : ReactiveUI.IViewLocator
{
    public IViewFor ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        HomeViewModel context => new HomeView { DataContext = context },
        PlayViewModel context => new PlayView { DataContext = context },
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}