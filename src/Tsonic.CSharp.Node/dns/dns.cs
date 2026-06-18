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
public static class dns
{
    private static readonly DnsPromises _promises = new();

    // ==================== Constants ====================

    /// <summary>
    /// Limits returned address types to the types of non-loopback addresses configured on the system.
    /// </summary>
    public const int ADDRCONFIG = 0x0400;

    /// <summary>
    /// If the IPv6 family was specified, but no IPv6 addresses were found,
    /// then return IPv4 mapped IPv6 addresses.
    /// </summary>
    public const int V4MAPPED = 0x0800;

    /// <summary>
    /// If dns.V4MAPPED is specified, return resolved IPv6 addresses as well as IPv4 mapped IPv6 addresses.
    /// </summary>
    public const int ALL = V4MAPPED | ADDRCONFIG;

    // Error codes
    /// <summary>DNS server returned answer with no data.</summary>
    public const string NODATA = "ENODATA";
    /// <summary>DNS server claims query was misformatted.</summary>
    public const string FORMERR = "EFORMERR";
    /// <summary>DNS server returned general failure.</summary>
    public const string SERVFAIL = "ESERVFAIL";
    /// <summary>Domain name not found.</summary>
    public const string NOTFOUND = "ENOTFOUND";
    /// <summary>DNS server does not implement requested operation.</summary>
    public const string NOTIMP = "ENOTIMP";
    /// <summary>DNS server refused query.</summary>
    public const string REFUSED = "EREFUSED";
    /// <summary>Misformatted DNS query.</summary>
    public const string BADQUERY = "EBADQUERY";
    /// <summary>Misformatted host name.</summary>
    public const string BADNAME = "EBADNAME";
    /// <summary>Unsupported address family.</summary>
    public const string BADFAMILY = "EBADFAMILY";
    /// <summary>Misformatted DNS reply.</summary>
    public const string BADRESP = "EBADRESP";
    /// <summary>Could not contact DNS servers.</summary>
    public const string CONNREFUSED = "ECONNREFUSED";
    /// <summary>Timeout while contacting DNS servers.</summary>
    public const string TIMEOUT = "ETIMEOUT";
    /// <summary>End of file.</summary>
    public const string EOF = "EOF";
    /// <summary>Error reading file.</summary>
    public const string FILE = "EFILE";
    /// <summary>Out of memory.</summary>
    public const string NOMEM = "ENOMEM";
    /// <summary>Channel is being destroyed.</summary>
    public const string DESTRUCTION = "EDESTRUCTION";
    /// <summary>Misformatted string.</summary>
    public const string BADSTR = "EBADSTR";
    /// <summary>Illegal flags specified.</summary>
    public const string BADFLAGS = "EBADFLAGS";
    /// <summary>Given host name is not numeric.</summary>
    public const string NONAME = "ENONAME";
    /// <summary>Illegal hints flags specified.</summary>
    public const string BADHINTS = "EBADHINTS";
    /// <summary>c-ares library initialization not yet performed.</summary>
    public const string NOTINITIALIZED = "ENOTINITIALIZED";
    /// <summary>Error loading iphlpapi.dll.</summary>
    public const string LOADIPHLPAPI = "ELOADIPHLPAPI";
    /// <summary>Could not find GetNetworkParams function.</summary>
    public const string ADDRGETNETWORKPARAMS = "EADDRGETNETWORKPARAMS";
    /// <summary>DNS query cancelled.</summary>
    public const string CANCELLED = "ECANCELLED";

    private static string _defaultResultOrder = "verbatim";

    /// <summary>
    /// Promise-based dns APIs.
    /// </summary>
    public static DnsPromises promises => _promises;

    // ==================== lookup ====================

    /// <summary>
    /// Resolves a host name into the first found A (IPv4) or AAAA (IPv6) record.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, address, family)</param>
    public static void lookup(string hostname, Action<Exception?, string, int> callback)
    {
        lookup(hostname, null, callback);
    }

    /// <summary>
    /// Resolves a host name into the first found A (IPv4) or AAAA (IPv6) record.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="family">The record family (4, 6, or 0)</param>
    /// <param name="callback">Callback function (err, address, family)</param>
    public static void lookup(string hostname, int family, Action<Exception?, string, int> callback)
    {
        var options = new LookupOptions { family = family };
        lookup(hostname, options, callback);
    }

