namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type/member names (Node.js surface)
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// Options for fs.mkdir()/fs.mkdirSync().
/// </summary>
public sealed class MakeDirectoryOptions
{
    /// <summary>
    /// When true, create parent directories recursively.
    /// </summary>
    public bool? recursive { get; set; }

    /// <summary>
    /// Directory mode (POSIX permissions). Applied on Unix when provided.
    /// </summary>
    public double? mode { get; set; }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
