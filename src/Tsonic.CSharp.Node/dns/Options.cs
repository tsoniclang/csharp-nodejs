using System;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// Options for DNS lookup operations.
/// </summary>
public class LookupOptions
{
    /// <summary>
    /// The record family. Must be 4, 6, or 0.
    /// 0 indicates that either an IPv4 or IPv6 address is returned.
    /// </summary>
    public object? family { get; set; } // Can be number or "IPv4" or "IPv6"

    /// <summary>
    /// One or more supported getaddrinfo flags. Multiple flags may be passed by bitwise OR.
    /// </summary>
    public int? hints { get; set; }

    /// <summary>
    /// When true, the callback returns all resolved addresses in an array.
    /// </summary>
    public bool? all { get; set; }

    /// <summary>
    /// The order in which to return addresses: "ipv4first", "ipv6first", or "verbatim".
    /// </summary>
    public string? order { get; set; }

    /// <summary>
    /// When true, the callback receives IPv4 and IPv6 addresses in the order the DNS resolver returned them.
    /// Deprecated in favor of order.
    /// </summary>
    public bool? verbatim { get; set; }
}

/// <summary>
/// Address returned by DNS lookup.
/// </summary>
public class LookupAddress
{
    /// <summary>
    /// A string representation of an IPv4 or IPv6 address.
    /// </summary>
    public string address { get; set; } = string.Empty;

    /// <summary>
    /// 4 or 6, denoting the family of address.
    /// </summary>
    public int family { get; set; }
}

/// <summary>
/// Options for DNS resolve operations with TTL support.
/// </summary>
public class ResolveOptions
{
    /// <summary>
    /// When true, includes TTL (time-to-live) information in results.
    /// </summary>
    public bool ttl { get; set; }
}

/// <summary>
/// Options for creating a Resolver instance.
/// </summary>
public class ResolverOptions
{
    /// <summary>
    /// Query timeout in milliseconds, or -1 to use the default timeout.
    /// </summary>
    public int? timeout { get; set; }

    /// <summary>
    /// The number of tries the resolver will try contacting each name server before giving up.
    /// </summary>
    public int? tries { get; set; }

    /// <summary>
    /// The max retry timeout, in milliseconds.
    /// </summary>
    public int? maxTimeout { get; set; }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
