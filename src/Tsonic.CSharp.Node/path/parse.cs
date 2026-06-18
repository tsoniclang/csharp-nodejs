using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Returns an object whose properties represent significant elements of the path.
    /// </summary>
    /// <param name="path">The path to parse.</param>
    /// <returns>A ParsedPath object containing path components.</returns>
    public static ParsedPath parse(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return new ParsedPath
            {
                root = string.Empty,
                dir = string.Empty,
                @base = string.Empty,
                ext = string.Empty,
                name = string.Empty
            };
        }

        var dir = Path.GetDirectoryName(path) ?? string.Empty;
        var basename = Path.GetFileName(path);
        var ext = Path.GetExtension(path);
        var name = basename;

        if (!string.IsNullOrEmpty(ext))
        {
            name = basename.Substring(0, basename.Length - ext.Length);
        }

        // Detect root (platform-specific)
        var root = string.Empty;
        if (Path.IsPathRooted(path))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: "C:\" or "\\"
                var pathRoot = Path.GetPathRoot(path) ?? string.Empty;
                root = pathRoot;
            }
            else
            {
                // Unix: "/"
                root = "/";
            }
        }

        return new ParsedPath
        {
            root = root,
            dir = dir,
            @base = basename,
            ext = ext,
            name = name
        };
    }
}

/// <summary>
/// Represents the components of a parsed path.
/// </summary>
public class ParsedPath
{
    /// <summary>The root of the path such as '/' or 'C:\'</summary>
    public string root { get; set; } = string.Empty;

    /// <summary>The full directory path such as '/home/user/dir' or 'c:\path\dir'</summary>
    public string dir { get; set; } = string.Empty;

    /// <summary>The file name including extension (if any) such as 'file.txt'</summary>
    public string @base { get; set; } = string.Empty;

    /// <summary>The file extension (if any) such as '.txt'</summary>
    public string ext { get; set; } = string.Empty;

    /// <summary>The file name without extension (if any) such as 'file'</summary>
    public string name { get; set; } = string.Empty;
}
