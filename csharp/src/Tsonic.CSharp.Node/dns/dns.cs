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
    private static string[] _servers = Array.Empty<string>();

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
