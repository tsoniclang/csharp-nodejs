using System.Text;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    // Helper to parse encoding strings
    private static Encoding ParseEncoding(string encoding)
    {
        return encoding.ToLowerInvariant() switch
        {
            "utf-8" or "utf8" => Utf8NoBom,
            "ascii" => Encoding.ASCII,
            "utf-16" or "utf16" => Encoding.Unicode,
            "utf-32" or "utf32" => Encoding.UTF32,
            _ => Utf8NoBom
        };
    }
}
