using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace Karata.App.Support;

public static class StringConverters
{
    public static readonly IValueConverter FirstInitial =
        new FuncValueConverter<string?, string>(s =>
        {
            if (string.IsNullOrWhiteSpace(s)) return "?";
            var ch = char.ToUpperInvariant(s.Trim()[0]);
            return ch.ToString();
        });
}
