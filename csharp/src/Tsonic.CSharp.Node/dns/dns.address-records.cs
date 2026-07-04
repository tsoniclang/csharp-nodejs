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
    // ==================== resolve4 ====================

    /// <summary>
    /// Uses the DNS protocol to resolve IPv4 addresses (A records) for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolve4(string hostname, Action<Exception?, string[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                var addresses = Dns.GetHostAddresses(hostname)
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.ToString())
                    .ToArray();
                callback(null, addresses);
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<string>());
            }
        });
    }

    /// <summary>
    /// Uses the DNS protocol to resolve IPv4 addresses with TTL information.
    /// </summary>
    public static void resolve4(string hostname, ResolveOptions options, Action<Exception?, object> callback)
    {
        if (options.ttl)
        {
            // Note: .NET DNS doesn't provide TTL info, so we return default TTL
            BackgroundDispatch.Run(() =>
            {
                try
                {
                    var addresses = Dns.GetHostAddresses(hostname)
                        .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                        .Select(a => new RecordWithTtl { address = a.ToString(), ttl = 0 })
                        .ToArray();
                    callback(null, addresses);
                }
                catch (Exception ex)
                {
                    callback(ex, Array.Empty<RecordWithTtl>());
                }
            });
        }
        else
        {
            resolve4(hostname, (err, addresses) => callback(err, addresses ?? Array.Empty<string>()));
        }
    }

    // ==================== resolve6 ====================

    /// <summary>
    /// Uses the DNS protocol to resolve IPv6 addresses (AAAA records) for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolve6(string hostname, Action<Exception?, string[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                var addresses = Dns.GetHostAddresses(hostname)
                    .Where(a => a.AddressFamily == AddressFamily.InterNetworkV6)
                    .Select(a => a.ToString())
                    .ToArray();
                callback(null, addresses);
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<string>());
            }
        });
    }

    /// <summary>
    /// Uses the DNS protocol to resolve IPv6 addresses with TTL information.
    /// </summary>
    public static void resolve6(string hostname, ResolveOptions options, Action<Exception?, object> callback)
    {
        if (options.ttl)
        {
            BackgroundDispatch.Run(() =>
            {
                try
                {
                    var addresses = Dns.GetHostAddresses(hostname)
                        .Where(a => a.AddressFamily == AddressFamily.InterNetworkV6)
                        .Select(a => new RecordWithTtl { address = a.ToString(), ttl = 0 })
                        .ToArray();
                    callback(null, addresses);
                }
                catch (Exception ex)
                {
                    callback(ex, Array.Empty<RecordWithTtl>());
                }
            });
        }
        else
        {
            resolve6(hostname, (err, addresses) => callback(err, addresses ?? Array.Empty<string>()));
        }
    }

    // ==================== resolveCname ====================

    /// <summary>
    /// Uses the DNS protocol to resolve CNAME records for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveCname(string hostname, Action<Exception?, string[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide direct CNAME query API
                // This is a simplified implementation
                var hostEntry = Dns.GetHostEntry(hostname);
                var cnames = new[] { hostEntry.HostName };
                callback(null, cnames);
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<string>());
            }
        });
    }
}
