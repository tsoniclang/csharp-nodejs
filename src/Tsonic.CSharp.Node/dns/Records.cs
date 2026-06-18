using System;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// Record with TTL (time-to-live) information.
/// </summary>
public class RecordWithTtl
{
    /// <summary>
    /// IP address.
    /// </summary>
    public string address { get; set; } = string.Empty;

    /// <summary>
    /// Time-to-live in seconds.
    /// </summary>
    public int ttl { get; set; }
}

/// <summary>
/// Base class for records with type information.
/// </summary>
public abstract class AnyRecord
{
    /// <summary>
    /// DNS record type (A, AAAA, CAA, CNAME, MX, NAPTR, NS, PTR, SOA, SRV, TLSA, TXT).
    /// </summary>
    public string type { get; protected set; } = string.Empty;
}

/// <summary>
/// A (IPv4 address) record.
/// </summary>
public class AnyARecord : RecordWithTtl
{
    /// <summary>
    /// Record type: "A".
    /// </summary>
    public string type { get; } = "A";
}

/// <summary>
/// AAAA (IPv6 address) record.
/// </summary>
public class AnyAaaaRecord : RecordWithTtl
{
    /// <summary>
    /// Record type: "AAAA".
    /// </summary>
    public string type { get; } = "AAAA";
}

/// <summary>
/// CAA (Certification Authority Authorization) record.
/// </summary>
public class CaaRecord
{
    /// <summary>
    /// Critical flag (0 or 128).
    /// </summary>
    public int critical { get; set; }

    /// <summary>
    /// Issue property.
    /// </summary>
    public string? issue { get; set; }

    /// <summary>
    /// Issue wildcard property.
    /// </summary>
    public string? issuewild { get; set; }

    /// <summary>
    /// IODEF property.
    /// </summary>
    public string? iodef { get; set; }

    /// <summary>
    /// Contact email property.
    /// </summary>
    public string? contactemail { get; set; }

    /// <summary>
    /// Contact phone property.
    /// </summary>
    public string? contactphone { get; set; }
}

/// <summary>
/// CAA record with type information.
/// </summary>
public class AnyCaaRecord : CaaRecord
{
    /// <summary>
    /// Record type: "CAA".
    /// </summary>
    public string type { get; } = "CAA";
}

/// <summary>
/// MX (Mail Exchange) record.
/// </summary>
public class MxRecord
{
    /// <summary>
    /// Mail server priority (lower is higher priority).
    /// </summary>
    public int priority { get; set; }

    /// <summary>
    /// Mail server hostname.
    /// </summary>
    public string exchange { get; set; } = string.Empty;
}

/// <summary>
/// MX record with type information.
/// </summary>
public class AnyMxRecord : MxRecord
{
    /// <summary>
    /// Record type: "MX".
    /// </summary>
    public string type { get; } = "MX";
}

/// <summary>
/// NAPTR (Naming Authority Pointer) record.
/// </summary>
public class NaptrRecord
{
    /// <summary>
    /// NAPTR flags.
    /// </summary>
    public string flags { get; set; } = string.Empty;

    /// <summary>
    /// Service specification.
    /// </summary>
    public string service { get; set; } = string.Empty;

    /// <summary>
    /// Regular expression.
    /// </summary>
    public string regexp { get; set; } = string.Empty;

    /// <summary>
    /// Replacement value.
    /// </summary>
    public string replacement { get; set; } = string.Empty;

    /// <summary>
    /// Order value.
    /// </summary>
    public int order { get; set; }

    /// <summary>
    /// Preference value.
    /// </summary>
    public int preference { get; set; }
}

/// <summary>
/// NAPTR record with type information.
/// </summary>
public class AnyNaptrRecord : NaptrRecord
{
    /// <summary>
    /// Record type: "NAPTR".
    /// </summary>
    public string type { get; } = "NAPTR";
}

