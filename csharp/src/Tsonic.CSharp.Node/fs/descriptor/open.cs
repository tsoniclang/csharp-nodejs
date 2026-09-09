using System;
using System.IO;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously opens a file and returns a file descriptor.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="flags">The file system flags.</param>
    /// <param name="mode">The file mode (permissions). Default is 0o666.</param>
    /// <returns>A Task that resolves to a file descriptor (integer).</returns>
    public static Task<int> open(string path, string flags, int? mode = null)
    {
        return Task.Run(() => openSync(path, flags, mode));
    }
}
