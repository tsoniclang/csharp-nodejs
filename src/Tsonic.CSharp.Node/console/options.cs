#pragma warning disable CS1591
#pragma warning disable IDE1006

using System.IO;

namespace Tsonic.CSharp.Node;

public enum ConsoleColorMode
{
    auto,
    always,
    never
}

public sealed class ConsoleOptions
{
    public TextWriter? stdout { get; set; }

    public TextWriter? stderr { get; set; }

    public bool ignoreErrors { get; set; } = true;

    public ConsoleColorMode colorMode { get; set; } = ConsoleColorMode.auto;
}
