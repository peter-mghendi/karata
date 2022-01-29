#nullable enable

namespace Karata.Web.Services;

using Microsoft.JSInterop;

/// <inheritdoc />
public class ClipboardService : IClipboardService
{
    private readonly IJSRuntime _jsRuntime;

    public ClipboardService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    /// <inheritdoc />
    public async Task<string> GetTextAsync(CancellationToken cancellationToken = default) =>
        await _jsRuntime.InvokeAsync<string>("navigator.clipboard.readText", cancellationToken);

    /// <inheritdoc />
    public async Task SetTextAsync(string text, CancellationToken cancellationToken = default) =>
        await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", cancellationToken, new object[] { text });
}