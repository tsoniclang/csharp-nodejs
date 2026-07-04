using System;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// An independent resolver for DNS requests.
/// Creating a new resolver uses the default server settings.
/// </summary>
public class Resolver
{
    private readonly ResolverOptions? _options;
    private bool _cancelled = false;
    private string? _localIpv4;
    private string? _localIpv6;

    /// <summary>
    /// Creates a new Resolver instance.
    /// </summary>
    public Resolver() : this(null)
    {
    }

    /// <summary>
    /// Creates a new Resolver instance with options.
    /// </summary>
    /// <param name="options">Resolver options</param>
    public Resolver(ResolverOptions? options)
    {
        _options = options;
    }

    /// <summary>
    /// Cancel all outstanding DNS queries made by this resolver.
    /// The corresponding callbacks will be called with an error with code ECANCELLED.
    /// </summary>
    public void cancel()
    {
        _cancelled = true;
    }

    /// <summary>
    /// Returns an array of IP address strings currently configured for DNS resolution.
    /// </summary>
    public string[] getServers()
    {
        return dns.getServers();
    }

    /// <summary>
    /// Sets the IP address and port of servers to be used when performing DNS resolution.
    /// </summary>
    public void setServers(string[] servers)
    {
        dns.setServers(servers);
    }

    /// <summary>
    /// The resolver instance will send its requests from the specified IP address.
    /// </summary>
    /// <param name="ipv4">A string representation of an IPv4 address</param>
    /// <param name="ipv6">A string representation of an IPv6 address</param>
    public void setLocalAddress(string? ipv4 = null, string? ipv6 = null)
    {
        _localIpv4 = ipv4;
        _localIpv6 = ipv6;
    }

    /// <summary>
    /// Uses the DNS protocol to resolve a host name.
    /// </summary>
    public void resolve(string hostname, Action<Exception?, string[]> callback)
    {
        CheckCancelled(callback, () => dns.resolve(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve a host name with specific record type.
    /// </summary>
    public void resolve(string hostname, string rrtype, Action<Exception?, object> callback)
    {
        CheckCancelled(callback, () => dns.resolve(hostname, rrtype, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve IPv4 addresses (A records).
    /// </summary>
    public void resolve4(string hostname, Action<Exception?, string[]> callback)
    {
        CheckCancelled(callback, () => dns.resolve4(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve IPv4 addresses with options.
    /// </summary>
    public void resolve4(string hostname, ResolveOptions options, Action<Exception?, object> callback)
    {
        CheckCancelled(callback, () => dns.resolve4(hostname, options, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve IPv6 addresses (AAAA records).
    /// </summary>
    public void resolve6(string hostname, Action<Exception?, string[]> callback)
    {
        CheckCancelled(callback, () => dns.resolve6(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve IPv6 addresses with options.
    /// </summary>
    public void resolve6(string hostname, ResolveOptions options, Action<Exception?, object> callback)
    {
        CheckCancelled(callback, () => dns.resolve6(hostname, options, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve all records (ANY query).
    /// </summary>
    public void resolveAny(string hostname, Action<Exception?, object[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveAny(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve CAA records.
    /// </summary>
    public void resolveCaa(string hostname, Action<Exception?, CaaRecord[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveCaa(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve CNAME records.
    /// </summary>
    public void resolveCname(string hostname, Action<Exception?, string[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveCname(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve MX records.
    /// </summary>
    public void resolveMx(string hostname, Action<Exception?, MxRecord[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveMx(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve NAPTR records.
    /// </summary>
    public void resolveNaptr(string hostname, Action<Exception?, NaptrRecord[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveNaptr(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve NS records.
    /// </summary>
    public void resolveNs(string hostname, Action<Exception?, string[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveNs(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve PTR records.
    /// </summary>
    public void resolvePtr(string hostname, Action<Exception?, string[]> callback)
    {
        CheckCancelled(callback, () => dns.resolvePtr(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve SOA record.
    /// </summary>
    public void resolveSoa(string hostname, Action<Exception?, SoaRecord> callback)
    {
        CheckCancelled(callback, () => dns.resolveSoa(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve SRV records.
    /// </summary>
    public void resolveSrv(string hostname, Action<Exception?, SrvRecord[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveSrv(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve TLSA records.
    /// </summary>
    public void resolveTlsa(string hostname, Action<Exception?, TlsaRecord[]> callback)
    {
        CheckCancelled(callback, () => dns.resolveTlsa(hostname, callback));
    }

    /// <summary>
    /// Uses the DNS protocol to resolve TXT records.
    /// </summary>
    public void resolveTxt(string hostname, Action<Exception?, string[][]> callback)
    {
        CheckCancelled(callback, () => dns.resolveTxt(hostname, callback));
    }

    /// <summary>
    /// Performs a reverse DNS query.
    /// </summary>
    public void reverse(string ip, Action<Exception?, string[]> callback)
    {
        CheckCancelled(callback, () => dns.reverse(ip, callback));
    }

    private void CheckCancelled<T>(Action<Exception?, T> callback, Action action)
    {
        if (_cancelled)
        {
            var ex = new Exception(dns.CANCELLED);
            callback(ex, default!);
        }
        else
        {
            action();
        }
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
