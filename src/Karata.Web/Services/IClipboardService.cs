#nullable enable

namespace Karata.Web.Services;

/// <summary>
/// Provides methods to place text on and retrieve text from the system clipboard.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Retrieves text data from the clipboard.
    /// </summary>
    Task<string> GetTextAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets text data to the clipboard.
    /// </summary>
    Task SetTextAsync(string text, CancellationToken cancellationToken = default);
}
