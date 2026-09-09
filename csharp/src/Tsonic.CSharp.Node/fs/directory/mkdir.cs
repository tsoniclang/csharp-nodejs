using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously creates a directory.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <returns>A promise that resolves when the directory is created.</returns>
    public static Task mkdir(string path)
    {
        return Task.Run(() => mkdirSync(path));
    }

    /// <summary>
    /// Asynchronously creates a directory using options object semantics.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="options">mkdir options ({ recursive?, mode? }).</param>
    /// <returns>A promise that resolves when the directory is created.</returns>
    public static Task mkdir(string path, MakeDirectoryOptions options)
    {
        return Task.Run(() => mkdirSync(path, options));
    }
}
