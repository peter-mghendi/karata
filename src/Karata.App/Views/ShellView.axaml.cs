using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Karata.App.ViewModels;
using ReactiveUI;

namespace Karata.App.Views;

public partial class ShellView : ReactiveUserControl<ShellViewModel>
{
    public ShellView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}