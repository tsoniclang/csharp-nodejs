#pragma warning disable CS1591
#pragma warning disable IDE1006

using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

public sealed class LegacyUrlObject
{
    public string? href { get; set; }

    public string? protocol { get; set; }

    public string? auth { get; set; }

    public string? host { get; set; }

    public string? hostname { get; set; }

    public string? port { get; set; }

    public string? pathname { get; set; }

    public string? search { get; set; }

    public object? query { get; set; }

    public string? hash { get; set; }

    public string? path { get; set; }

    public bool? slashes { get; set; }
}

public sealed class HttpOptions
{
    public string? protocol { get; set; }

    public string? hostname { get; set; }

    public string? host { get; set; }

    public int? port { get; set; }

    public string? path { get; set; }

    public string? href { get; set; }

    public string? auth { get; set; }
}

public sealed class URLFormatOptions
{
    public bool auth { get; set; } = true;

    public bool fragment { get; set; } = true;

    public bool search { get; set; } = true;

    public bool unicode { get; set; }
}

public sealed class FileUrlToPathOptions
{
    public bool? windows { get; set; }
}

public sealed class PathToFileUrlOptions
{
    public bool? windows { get; set; }
}

public sealed class URLPatternInit
{
    public string? protocol { get; set; }

    public string? username { get; set; }

    public string? password { get; set; }

    public string? hostname { get; set; }

    public string? port { get; set; }

    public string? pathname { get; set; }

    public string? search { get; set; }

    public string? hash { get; set; }

    public string? baseURL { get; set; }
}

public sealed class URLPatternOptions
{
    public bool ignoreCase { get; set; }
}

public sealed class URLPatternComponentResult
{
    public string input { get; set; } = string.Empty;

    public Dictionary<string, string> groups { get; set; } = [];
}

public sealed class URLPatternResult
{
    public string[] inputs { get; set; } = [];

    public URLPatternComponentResult protocol { get; set; } = new();

    public URLPatternComponentResult username { get; set; } = new();

    public URLPatternComponentResult password { get; set; } = new();

    public URLPatternComponentResult hostname { get; set; } = new();

    public URLPatternComponentResult port { get; set; } = new();

    public URLPatternComponentResult pathname { get; set; } = new();

    public URLPatternComponentResult search { get; set; } = new();

    public URLPatternComponentResult hash { get; set; } = new();
}