/// <summary>
/// SOA (Start of Authority) record.
/// </summary>
public class SoaRecord
{
    /// <summary>
    /// Primary name server.
    /// </summary>
    public string nsname { get; set; } = string.Empty;

    /// <summary>
    /// Responsible party email.
    /// </summary>
    public string hostmaster { get; set; } = string.Empty;

    /// <summary>
    /// Serial number.
    /// </summary>
    public int serial { get; set; }

    /// <summary>
    /// Refresh interval in seconds.
    /// </summary>
    public int refresh { get; set; }

    /// <summary>
    /// Retry interval in seconds.
    /// </summary>
    public int retry { get; set; }

    /// <summary>
    /// Expire timeout in seconds.
    /// </summary>
    public int expire { get; set; }

    /// <summary>
    /// Minimum TTL in seconds.
    /// </summary>
    public int minttl { get; set; }
}

/// <summary>
/// SOA record with type information.
/// </summary>
public class AnySoaRecord : SoaRecord
{
    /// <summary>
    /// Record type: "SOA".
    /// </summary>
    public string type { get; } = "SOA";
}

/// <summary>
/// SRV (Service) record.
/// </summary>
public class SrvRecord
{
    /// <summary>
    /// Service priority.
    /// </summary>
    public int priority { get; set; }

    /// <summary>
    /// Service weight.
    /// </summary>
    public int weight { get; set; }

    /// <summary>
    /// Service port.
    /// </summary>
    public int port { get; set; }

    /// <summary>
    /// Target hostname.
    /// </summary>
    public string name { get; set; } = string.Empty;
}

/// <summary>
/// SRV record with type information.
/// </summary>
public class AnySrvRecord : SrvRecord
{
    /// <summary>
    /// Record type: "SRV".
    /// </summary>
    public string type { get; } = "SRV";
}

/// <summary>
/// TLSA (DNS-Based Authentication of Named Entities) record.
/// </summary>
public class TlsaRecord
{
    /// <summary>
    /// Certificate usage field.
    /// </summary>
    public int certUsage { get; set; }

    /// <summary>
    /// Selector field.
    /// </summary>
    public int selector { get; set; }

    /// <summary>
    /// Matching type field.
    /// </summary>
    public int match { get; set; }

    /// <summary>
    /// Certificate association data.
    /// </summary>
    public byte[] data { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// TLSA record with type information.
/// </summary>
public class AnyTlsaRecord : TlsaRecord
{
    /// <summary>
    /// Record type: "TLSA".
    /// </summary>
    public string type { get; } = "TLSA";
}

/// <summary>
/// TXT record with type information.
/// </summary>
public class AnyTxtRecord
{
    /// <summary>
    /// Record type: "TXT".
    /// </summary>
    public string type { get; } = "TXT";

    /// <summary>
    /// Text entries.
    /// </summary>
    public string[] entries { get; set; } = Array.Empty<string>();
}

/// <summary>
/// NS (Name Server) record with type information.
/// </summary>
public class AnyNsRecord
{
    /// <summary>
    /// Record type: "NS".
    /// </summary>
    public string type { get; } = "NS";

    /// <summary>
    /// Name server hostname.
    /// </summary>
    public string value { get; set; } = string.Empty;
}

/// <summary>
/// PTR (Pointer) record with type information.
/// </summary>
public class AnyPtrRecord
{
    /// <summary>
    /// Record type: "PTR".
    /// </summary>
    public string type { get; } = "PTR";

    /// <summary>
    /// Pointer value.
    /// </summary>
    public string value { get; set; } = string.Empty;
}

/// <summary>
/// CNAME (Canonical Name) record with type information.
/// </summary>
public class AnyCnameRecord
{
    /// <summary>
    /// Record type: "CNAME".
    /// </summary>
    public string type { get; } = "CNAME";

    /// <summary>
    /// Canonical name value.
    /// </summary>
    public string value { get; set; } = string.Empty;
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
