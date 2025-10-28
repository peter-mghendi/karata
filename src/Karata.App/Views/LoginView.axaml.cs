using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Karata.App.ViewModels;
using ReactiveUI;

namespace Karata.App.Views;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
    public LoginView()
    {
        this.WhenActivated(_ => { });
        AvaloniaXamlLoader.Load(this);
    }
}