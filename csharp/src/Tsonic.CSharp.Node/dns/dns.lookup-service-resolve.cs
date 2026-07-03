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
    // ==================== lookupService ====================

    /// <summary>
    /// Resolves the given address and port into a host name and service.
    /// </summary>
    /// <param name="address">IP address</param>
    /// <param name="port">Port number</param>
    /// <param name="callback">Callback function (err, hostname, service)</param>
    public static void lookupService(string address, int port, Action<Exception?, string, string> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                if (!IPAddress.TryParse(address, out var ipAddress))
                {
                    throw new ArgumentException($"Invalid IP address: {address}");
                }

                if (port < 0 || port > 65535)
                {
                    throw new ArgumentException($"Invalid port: {port}");
                }

                var hostEntry = Dns.GetHostEntry(ipAddress);
                var hostname = hostEntry.HostName;

                // Get service name (simplified - in real Node.js this uses getservbyport)
                var service = port.ToString();

                callback(null, hostname, service);
            }
            catch (Exception ex)
            {
                callback(ex, string.Empty, string.Empty);
            }
        });
    }

    // ==================== resolve ====================

    /// <summary>
    /// Uses the DNS protocol to resolve a host name into an array of resource records.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolve(string hostname, Action<Exception?, string[]> callback)
    {
        resolve4(hostname, callback);
    }

    /// <summary>
    /// Uses the DNS protocol to resolve a host name into an array of resource records.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="rrtype">Resource record type</param>
    /// <param name="callback">Callback function (err, records)</param>
    public static void resolve(string hostname, string rrtype, Action<Exception?, object> callback)
    {
        switch (rrtype.ToUpperInvariant())
        {
            case "A":
            case "AAAA":
            case "CNAME":
            case "NS":
            case "PTR":
                ResolveStringArray(hostname, rrtype, callback);
                break;
            case "MX":
                resolveMx(hostname, (err, records) => callback(err, records ?? Array.Empty<MxRecord>()));
                break;
            case "TXT":
                resolveTxt(hostname, (err, records) => callback(err, records ?? Array.Empty<string[]>()));
                break;
            case "SRV":
                resolveSrv(hostname, (err, records) => callback(err, records ?? Array.Empty<SrvRecord>()));
                break;
            case "NAPTR":
                resolveNaptr(hostname, (err, records) => callback(err, records ?? Array.Empty<NaptrRecord>()));
                break;
            case "SOA":
                resolveSoa(hostname, (err, record) => callback(err, record ?? new SoaRecord()));
                break;
            case "CAA":
                resolveCaa(hostname, (err, records) => callback(err, records ?? Array.Empty<CaaRecord>()));
                break;
            case "TLSA":
                resolveTlsa(hostname, (err, records) => callback(err, records ?? Array.Empty<TlsaRecord>()));
                break;
            default:
                callback(new ArgumentException($"Unknown rrtype: {rrtype}"), Array.Empty<string>());
                break;
        }
    }

    private static void ResolveStringArray(string hostname, string rrtype, Action<Exception?, object> callback)
    {
        switch (rrtype.ToUpperInvariant())
        {
            case "A":
                resolve4(hostname, (err, addresses) => callback(err, addresses ?? Array.Empty<string>()));
                break;
            case "AAAA":
                resolve6(hostname, (err, addresses) => callback(err, addresses ?? Array.Empty<string>()));
                break;
            case "CNAME":
                resolveCname(hostname, (err, addresses) => callback(err, addresses ?? Array.Empty<string>()));
                break;
            case "NS":
                resolveNs(hostname, (err, addresses) => callback(err, addresses ?? Array.Empty<string>()));
                break;
            case "PTR":
                resolvePtr(hostname, (err, addresses) => callback(err, addresses ?? Array.Empty<string>()));
                break;
        }
    }

}
