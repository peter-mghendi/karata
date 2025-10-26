using System;
using ReactiveUI;

namespace Karata.App.ViewModels;

public class PlayViewModel(IScreen screen) : ViewModelBase
{
    public override IScreen HostScreen { get; } = screen;

    public override string UrlPathSegment => "play";
}