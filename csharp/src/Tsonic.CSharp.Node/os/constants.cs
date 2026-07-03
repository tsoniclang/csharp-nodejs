#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Tsonic.CSharp.Node;

public sealed class NetworkInterfaceInfo
{
    public string address { get; set; } = string.Empty;

    public string netmask { get; set; } = string.Empty;

    public string family { get; set; } = string.Empty;

    public string mac { get; set; } = string.Empty;

    public bool @internal { get; set; }

    public string? cidr { get; set; }

    public long? scopeid { get; set; }
}

public sealed class UserInfoOptions
{
    public string encoding { get; set; } = "utf8";
}

public sealed class PriorityConstants
{
    public int PRIORITY_LOW { get; init; } = 19;

    public int PRIORITY_BELOW_NORMAL { get; init; } = 10;

    public int PRIORITY_NORMAL { get; init; }

    public int PRIORITY_ABOVE_NORMAL { get; init; } = -7;

    public int PRIORITY_HIGH { get; init; } = -14;

    public int PRIORITY_HIGHEST { get; init; } = -20;
}

public sealed class DlopenConstants
{
    public int RTLD_LAZY { get; init; } = 1;

    public int RTLD_NOW { get; init; } = 2;

    public int RTLD_GLOBAL { get; init; } = 256;

    public int RTLD_LOCAL { get; init; }

    public int RTLD_DEEPBIND { get; init; } = 8;
}

public sealed class OsConstants
{
    public Dictionary<string, int> errno { get; init; } = new(StringComparer.Ordinal);

    public Dictionary<string, int> signals { get; init; } = new(StringComparer.Ordinal);

    public PriorityConstants priority { get; init; } = new();

    public DlopenConstants dlopen { get; init; } = new();

    public Dictionary<string, int> uv { get; init; } = new(StringComparer.Ordinal);
}

public static partial class os
{
    public static readonly OsConstants constants = CreateConstants();

    public static Dictionary<string, NetworkInterfaceInfo[]> networkInterfaces()
    {
        var result = new Dictionary<string, NetworkInterfaceInfo[]>(StringComparer.Ordinal);
        foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            var properties = networkInterface.GetIPProperties();
            var entries = new List<NetworkInterfaceInfo>();
            foreach (var address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                    continue;

                entries.Add(new NetworkInterfaceInfo
                {
                    address = address.Address.ToString(),
                    family = address.Address.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6",
                    netmask = address.IPv4Mask?.ToString() ?? string.Empty,
                    mac = FormatMacAddress(networkInterface.GetPhysicalAddress()),
                    @internal = IPAddressIsLoopback(address.Address),
                    cidr = BuildCidr(address),
                    scopeid = address.Address.AddressFamily == AddressFamily.InterNetworkV6 ? address.Address.ScopeId : null
                });
            }

            if (entries.Count > 0)
                result[networkInterface.Name] = entries.ToArray();
        }

