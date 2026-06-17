/**
 * DNS record types.
 *
 */

/**
 * Record with TTL (time-to-live) information.
 */
export class RecordWithTtl {
  /** IP address. */
  address: string = "";

  /** Time-to-live in seconds. */
  ttl: number = 0;
}

/**
 * A (IPv4 address) record with type information.
 */
export class AnyARecord extends RecordWithTtl {
  /** Record type: "A". */
  type: string = "A";
}

/**
 * AAAA (IPv6 address) record with type information.
 */
export class AnyAaaaRecord extends RecordWithTtl {
  /** Record type: "AAAA". */
  type: string = "AAAA";
}

/**
 * CAA (Certification Authority Authorization) record.
 */
export class CaaRecord {
  /** Critical flag (0 or 128). */
  critical: number = 0;

  /** Issue property. */
  issue: string | null = null;

  /** Issue wildcard property. */
  issuewild: string | null = null;

  /** IODEF property. */
  iodef: string | null = null;

  /** Contact email property. */
  contactemail: string | null = null;

  /** Contact phone property. */
  contactphone: string | null = null;
}

/**
 * CAA record with type information.
 */
export class AnyCaaRecord extends CaaRecord {
  /** Record type: "CAA". */
  type: string = "CAA";
}

/**
 * MX (Mail Exchange) record.
 */
export class MxRecord {
  /** Mail server priority (lower is higher priority). */
  priority: number = 0;

  /** Mail server hostname. */
  exchange: string = "";
}

/**
 * MX record with type information.
 */
export class AnyMxRecord extends MxRecord {
  /** Record type: "MX". */
  type: string = "MX";
}

/**
 * NAPTR (Naming Authority Pointer) record.
 */
export class NaptrRecord {
  /** NAPTR flags. */
  flags: string = "";

  /** Service specification. */
  service: string = "";

  /** Regular expression. */
  regexp: string = "";

  /** Replacement value. */
  replacement: string = "";

  /** Order value. */
  order: number = 0;

  /** Preference value. */
  preference: number = 0;
}

/**
 * NAPTR record with type information.
 */
export class AnyNaptrRecord extends NaptrRecord {
  /** Record type: "NAPTR". */
  type: string = "NAPTR";
}

/**
 * SOA (Start of Authority) record.
 */
export class SoaRecord {
  /** Primary name server. */
  nsname: string = "";

  /** Responsible party email. */
  hostmaster: string = "";

  /** Serial number. */
  serial: number = 0;

  /** Refresh interval in seconds. */
  refresh: number = 0;

  /** Retry interval in seconds. */
  retry: number = 0;

  /** Expire timeout in seconds. */
  expire: number = 0;

  /** Minimum TTL in seconds. */
  minttl: number = 0;
}

/**
 * SOA record with type information.
 */
export class AnySoaRecord extends SoaRecord {
  /** Record type: "SOA". */
  type: string = "SOA";
}

/**
 * SRV (Service) record.
 */
export class SrvRecord {
  /** Service priority. */
  priority: number = 0;

  /** Service weight. */
  weight: number = 0;

  /** Service port. */
  port: number = 0;

  /** Target hostname. */
  name: string = "";
}

/**
 * SRV record with type information.
 */
export class AnySrvRecord extends SrvRecord {
  /** Record type: "SRV". */
  type: string = "SRV";
}

/**
 * TLSA (DNS-Based Authentication of Named Entities) record.
 */
export class TlsaRecord {
  /** Certificate usage field. */
  certUsage: number = 0;

  /** Selector field. */
  selector: number = 0;

  /** Matching type field. */
  match: number = 0;

  /** Certificate association data. */
  data: Array<number> = [];
}

/**
 * TLSA record with type information.
 */
export class AnyTlsaRecord extends TlsaRecord {
  /** Record type: "TLSA". */
  type: string = "TLSA";
}

/**
 * TXT record with type information.
 */
export class AnyTxtRecord {
  /** Record type: "TXT". */
  type: string = "TXT";

  /** Text entries. */
  entries: Array<string> = [];
}

/**
 * NS (Name Server) record with type information.
 */
export class AnyNsRecord {
  /** Record type: "NS". */
  type: string = "NS";

  /** Name server hostname. */
  value: string = "";
}

/**
 * PTR (Pointer) record with type information.
 */
export class AnyPtrRecord {
  /** Record type: "PTR". */
  type: string = "PTR";

  /** Pointer value. */
  value: string = "";
}

/**
 * CNAME (Canonical Name) record with type information.
 */
export class AnyCnameRecord {
  /** Record type: "CNAME". */
  type: string = "CNAME";

  /** Canonical name value. */
  value: string = "";
}
