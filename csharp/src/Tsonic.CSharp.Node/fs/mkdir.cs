using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously creates a directory.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="recursive">If true, creates parent directories as needed (default: false).</param>
    /// <returns>A promise that resolves when the directory is created.</returns>
    public static Task mkdir(string path, bool recursive = false)
    {
        return Task.Run(() =>
        {
            if (recursive)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                // Non-recursive: fail if parent doesn't exist
                var parent = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                {
                    throw new DirectoryNotFoundException($"Parent directory does not exist: {parent}");
                }
                Directory.CreateDirectory(path);
            }
        });
    }

    /// <summary>
    /// Asynchronously creates a directory using options object semantics.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="options">mkdir options ({ recursive?, mode? }).</param>
    /// <returns>A promise that resolves when the directory is created.</returns>
    public static Task mkdir(string path, MkdirOptions? options)
    {
        return Task.Run(() => mkdirSync(path, (object?)options));
    }

    /// <summary>
    /// Asynchronously creates a directory using object options semantics.
    /// Accepts either a boolean recursive flag or an options object with
    /// { recursive?, mode? }.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="options">A recursive flag or options object.</param>
    /// <returns>A promise that resolves when the directory is created.</returns>
    public static Task mkdir(string path, object? options)
    {
        return Task.Run(() => mkdirSync(path, options));
    }
}
