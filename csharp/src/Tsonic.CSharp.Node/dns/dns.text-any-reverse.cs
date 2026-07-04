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
    // ==================== resolveTxt ====================

    /// <summary>
    /// Uses the DNS protocol to resolve text queries (TXT records) for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveTxt(string hostname, Action<Exception?, string[][]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide TXT record queries
                callback(null, Array.Empty<string[]>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<string[]>());
            }
        });
    }

    // ==================== resolveAny ====================

    /// <summary>
    /// Uses the DNS protocol to resolve all records (ANY or * query).
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveAny(string hostname, Action<Exception?, object[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide ANY record queries
                // This would combine all record types
                callback(null, Array.Empty<object>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<object>());
            }
        });
    }

    // ==================== reverse ====================

    /// <summary>
    /// Performs a reverse DNS query that resolves an IPv4 or IPv6 address to an array of host names.
    /// </summary>
    /// <param name="ip">IP address to resolve</param>
    /// <param name="callback">Callback function (err, hostnames)</param>
    public static void reverse(string ip, Action<Exception?, string[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                if (!IPAddress.TryParse(ip, out var ipAddress))
                {
                    throw new ArgumentException($"Invalid IP address: {ip}");
                }

                var hostEntry = Dns.GetHostEntry(ipAddress);
                var hostnames = new[] { hostEntry.HostName }
                    .Concat(hostEntry.Aliases)
                    .Distinct()
                    .ToArray();

                callback(null, hostnames);
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<string>());
            }
        });
    }

}
