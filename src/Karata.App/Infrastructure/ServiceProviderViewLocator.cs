using System;
using ReactiveUI;

namespace Karata.App.Infrastructure;

public sealed class ServiceProviderViewLocator(IServiceProvider sp) : IViewLocator
{
    public IViewFor? ResolveView<T>(T? vm, string? contract = null)
        => (IViewFor?)sp.GetService(typeof(IViewFor<>).MakeGenericType(vm!.GetType()));
}