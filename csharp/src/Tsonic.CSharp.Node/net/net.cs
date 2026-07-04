using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// The net module provides an asynchronous network API for creating stream-based TCP or IPC servers and clients.
/// </summary>
public static class net
{
    private static bool _defaultAutoSelectFamily = false;
    private static int _defaultAutoSelectFamilyAttemptTimeout = 250;

    // ==================== createServer ====================

    /// <summary>
    /// Creates a new TCP or IPC server.
    /// </summary>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>A new Server instance</returns>
    public static Server createServer(Action<Socket>? connectionListener = null)
    {
        return new Server(connectionListener);
    }

    /// <summary>
    /// Creates a new TCP or IPC server with options.
    /// </summary>
    /// <param name="options">Server options</param>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>A new Server instance</returns>
    public static Server createServer(ServerOpts options, Action<Socket>? connectionListener = null)
    {
        return new Server(options, connectionListener);
    }

    // ==================== connect / createConnection ====================

    /// <summary>
    /// Creates a new socket connection.
    /// </summary>
    /// <param name="port">Port to connect to</param>
    /// <param name="host">Host to connect to</param>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>A new Socket instance</returns>
    public static Socket connect(int port, string? host = null, Action? connectionListener = null)
    {
        var socket = new Socket();
        socket.connect(port, host, connectionListener);
        return socket;
    }

    /// <summary>
    /// Creates a new socket connection with options.
    /// </summary>
    /// <param name="options">Connection options</param>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>A new Socket instance</returns>
    public static Socket connect(TcpSocketConnectOpts options, Action? connectionListener = null)
    {
        var socket = new Socket();
        socket.connect(options, connectionListener);
        return socket;
    }

    /// <summary>
    /// Creates a new socket connection with normalized connection options.
    /// </summary>
    public static Socket connect(ConnectOptions options, Action? connectionListener = null)
    {
        return connect((TcpSocketConnectOpts)options, connectionListener);
    }

    /// <summary>
    /// Creates a new IPC socket connection.
    /// </summary>
    /// <param name="path">Path to connect to</param>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>A new Socket instance</returns>
    public static Socket connect(string path, Action? connectionListener = null)
    {
        var socket = new Socket();
        socket.connect(path, connectionListener);
        return socket;
    }

    /// <summary>
    /// Alias for connect().
    /// </summary>
    public static Socket createConnection(int port, string? host = null, Action? connectionListener = null)
    {
        return connect(port, host, connectionListener);
    }

    /// <summary>
    /// Alias for connect() with options.
    /// </summary>
    public static Socket createConnection(TcpSocketConnectOpts options, Action? connectionListener = null)
    {
        return connect(options, connectionListener);
    }

    /// <summary>
    /// Alias for connect() with normalized connection options.
    /// </summary>
    public static Socket createConnection(ConnectOptions options, Action? connectionListener = null)
    {
        return connect(options, connectionListener);
    }

    /// <summary>
    /// Returns a closed endpoint carrier for a resolved address.
    /// </summary>
    public static LookupEndpoint lookupEndpoint(string address)
    {
        return new LookupEndpoint
        {
            address = address,
            family = isIP(address)
        };
    }

    /// <summary>
    /// Alias for connect() with path.
    /// </summary>
    public static Socket createConnection(string path, Action? connectionListener = null)
    {
        return connect(path, connectionListener);
    }

    // ==================== IP Utilities ====================

    /// <summary>
    /// Tests if input is an IP address. Returns 0 for invalid strings,
    /// returns 4 for IP version 4 addresses, and returns 6 for IP version 6 addresses.
    /// </summary>
    /// <param name="input">IP address string to test</param>
    /// <returns>0, 4, or 6</returns>
    public static int isIP(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        if (IPAddress.TryParse(input, out var addr))
        {
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return 4;
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return 6;
        }

        return 0;
    }

    /// <summary>
    /// Returns true if input is a version 4 IP address, otherwise returns false.
    /// </summary>
    /// <param name="input">IP address string to test</param>
    /// <returns>True if IPv4</returns>
    public static bool isIPv4(string input)
    {
        return isIP(input) == 4;
    }

    /// <summary>
    /// Returns true if input is a version 6 IP address, otherwise returns false.
    /// </summary>
    /// <param name="input">IP address string to test</param>
    /// <returns>True if IPv6</returns>
    public static bool isIPv6(string input)
    {
        return isIP(input) == 6;
    }

    // ==================== Auto-Select Family Configuration ====================

    /// <summary>
    /// Gets the default value of autoSelectFamily option of socket.connect(options).
    /// </summary>
    /// <returns>The default auto-select family value</returns>
    public static bool getDefaultAutoSelectFamily()
    {
        return _defaultAutoSelectFamily;
    }

    /// <summary>
    /// Sets the default value of autoSelectFamily option of socket.connect(options).
    /// </summary>
    /// <param name="value">The new default value</param>
    public static void setDefaultAutoSelectFamily(bool value)
    {
        _defaultAutoSelectFamily = value;
    }

    /// <summary>
    /// Gets the default value of autoSelectFamilyAttemptTimeout option of socket.connect(options).
    /// </summary>
    /// <returns>The default timeout in milliseconds</returns>
    public static int getDefaultAutoSelectFamilyAttemptTimeout()
    {
        return _defaultAutoSelectFamilyAttemptTimeout;
    }

    /// <summary>
    /// Sets the default value of autoSelectFamilyAttemptTimeout option of socket.connect(options).
    /// </summary>
    /// <param name="value">The new default timeout in milliseconds</param>
    public static void setDefaultAutoSelectFamilyAttemptTimeout(int value)
    {
        _defaultAutoSelectFamilyAttemptTimeout = value;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
