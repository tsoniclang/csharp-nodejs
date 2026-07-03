namespace Tsonic.CSharp.Node;

/// <summary>
/// An object containing version strings of Node.js and its dependencies.
/// </summary>
public class ProcessVersions
{
    /// <summary>
    /// The Node.js version.
    /// </summary>
    public string node { get; set; } = "24.0.0";

    /// <summary>
    /// The V8 JavaScript engine version.
    /// </summary>
    public string v8 { get; set; } = "0.0.0";

    /// <summary>
    /// The .NET runtime version (Tsonic-specific).
    /// </summary>
    public string dotnet { get; set; } = Environment.Version.ToString();

    /// <summary>
    /// The Tsonic toolchain version (Tsonic-specific).
    /// </summary>
    public string tsonic { get; set; } = "0.0.34";
}

public static partial class process
{
    private static readonly ProcessVersions _versions = new ProcessVersions();

    /// <summary>
    /// An object containing the version strings of Node.js and its dependencies.
    /// </summary>
    public static ProcessVersions versions => _versions;
}
