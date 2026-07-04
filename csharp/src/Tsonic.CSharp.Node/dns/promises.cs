using System;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

/// <summary>
/// Promise-based wrappers over dns callback APIs.
/// </summary>
public class DnsPromises
{
    public Task<LookupAddress> lookup(string hostname, LookupOptions? options = null)
    {
        var tcs = new TaskCompletionSource<LookupAddress>();
        dns.lookup(hostname, options, (Exception? err, string address, int family) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(new LookupAddress { address = address, family = family });
        });
        return tcs.Task;
    }

    public Task<LookupAddress[]> lookupAll(string hostname, LookupOptions? options = null)
    {
        var lookupOptions = options ?? new LookupOptions();
        lookupOptions.all = true;

        var tcs = new TaskCompletionSource<LookupAddress[]>();
        dns.lookup(hostname, lookupOptions, (Exception? err, LookupAddress[] addresses) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(addresses);
        });
        return tcs.Task;
    }

    public Task<LookupServiceResult> lookupService(string address, int port)
    {
        var tcs = new TaskCompletionSource<LookupServiceResult>();
        dns.lookupService(address, port, (err, hostname, service) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(new LookupServiceResult { hostname = hostname, service = service });
        });
        return tcs.Task;
    }

    public Task<string[]> resolve(string hostname)
    {
        var tcs = new TaskCompletionSource<string[]>();
        dns.resolve(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<object> resolve(string hostname, string rrtype)
    {
        var tcs = new TaskCompletionSource<object>();
        dns.resolve(hostname, rrtype, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<string[]> resolve4(string hostname)
    {
        var tcs = new TaskCompletionSource<string[]>();
        dns.resolve4(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<object> resolve4(string hostname, ResolveOptions options)
    {
        var tcs = new TaskCompletionSource<object>();
        dns.resolve4(hostname, options, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<string[]> resolve6(string hostname)
    {
        var tcs = new TaskCompletionSource<string[]>();
        dns.resolve6(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<object> resolve6(string hostname, ResolveOptions options)
    {
        var tcs = new TaskCompletionSource<object>();
        dns.resolve6(hostname, options, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<string[]> resolveCname(string hostname)
    {
        var tcs = new TaskCompletionSource<string[]>();
        dns.resolveCname(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<CaaRecord[]> resolveCaa(string hostname)
    {
        var tcs = new TaskCompletionSource<CaaRecord[]>();
        dns.resolveCaa(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<MxRecord[]> resolveMx(string hostname)
    {
        var tcs = new TaskCompletionSource<MxRecord[]>();
        dns.resolveMx(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<NaptrRecord[]> resolveNaptr(string hostname)
    {
        var tcs = new TaskCompletionSource<NaptrRecord[]>();
        dns.resolveNaptr(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<string[]> resolveNs(string hostname)
    {
        var tcs = new TaskCompletionSource<string[]>();
        dns.resolveNs(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<string[]> resolvePtr(string hostname)
    {
        var tcs = new TaskCompletionSource<string[]>();
        dns.resolvePtr(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<SoaRecord> resolveSoa(string hostname)
    {
        var tcs = new TaskCompletionSource<SoaRecord>();
        dns.resolveSoa(hostname, (err, record) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(record);
        });
        return tcs.Task;
    }

    public Task<SrvRecord[]> resolveSrv(string hostname)
    {
        var tcs = new TaskCompletionSource<SrvRecord[]>();
        dns.resolveSrv(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<TlsaRecord[]> resolveTlsa(string hostname)
    {
        var tcs = new TaskCompletionSource<TlsaRecord[]>();
        dns.resolveTlsa(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<string[][]> resolveTxt(string hostname)
    {
        var tcs = new TaskCompletionSource<string[][]>();
        dns.resolveTxt(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<object[]> resolveAny(string hostname)
    {
        var tcs = new TaskCompletionSource<object[]>();
        dns.resolveAny(hostname, (err, records) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(records);
        });
        return tcs.Task;
    }

    public Task<string[]> reverse(string ip)
    {
        var tcs = new TaskCompletionSource<string[]>();
        dns.reverse(ip, (err, hostnames) =>
        {
            if (err != null)
            {
                tcs.TrySetException(err);
                return;
            }

            tcs.TrySetResult(hostnames);
        });
        return tcs.Task;
    }

    public string getDefaultResultOrder() => dns.getDefaultResultOrder();
    public void setDefaultResultOrder(string order) => dns.setDefaultResultOrder(order);
    public string[] getServers() => dns.getServers();
    public void setServers(string[] servers) => dns.setServers(servers);
}

public class LookupServiceResult
{
    public string hostname { get; set; } = string.Empty;
    public string service { get; set; } = string.Empty;
}

#pragma warning restore CS1591