    /// <summary>
    /// Resolves a host name into the first found A (IPv4) or AAAA (IPv6) record.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="options">Lookup options</param>
    /// <param name="callback">Callback function (err, address, family) or (err, addresses) if all=true</param>
    public static void lookup(string hostname, LookupOptions? options, Action<Exception?, string, int> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                var family = ParseFamily(options?.family);
                var addressFamily = family == 4 ? AddressFamily.InterNetwork :
                                  family == 6 ? AddressFamily.InterNetworkV6 :
                                  AddressFamily.Unspecified;

                var addresses = Dns.GetHostAddresses(hostname);

                if (addressFamily != AddressFamily.Unspecified)
                {
                    addresses = addresses.Where(a => a.AddressFamily == addressFamily).ToArray();
                }

                if (addresses.Length == 0)
                {
                    var ex = new Exception($"{NOTFOUND}: {hostname}");
                    callback(ex, string.Empty, 0);
                    return;
                }

                var address = addresses[0];
                callback(null, address.ToString(), address.AddressFamily == AddressFamily.InterNetwork ? 4 : 6);
            }
            catch (Exception ex)
            {
                callback(ex, string.Empty, 0);
            }
        });
    }

    /// <summary>
    /// Resolves a host name and returns all addresses when options.all is true.
    /// </summary>
    public static void lookup(string hostname, LookupOptions? options, Action<Exception?, LookupAddress[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                var family = ParseFamily(options?.family);
                var addressFamily = family == 4 ? AddressFamily.InterNetwork :
                                  family == 6 ? AddressFamily.InterNetworkV6 :
                                  AddressFamily.Unspecified;

                var addresses = Dns.GetHostAddresses(hostname);

                if (addressFamily != AddressFamily.Unspecified)
                {
                    addresses = addresses.Where(a => a.AddressFamily == addressFamily).ToArray();
                }

                var results = addresses.Select(a => new LookupAddress
                {
                    address = a.ToString(),
                    family = a.AddressFamily == AddressFamily.InterNetwork ? 4 : 6
                }).ToArray();

                // Apply ordering
                results = ApplyAddressOrdering(results, options);

                callback(null, results);
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<LookupAddress>());
            }
        });
    }

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

    // ==================== resolveCaa ====================

    /// <summary>
    /// Uses the DNS protocol to resolve CAA records for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, records)</param>
    public static void resolveCaa(string hostname, Action<Exception?, CaaRecord[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide CAA record queries
                // This is a stub implementation
                callback(null, Array.Empty<CaaRecord>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<CaaRecord>());
            }
        });
    }

    // ==================== resolveMx ====================

    /// <summary>
    /// Uses the DNS protocol to resolve mail exchange records (MX records) for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveMx(string hostname, Action<Exception?, MxRecord[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide MX record queries directly
                // This is a stub implementation
                callback(null, Array.Empty<MxRecord>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<MxRecord>());
            }
        });
    }

    // ==================== resolveNaptr ====================

    /// <summary>
    /// Uses the DNS protocol to resolve regular expression-based records (NAPTR records).
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveNaptr(string hostname, Action<Exception?, NaptrRecord[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide NAPTR record queries
                callback(null, Array.Empty<NaptrRecord>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<NaptrRecord>());
            }
        });
    }

    // ==================== resolveNs ====================

    /// <summary>
    /// Uses the DNS protocol to resolve name server records (NS records) for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveNs(string hostname, Action<Exception?, string[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide NS record queries
                callback(null, Array.Empty<string>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<string>());
            }
        });
    }

    // ==================== resolvePtr ====================

    /// <summary>
    /// Uses the DNS protocol to resolve pointer records (PTR records) for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolvePtr(string hostname, Action<Exception?, string[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide PTR record queries directly
                // This would use reverse DNS
                callback(null, Array.Empty<string>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<string>());
            }
        });
    }

    // ==================== resolveSoa ====================

    /// <summary>
    /// Uses the DNS protocol to resolve a start of authority record (SOA record).
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, address)</param>
    public static void resolveSoa(string hostname, Action<Exception?, SoaRecord> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide SOA record queries
                callback(new Exception("SOA records not supported"), new SoaRecord());
            }
            catch (Exception ex)
            {
                callback(ex, new SoaRecord());
            }
        });
    }

    // ==================== resolveSrv ====================

    /// <summary>
    /// Uses the DNS protocol to resolve service records (SRV records) for the hostname.
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveSrv(string hostname, Action<Exception?, SrvRecord[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide SRV record queries
                callback(null, Array.Empty<SrvRecord>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<SrvRecord>());
            }
        });
    }

    // ==================== resolveTlsa ====================

    /// <summary>
    /// Uses the DNS protocol to resolve certificate associations (TLSA records).
    /// </summary>
    /// <param name="hostname">Host name to resolve</param>
    /// <param name="callback">Callback function (err, addresses)</param>
    public static void resolveTlsa(string hostname, Action<Exception?, TlsaRecord[]> callback)
    {
        BackgroundDispatch.Run(() =>
        {
            try
            {
                // Note: .NET doesn't provide TLSA record queries
                callback(null, Array.Empty<TlsaRecord>());
            }
            catch (Exception ex)
            {
                callback(ex, Array.Empty<TlsaRecord>());
            }
        });
    }

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
        // Note: .NET doesn't support changing DNS servers programmatically
        // This is a stub implementation
    }

    /// <summary>
    /// Returns an array of IP address strings currently configured for DNS resolution.
    /// </summary>
    /// <returns>Array of DNS server addresses</returns>
    public static string[] getServers()
    {
        // Note: .NET doesn't provide API to get DNS servers
        // This is a stub implementation
        return Array.Empty<string>();
    }

    // ==================== Helper Methods ====================

    private static int ParseFamily(object? family)
    {
        if (family == null)
            return 0;

        if (family is int intFamily)
            return intFamily;

        if (family is string strFamily)
        {
            return strFamily.ToLowerInvariant() switch
            {
                "ipv4" => 4,
                "ipv6" => 6,
                _ => 0
            };
        }

        return 0;
    }

    private static LookupAddress[] ApplyAddressOrdering(LookupAddress[] addresses, LookupOptions? options)
    {
        var order = options?.order ?? _defaultResultOrder;

        if (order == "verbatim")
        {
            return addresses;
        }
        else if (order == "ipv4first")
        {
            return addresses.OrderBy(a => a.family == 4 ? 0 : 1).ToArray();
        }
        else if (order == "ipv6first")
        {
            return addresses.OrderBy(a => a.family == 6 ? 0 : 1).ToArray();
        }

        return addresses;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
