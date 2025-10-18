using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Karata.Shared.Client;
using Karata.Shared.Models;
using ReactiveUI;

// If your ViewModelBase derives from ReactiveObject, that's fine—
// it doesn't require System.Reactive. Otherwise implement INotifyPropertyChanged.
namespace Karata.App.ViewModels;

public class MainViewModel(KarataClient karata) : ViewModelBase
{
    private ObservableCollection<ActivityData> _activity = new();
    private bool _loading;
    private string? _error;
    private bool _loadedOnce;

    public ObservableCollection<ActivityData> Activity
    {
        get => _activity;
        private set => this.RaiseAndSetIfChanged(ref _activity, value);
    }

    public bool Loading
    {
        get => _loading;
        private set => this.RaiseAndSetIfChanged(ref _loading, value);
    }

    public string? Error
    {
        get => _error;
        private set => this.RaiseAndSetIfChanged(ref _error, value);
    }
    
    public ICommand OpenLink { get; } = ReactiveCommand.Create<string>(url =>
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // optionally set Error = "Couldn't open link";
        }
    });
    
    public async Task LoadAsync()
    {
        if (_loadedOnce) return;
        _loadedOnce = true;

        Loading = true;
        Error = null;

        try
        {
            var items = await karata.Activity.ListAsync();

            Activity.Clear();
            foreach (var it in items)
                Activity.Add(it);
        }
        catch (System.Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            Loading = false;
        }
    }
}