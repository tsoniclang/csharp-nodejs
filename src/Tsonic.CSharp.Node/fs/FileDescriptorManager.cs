using System;
using System.Collections.Generic;
using System.IO;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Internal helper class to manage file descriptors for fs operations.
/// Maps integer file descriptors to FileStream objects.
/// </summary>
internal static class FileDescriptorManager
{
    private static readonly Dictionary<int, FileStream> _openFiles = new();
    private static int _nextFd = 3; // Start at 3 (0,1,2 are stdin/stdout/stderr)
    private static readonly object _lock = new();

    /// <summary>
    /// Registers a FileStream and returns its file descriptor.
    /// </summary>
    public static int Register(FileStream stream)
    {
        lock (_lock)
        {
            var fd = _nextFd++;
            _openFiles[fd] = stream;
            return fd;
        }
    }

    /// <summary>
    /// Gets the FileStream associated with a file descriptor.
    /// </summary>
    public static FileStream? Get(int fd)
    {
        lock (_lock)
        {
            return _openFiles.TryGetValue(fd, out var stream) ? stream : null;
        }
    }

    /// <summary>
    /// Closes and unregisters a file descriptor.
    /// </summary>
    public static void Unregister(int fd)
    {
        lock (_lock)
        {
            if (_openFiles.TryGetValue(fd, out var stream))
            {
                stream.Dispose();
                _openFiles.Remove(fd);
            }
        }
    }

    /// <summary>
    /// Checks if a file descriptor is valid.
    /// </summary>
    public static bool IsValid(int fd)
    {
        lock (_lock)
        {
            return _openFiles.ContainsKey(fd);
        }
    }
}
