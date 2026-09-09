namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously removes files and directories (modern API).
    /// </summary>
    /// <param name="path">The path to remove.</param>
    public static void rmSync(string path)
    {
        rmSync(path, new RmOptions());
    }

    /// <summary>
    /// Synchronously removes a file or directory with the exact Node options contract.
    /// </summary>
    public static void rmSync(string path, RmOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var recursive = options.recursive ?? false;
        var force = options.force ?? false;
        var configuredMaxRetries = checked((uint)RequireNonNegativeInteger(
            options.maxRetries ?? 0,
            nameof(options.maxRetries),
            uint.MaxValue));
        var maxRetries = recursive ? configuredMaxRetries : 0U;
        var retryDelay = checked((int)RequireNonNegativeInteger(
            options.retryDelay ?? 100,
            nameof(options.retryDelay),
            int.MaxValue));
        for (uint attempt = 0; ; attempt++)
        {
            try
            {
                RemovePath(path, recursive, force);
                return;
            }
            catch (Exception exception) when (
                attempt < maxRetries &&
                IsRetryableRemovalError(exception))
            {
                var delay = checked((long)retryDelay * (attempt + 1L));
                if (delay > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(options.retryDelay),
                        "retryDelay multiplied by the retry count exceeds the supported timer range.");
                }
                if (delay > 0)
                {
                    Thread.Sleep((int)delay);
                }
            }
        }
    }

    private static void RemovePath(string path, bool recursive, bool force)
    {
        FileAttributes attributes;
        try
        {
            attributes = File.GetAttributes(path);
        }
        catch (Exception exception) when (
            force &&
            (exception is FileNotFoundException || exception is DirectoryNotFoundException))
        {
            return;
        }
        var symbolicLink = (attributes & FileAttributes.ReparsePoint) != 0;
        var directory = (attributes & FileAttributes.Directory) != 0;
        if (symbolicLink)
        {
            if (directory && OperatingSystem.IsWindows())
            {
                Directory.Delete(path, recursive: false);
            }
            else
            {
                File.Delete(path);
            }
            return;
        }
        if (!directory)
        {
            File.Delete(path);
            return;
        }
        if (!recursive)
        {
            throw new IOException($"Path is a directory and recursive removal was not requested: {path}");
        }
        Directory.Delete(path, recursive: true);
    }

    private static bool IsRetryableRemovalError(Exception exception)
    {
        if (exception is not IOException && exception is not UnauthorizedAccessException)
        {
            return false;
        }
        var nativeCode = exception.HResult & 0xFFFF;
        return OperatingSystem.IsWindows()
            ? nativeCode is 4 or 5 or 32 or 33 or 145
            : nativeCode is 1 or 16 or 23 or 24 or 39;
    }
}
