using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Karata.App.ViewModels;
using ReactiveUI;

namespace Karata.App.Views;

public partial class PlayView : ReactiveUserControl<PlayViewModel>
{
    public PlayView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}