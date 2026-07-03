namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The process.cwd() method returns the current working directory of the Node.js process.
    /// </summary>
    /// <returns>The current working directory.</returns>
    public static string cwd()
    {
        return Directory.GetCurrentDirectory();
    }
}
