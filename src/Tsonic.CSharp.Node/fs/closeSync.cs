using System;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously closes a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    public static void closeSync(int fd)
    {
        if (!FileDescriptorManager.IsValid(fd))
            throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd));

        FileDescriptorManager.Unregister(fd);
    }
}
