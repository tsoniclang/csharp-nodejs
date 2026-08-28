using System.Threading.Tasks;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

public static class dns_promises
{
    public static Task<LookupAddress> lookup(string hostname, LookupOptions? options = null) =>
        dns.promises.lookup(hostname, options);

    public static Task<JSArray<LookupAddress>> lookupAll(string hostname, LookupOptions? options = null) =>
        dns.promises.lookupAll(hostname, options);
}
