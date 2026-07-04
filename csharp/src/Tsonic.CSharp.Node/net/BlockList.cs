using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// The BlockList object can be used with some network APIs to specify rules for disabling inbound or outbound access to specific IP addresses, IP ranges, or IP subnets.
/// </summary>
public class BlockList
{
    private readonly HashSet<string> _blockedAddresses = new();
    private readonly List<(IPAddress start, IPAddress end, string type)> _blockedRanges = new();
    private readonly List<(IPAddress network, int prefix, string type)> _blockedSubnets = new();

    /// <summary>
    /// Creates a new BlockList instance.
    /// </summary>
    public BlockList()
    {
    }

    /// <summary>
    /// Adds a rule to block the given IP address.
    /// </summary>
    /// <param name="address">IP address to block</param>
    /// <param name="type">Type ("ipv4" or "ipv6")</param>
    public void addAddress(string address, string type = "ipv4")
    {
        _blockedAddresses.Add(address);
    }

    /// <summary>
    /// Adds a rule to block a range of IP addresses from start (inclusive) to end (inclusive).
    /// </summary>
    /// <param name="start">Start IP address</param>
    /// <param name="end">End IP address</param>
    /// <param name="type">Type ("ipv4" or "ipv6")</param>
    public void addRange(string start, string end, string type = "ipv4")
    {
        if (IPAddress.TryParse(start, out var startAddr) && IPAddress.TryParse(end, out var endAddr))
        {
            _blockedRanges.Add((startAddr, endAddr, type));
        }
    }

    /// <summary>
    /// Adds a rule to block a range of IP addresses specified as a subnet mask.
    /// </summary>
    /// <param name="network">Network address</param>
    /// <param name="prefix">Prefix length</param>
    /// <param name="type">Type ("ipv4" or "ipv6")</param>
    public void addSubnet(string network, int prefix, string type = "ipv4")
    {
        if (IPAddress.TryParse(network, out var networkAddr))
        {
            _blockedSubnets.Add((networkAddr, prefix, type));
        }
    }

    /// <summary>
    /// Returns true if the given IP address matches any of the rules added to the BlockList.
    /// </summary>
    /// <param name="address">IP address to check</param>
    /// <param name="type">Type ("ipv4" or "ipv6")</param>
    /// <returns>True if blocked</returns>
    public bool check(string address, string type = "ipv4")
    {
        // Check exact match
        if (_blockedAddresses.Contains(address))
            return true;

        if (!IPAddress.TryParse(address, out var ipAddr))
            return false;

        // Check ranges
        foreach (var (start, end, rangeType) in _blockedRanges)
        {
            if (rangeType == type && IsInRange(ipAddr, start, end))
                return true;
        }

        // Check subnets
        foreach (var (network, prefix, subnetType) in _blockedSubnets)
        {
            if (subnetType == type && IsInSubnet(ipAddr, network, prefix))
                return true;
        }

        return false;
    }

    private static bool IsInRange(IPAddress addr, IPAddress start, IPAddress end)
    {
        var addrBytes = addr.GetAddressBytes();
        var startBytes = start.GetAddressBytes();
        var endBytes = end.GetAddressBytes();

        if (addrBytes.Length != startBytes.Length || addrBytes.Length != endBytes.Length)
            return false;

        for (int i = 0; i < addrBytes.Length; i++)
        {
            if (addrBytes[i] < startBytes[i] || addrBytes[i] > endBytes[i])
                return false;
        }

        return true;
    }

    private static bool IsInSubnet(IPAddress addr, IPAddress network, int prefix)
    {
        var addrBytes = addr.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();

        if (addrBytes.Length != networkBytes.Length)
            return false;

        int fullBytes = prefix / 8;
        int remainingBits = prefix % 8;

        // Check full bytes
        for (int i = 0; i < fullBytes; i++)
        {
            if (addrBytes[i] != networkBytes[i])
                return false;
        }

        // Check remaining bits
        if (remainingBits > 0)
        {
            byte mask = (byte)(0xFF << (8 - remainingBits));
            if ((addrBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns an array of rules added to the blocklist.
    /// </summary>
    /// <returns>Array of rule strings</returns>
    public string[] getRules()
    {
        var rules = new List<string>();
        rules.AddRange(_blockedAddresses);
        rules.AddRange(_blockedRanges.Select(r => $"{r.start}-{r.end}"));
        rules.AddRange(_blockedSubnets.Select(s => $"{s.network}/{s.prefix}"));
        return rules.ToArray();
    }
}

/// <summary>
/// Options for SocketAddress constructor.
/// </summary>
public class SocketAddressInitOptions
{
    /// <summary>
    /// IP address.
    /// </summary>
    public string? address { get; set; }

    /// <summary>
    /// IP family ("ipv4" or "ipv6").
    /// </summary>
    public string? family { get; set; }

    /// <summary>
    /// Flow label (IPv6 only).
    /// </summary>
    public int? flowlabel { get; set; }

    /// <summary>
    /// Port number.
    /// </summary>
    public int? port { get; set; }
}

/// <summary>
/// Represents a socket address.
/// </summary>
public class SocketAddress
{
    /// <summary>
    /// IP address.
    /// </summary>
    public string address { get; private set; }

    /// <summary>
    /// IP family ("ipv4" or "ipv6").
    /// </summary>
    public string family { get; private set; }

    /// <summary>
    /// Flow label (IPv6 only).
    /// </summary>
    public int? flowlabel { get; private set; }

    /// <summary>
    /// Port number.
    /// </summary>
    public int port { get; private set; }

    /// <summary>
    /// Creates a new SocketAddress instance.
    /// </summary>
    /// <param name="options">Socket address options</param>
    public SocketAddress(SocketAddressInitOptions options)
    {
        address = options.address ?? "0.0.0.0";
        family = options.family ?? "ipv4";
        flowlabel = options.flowlabel;
        port = options.port ?? 0;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
