namespace Karata.Surface.Extensions;

public static class RoomCode
{
    public static Guid? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var text = input.Trim();
        if (Guid.TryParse(text, out var id)) return id;

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)) return null;

        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var game = Array.IndexOf(segments, "game");

        return game >= 0 && game + 1 < segments.Length && Guid.TryParse(segments[game + 1], out id) ? id : null;
    }
}