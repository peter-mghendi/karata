using Avalonia.Data.Converters;

namespace Karata.App.Support;

public static class StringConverters
{
    public static readonly IValueConverter FirstInitial =
        new FuncValueConverter<string?, string>(s => s is [var ch, ..] ? char.ToUpperInvariant(ch).ToString() : "?");
}
