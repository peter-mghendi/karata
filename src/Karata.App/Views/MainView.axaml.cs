using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Karata.App.ViewModels;

namespace Karata.App.Views;

public partial class MainView : UserControl
{
    private bool _called;

    public MainView()
    {
        InitializeComponent();

        AttachedToVisualTree += async (_, _) =>
        {
            if (_called) return;
            _called = true;

            if (DataContext is MainViewModel vm)
                await vm.LoadAsync();
        };
    }
}