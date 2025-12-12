using System.Reactive;
using System.Reactive.Linq;
using Karata.App.Services;
using Karata.Kit.Application.Client;
using ReactiveUI;

namespace Karata.App.ViewModels;

public class ShellViewModel : ReactiveObject, IScreen
{
    private readonly AuthService _auth;
    
    public RoutingState Router { get; } = new();
    
    public string? Username =>
        _auth.IsAuthenticated
            ? _auth.AccessToken != null
                ? _auth.User?.Identity?.Name ?? _auth.User?.FindFirst("preferred_username")?.Value
                : null
            : null;
    
    public ReactiveCommand<Unit, IRoutableViewModel> Back => Router.NavigateBack;

    public ReactiveCommand<Unit, IRoutableViewModel> Home { get; }

    public ReactiveCommand<Unit, IRoutableViewModel> Play { get; }
    
    public ReactiveCommand<Unit, Unit> Logout { get; }

    public ShellViewModel(KarataClient karata, AuthService auth)
    {
        _auth = auth;
        _auth.StateChanged += (_, _) => this.RaisePropertyChanged(nameof(Username));
        
        Home = ReactiveCommand.CreateFromObservable(() => Router.NavigateAndReset.Execute(new HomeViewModel(this, karata)));
        Play = ReactiveCommand.CreateFromObservable(() => Router.NavigateAndReset.Execute(new PlayViewModel(this)));
        Logout = ReactiveCommand.CreateFromTask(async () =>
        {
            await _auth.LogoutAsync();
            var login = new LoginViewModel(this, _auth, karata);
            await Router.NavigateAndReset.Execute(login);
        });
    }
}