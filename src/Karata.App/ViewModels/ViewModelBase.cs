using ReactiveUI;

namespace Karata.App.ViewModels;

public abstract class ViewModelBase : ReactiveObject, IRoutableViewModel
{
    public abstract string? UrlPathSegment { get; }
    public abstract IScreen HostScreen { get; }
}
