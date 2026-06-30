using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// The dns module enables name resolution.
/// </summary>
public static partial class dns
{
    // ==================== Configuration Methods ====================

    /// <summary>
    /// Get the default value for order in dns.lookup().
    /// </summary>
    /// <returns>The default result order</returns>
    public static string getDefaultResultOrder()
    {
        return _defaultResultOrder;
    }

    /// <summary>
    /// Set the default value of order in dns.lookup().
    /// </summary>
    /// <param name="order">Must be 'ipv4first', 'ipv6first' or 'verbatim'</param>
    public static void setDefaultResultOrder(string order)
    {
        if (order != "ipv4first" && order != "ipv6first" && order != "verbatim")
        {
            throw new ArgumentException($"Invalid order value: {order}. Must be 'ipv4first', 'ipv6first' or 'verbatim'");
        }
        _defaultResultOrder = order;
    }

    /// <summary>
    /// Sets the IP address and port of servers to be used when performing DNS resolution.
    /// </summary>
    /// <param name="servers">Array of RFC 5952 formatted addresses</param>
    public static void setServers(string[] servers)
    {
        _servers = servers.ToArray();
    }

    /// <summary>
    /// Returns an array of IP address strings currently configured for DNS resolution.
    /// </summary>
    /// <returns>Array of DNS server addresses</returns>
    public static string[] getServers()
    {
        return _servers.ToArray();
    }

}
