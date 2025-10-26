using System.Reactive;
using Karata.Shared.Client;
using ReactiveUI;

namespace Karata.App.ViewModels;

public class ShellViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new();

    public ReactiveCommand<Unit, IRoutableViewModel> Back => Router.NavigateBack;

    public ReactiveCommand<Unit, IRoutableViewModel> Home { get; }

    public ReactiveCommand<Unit, IRoutableViewModel> Play { get; }

    public ShellViewModel(KarataClient karata)
    {
        Home = ReactiveCommand.CreateFromObservable(() => Router.NavigateAndReset.Execute(new HomeViewModel(this, karata)));
        Play = ReactiveCommand.CreateFromObservable(() => Router.NavigateAndReset.Execute(new PlayViewModel(this)));
    }
}