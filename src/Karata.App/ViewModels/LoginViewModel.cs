using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Karata.App.Services;
using Karata.Shared.Client;

namespace Karata.App.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private bool _busy;
    
    public override string UrlPathSegment => "login";
    public override IScreen HostScreen { get; }

    private readonly AuthService _auth;

    public ReactiveCommand<Unit, Unit> Login { get; }
    public ReactiveCommand<Unit, Unit> Logout { get; }
    
    public bool Busy
    {
        get => _busy;
        private set => this.RaiseAndSetIfChanged(ref _busy, value);
    }

    public LoginViewModel(IScreen hostScreen, AuthService auth, KarataClient karata)
    {
        HostScreen = hostScreen;
        _auth = auth;

        Login = ReactiveCommand.CreateFromTask(async () =>
        {
            var ok = await _auth.LoginAsync();
            if (ok)
            {
                var home = new HomeViewModel(HostScreen, karata);
                await HostScreen.Router.NavigateAndReset.Execute(home);
            }/* resolve deps from DI if you prefer */
        });

        Logout = ReactiveCommand.CreateFromTask(() => _auth.LogoutAsync());
        Login.IsExecuting.Select(x => x).ToProperty(this, x => x.Busy);
    }
}