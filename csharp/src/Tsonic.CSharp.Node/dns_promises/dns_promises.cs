using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

#pragma warning disable IDE1006

public static class dns_promises
{
    public static Task<LookupAddress> lookup(string hostname, LookupOptions? options = null) => dns.promises.lookup(hostname, options);
    public static Task<LookupAddress[]> lookupAll(string hostname, LookupOptions? options = null) => dns.promises.lookupAll(hostname, options);
    public static Task<LookupServiceResult> lookupService(string address, int port) => dns.promises.lookupService(address, port);
    public static Task<string[]> resolve(string hostname) => dns.promises.resolve(hostname);
    public static Task<object> resolve(string hostname, string rrtype) => dns.promises.resolve(hostname, rrtype);
    public static Task<string[]> resolve4(string hostname) => dns.promises.resolve4(hostname);
    public static Task<object> resolve4(string hostname, ResolveOptions options) => dns.promises.resolve4(hostname, options);
    public static Task<string[]> resolve6(string hostname) => dns.promises.resolve6(hostname);
    public static Task<object> resolve6(string hostname, ResolveOptions options) => dns.promises.resolve6(hostname, options);
    public static Task<string[]> resolveCname(string hostname) => dns.promises.resolveCname(hostname);
    public static Task<CaaRecord[]> resolveCaa(string hostname) => dns.promises.resolveCaa(hostname);
    public static Task<MxRecord[]> resolveMx(string hostname) => dns.promises.resolveMx(hostname);
    public static Task<NaptrRecord[]> resolveNaptr(string hostname) => dns.promises.resolveNaptr(hostname);
    public static Task<string[]> resolveNs(string hostname) => dns.promises.resolveNs(hostname);
    public static Task<string[]> resolvePtr(string hostname) => dns.promises.resolvePtr(hostname);
    public static Task<SoaRecord> resolveSoa(string hostname) => dns.promises.resolveSoa(hostname);
    public static Task<SrvRecord[]> resolveSrv(string hostname) => dns.promises.resolveSrv(hostname);
    public static Task<TlsaRecord[]> resolveTlsa(string hostname) => dns.promises.resolveTlsa(hostname);
    public static Task<string[][]> resolveTxt(string hostname) => dns.promises.resolveTxt(hostname);
    public static Task<object[]> resolveAny(string hostname) => dns.promises.resolveAny(hostname);
    public static Task<string[]> reverse(string ip) => dns.promises.reverse(ip);
}