        return result;
    }

    public static int errnoConstant(string name) => constants.errno[name];

    public static int signalConstant(string name) => constants.signals[name];

    public static int priorityConstant(string name)
    {
        return name switch
        {
            "PRIORITY_LOW" => constants.priority.PRIORITY_LOW,
            "PRIORITY_BELOW_NORMAL" => constants.priority.PRIORITY_BELOW_NORMAL,
            "PRIORITY_NORMAL" => constants.priority.PRIORITY_NORMAL,
            "PRIORITY_ABOVE_NORMAL" => constants.priority.PRIORITY_ABOVE_NORMAL,
            "PRIORITY_HIGH" => constants.priority.PRIORITY_HIGH,
            "PRIORITY_HIGHEST" => constants.priority.PRIORITY_HIGHEST,
            _ => throw new KeyNotFoundException(name)
        };
    }

    public static int dlopenConstant(string name)
    {
        return name switch
        {
            "RTLD_LAZY" => constants.dlopen.RTLD_LAZY,
            "RTLD_NOW" => constants.dlopen.RTLD_NOW,
            "RTLD_GLOBAL" => constants.dlopen.RTLD_GLOBAL,
            "RTLD_LOCAL" => constants.dlopen.RTLD_LOCAL,
            "RTLD_DEEPBIND" => constants.dlopen.RTLD_DEEPBIND,
            _ => throw new KeyNotFoundException(name)
        };
    }

    public static int uvConstant(string name) => constants.uv[name];

    private static OsConstants CreateConstants()
    {
        return new OsConstants
        {
            errno = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["E2BIG"] = 7, ["EACCES"] = 13, ["EADDRINUSE"] = 98, ["EADDRNOTAVAIL"] = 99,
                ["EAFNOSUPPORT"] = 97, ["EAGAIN"] = 11, ["EALREADY"] = 114, ["EBADF"] = 9,
                ["EBADMSG"] = 74, ["EBUSY"] = 16, ["ECANCELED"] = 125, ["ECHILD"] = 10,
                ["ECONNABORTED"] = 103, ["ECONNREFUSED"] = 111, ["ECONNRESET"] = 104,
                ["EDEADLK"] = 35, ["EDESTADDRREQ"] = 89, ["EDOM"] = 33, ["EDQUOT"] = 122,
                ["EEXIST"] = 17, ["EFAULT"] = 14, ["EFBIG"] = 27, ["EHOSTUNREACH"] = 113,
                ["EIDRM"] = 43, ["EILSEQ"] = 84, ["EINPROGRESS"] = 115, ["EINTR"] = 4,
                ["EINVAL"] = 22, ["EIO"] = 5, ["EISCONN"] = 106, ["EISDIR"] = 21,
                ["ELOOP"] = 40, ["EMFILE"] = 24, ["EMLINK"] = 31, ["EMSGSIZE"] = 90,
                ["EMULTIHOP"] = 72, ["ENAMETOOLONG"] = 36, ["ENETDOWN"] = 100,
                ["ENETRESET"] = 102, ["ENETUNREACH"] = 101, ["ENFILE"] = 23,
                ["ENOBUFS"] = 105, ["ENODATA"] = 61, ["ENODEV"] = 19, ["ENOENT"] = 2,
                ["ENOEXEC"] = 8, ["ENOLCK"] = 37, ["ENOLINK"] = 67, ["ENOMEM"] = 12,
                ["ENOMSG"] = 42, ["ENOPROTOOPT"] = 92, ["ENOSPC"] = 28, ["ENOSR"] = 63,
                ["ENOSTR"] = 60, ["ENOSYS"] = 38, ["ENOTCONN"] = 107, ["ENOTDIR"] = 20,
                ["ENOTEMPTY"] = 39, ["ENOTSOCK"] = 88, ["ENOTSUP"] = 95, ["ENOTTY"] = 25,
                ["ENXIO"] = 6, ["EOPNOTSUPP"] = 95, ["EOVERFLOW"] = 75, ["EPERM"] = 1,
                ["EPIPE"] = 32, ["EPROTO"] = 71, ["EPROTOTYPE"] = 91, ["ERANGE"] = 34,
                ["EROFS"] = 30, ["ESPIPE"] = 29, ["ESRCH"] = 3, ["ESTALE"] = 116,
                ["ETIME"] = 62, ["ETIMEDOUT"] = 110, ["ETXTBSY"] = 26, ["EXDEV"] = 18,
                ["EWOULDBLOCK"] = 11, ["WSAEACCES"] = 10013, ["WSAEADDRINUSE"] = 10048,
                ["WSAEADDRNOTAVAIL"] = 10049, ["WSAEAFNOSUPPORT"] = 10047,
                ["WSAECONNABORTED"] = 10053, ["WSAECONNREFUSED"] = 10061,
                ["WSAECONNRESET"] = 10054, ["WSAEHOSTDOWN"] = 10064,
                ["WSAEHOSTUNREACH"] = 10065, ["WSAEINPROGRESS"] = 10036,
                ["WSAEINTR"] = 10004, ["WSAEINVAL"] = 10022, ["WSAEISCONN"] = 10056,
                ["WSAELOOP"] = 10062, ["WSAEMFILE"] = 10024, ["WSAEMSGSIZE"] = 10040,
                ["WSAENAMETOOLONG"] = 10063, ["WSAENETDOWN"] = 10050,
                ["WSAENETRESET"] = 10052, ["WSAENETUNREACH"] = 10051,
                ["WSAENOBUFS"] = 10055, ["WSAENOMORE"] = 10102, ["WSAENOPROTOOPT"] = 10042,
                ["WSAENOTCONN"] = 10057, ["WSAENOTEMPTY"] = 10066, ["WSAENOTSOCK"] = 10038,
                ["WSAEOPNOTSUPP"] = 10045, ["WSAEPFNOSUPPORT"] = 10046,
                ["WSAEPROCLIM"] = 10067, ["WSAEPROTONOSUPPORT"] = 10043,
                ["WSAEPROTOTYPE"] = 10041, ["WSAEPROVIDERFAILEDINIT"] = 10106,
                ["WSAEREFUSED"] = 10112, ["WSAEREMOTE"] = 10071, ["WSAESOCKTNOSUPPORT"] = 10044,
                ["WSAESTALE"] = 10070, ["WSAETIMEDOUT"] = 10060, ["WSAETOOMANYREFS"] = 10059,
                ["WSAEUSERS"] = 10068, ["WSAEWOULDBLOCK"] = 10035, ["WSANOTINITIALISED"] = 10093,
                ["WSASERVICE_NOT_FOUND"] = 10108, ["WSASYSCALLFAILURE"] = 10107,
                ["WSASYSNOTREADY"] = 10091, ["WSATYPE_NOT_FOUND"] = 10109,
                ["WSAVERNOTSUPPORTED"] = 10092, ["WSA_E_CANCELLED"] = 10111, ["WSA_E_NO_MORE"] = 10110,
                ["WSAECANCELLED"] = 10103, ["WSAEDESTADDRREQ"] = 10039,
                ["WSAEDISCON"] = 10101, ["WSAEDQUOT"] = 10069, ["WSAEFAULT"] = 10014,
                ["WSAEINVALIDPROCTABLE"] = 10104, ["WSAEINVALIDPROVIDER"] = 10105,
                ["WSAEBADF"] = 10009
            },
            signals = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["SIGHUP"] = 1, ["SIGINT"] = 2, ["SIGQUIT"] = 3, ["SIGILL"] = 4,
                ["SIGTRAP"] = 5, ["SIGABRT"] = 6, ["SIGIOT"] = 6, ["SIGBUS"] = 7,
                ["SIGFPE"] = 8, ["SIGKILL"] = 9, ["SIGUSR1"] = 10, ["SIGSEGV"] = 11,
                ["SIGUSR2"] = 12, ["SIGPIPE"] = 13, ["SIGALRM"] = 14, ["SIGTERM"] = 15,
                ["SIGCHLD"] = 17, ["SIGCONT"] = 18, ["SIGSTOP"] = 19, ["SIGTSTP"] = 20,
                ["SIGTTIN"] = 21, ["SIGTTOU"] = 22, ["SIGURG"] = 23, ["SIGXCPU"] = 24,
                ["SIGXFSZ"] = 25, ["SIGVTALRM"] = 26, ["SIGPROF"] = 27, ["SIGWINCH"] = 28,
                ["SIGIO"] = 29, ["SIGPOLL"] = 29, ["SIGPWR"] = 30, ["SIGSYS"] = 31
            },
            uv = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["UV_UDP_REUSEADDR"] = 4
            }
        };
    }

    private static string FormatMacAddress(PhysicalAddress address)
    {
        var bytes = address.GetAddressBytes();
        return bytes.Length == 0 ? "00:00:00:00:00:00" : string.Join(":", bytes.Select(item => item.ToString("x2")));
    }

    private static bool IPAddressIsLoopback(System.Net.IPAddress address)
    {
        return System.Net.IPAddress.IsLoopback(address);
    }

    private static string? BuildCidr(UnicastIPAddressInformation address)
    {
        if (address.Address.AddressFamily == AddressFamily.InterNetwork && address.IPv4Mask != null)
        {
            var maskBytes = address.IPv4Mask.GetAddressBytes();
            var prefix = maskBytes.Sum(value => CountBits(value));
            return $"{address.Address}/{prefix}";
        }

        if (address.Address.AddressFamily == AddressFamily.InterNetworkV6 && address.PrefixLength > 0)
            return $"{address.Address}/{address.PrefixLength}";

        return null;
    }

    private static int CountBits(byte value)
    {
        var count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }
}
