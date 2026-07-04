namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the system uptime in number of seconds.
    /// </summary>
    /// <returns>The system uptime in seconds.</returns>
    public static long uptime()
    {
        return Environment.TickCount64 / 1000;
    }
}
