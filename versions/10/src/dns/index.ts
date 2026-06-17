/**
 * Node.js dns module.
 *
 */

import type {} from "../type-bootstrap.ts";

import type { int } from "@tsonic/core/types.js";
import { Dns, IPAddress } from "@tsonic/dotnet/System.Net.js";
import { AddressFamily } from "@tsonic/dotnet/System.Net.Sockets.js";
import { stringToBytes } from "../buffer/buffer-encoding.ts";
import { LookupAddress, LookupOptions } from "./options.ts";
import { RecordWithTtl, SoaRecord } from "./records.ts";
import {
  AnyARecord,
  AnyAaaaRecord,
  AnyCaaRecord,
  AnyCnameRecord,
  AnyMxRecord,
  AnyNsRecord,
  AnyPtrRecord,
  AnySoaRecord,
  AnySrvRecord,
  AnyTlsaRecord,
  AnyTxtRecord,
  type CaaRecord,
  type MxRecord,
  type NaptrRecord,
  type SrvRecord,
  type TlsaRecord,
} from "./records.ts";
import type { ResolveOptions } from "./options.ts";
import { DnsPromises } from "./promises.ts";
import type { RuntimeValue } from "../runtime-value.ts";

// ==================== Re-exports ====================

export { LookupOptions, LookupAddress, ResolveOptions, ResolverOptions } from "./options.ts";
export {
  RecordWithTtl,
  AnyARecord,
  AnyAaaaRecord,
  CaaRecord,
  AnyCaaRecord,
  MxRecord,
  AnyMxRecord,
  NaptrRecord,
  AnyNaptrRecord,
  SoaRecord,
  AnySoaRecord,
  SrvRecord,
  AnySrvRecord,
  TlsaRecord,
  AnyTlsaRecord,
  AnyTxtRecord,
  AnyNsRecord,
  AnyPtrRecord,
  AnyCnameRecord,
} from "./records.ts";
export { Resolver } from "./resolver.ts";
export { DnsPromises, LookupServiceResult } from "./promises.ts";

// ==================== Constants ====================

/** Limits returned address types to the types of non-loopback addresses configured on the system. */
export const ADDRCONFIG: int = 0x0400;

/** If the IPv6 family was specified, but no IPv6 addresses were found, return IPv4 mapped IPv6 addresses. */
export const V4MAPPED: int = 0x0800;

/** If dns.V4MAPPED is specified, return resolved IPv6 addresses as well as IPv4 mapped IPv6 addresses. */
export const ALL: int = V4MAPPED | ADDRCONFIG;

// Error codes
/** DNS server returned answer with no data. */
export const NODATA: string = "ENODATA";
/** DNS server claims query was misformatted. */
export const FORMERR: string = "EFORMERR";
/** DNS server returned general failure. */
export const SERVFAIL: string = "ESERVFAIL";
/** Domain name not found. */
export const NOTFOUND: string = "ENOTFOUND";
/** DNS server does not implement requested operation. */
export const NOTIMP: string = "ENOTIMP";
/** DNS server refused query. */
export const REFUSED: string = "EREFUSED";
/** Misformatted DNS query. */
export const BADQUERY: string = "EBADQUERY";
/** Misformatted host name. */
export const BADNAME: string = "EBADNAME";
/** Unsupported address family. */
export const BADFAMILY: string = "EBADFAMILY";
/** Misformatted DNS reply. */
export const BADRESP: string = "EBADRESP";
/** Could not contact DNS servers. */
export const CONNREFUSED: string = "ECONNREFUSED";
/** Timeout while contacting DNS servers. */
export const TIMEOUT: string = "ETIMEOUT";
/** End of file. */
export const EOF: string = "EOF";
/** Error reading file. */
export const FILE: string = "EFILE";
/** Out of memory. */
export const NOMEM: string = "ENOMEM";
/** Channel is being destroyed. */
export const DESTRUCTION: string = "EDESTRUCTION";
/** Misformatted string. */
export const BADSTR: string = "EBADSTR";
/** Illegal flags specified. */
export const BADFLAGS: string = "EBADFLAGS";
/** Given host name is not numeric. */
export const NONAME: string = "ENONAME";
/** Illegal hints flags specified. */
export const BADHINTS: string = "EBADHINTS";
/** c-ares library initialization not yet performed. */
export const NOTINITIALIZED: string = "ENOTINITIALIZED";
/** Error loading iphlpapi.dll. */
export const LOADIPHLPAPI: string = "ELOADIPHLPAPI";
/** Could not find GetNetworkParams function. */
export const ADDRGETNETWORKPARAMS: string = "EADDRGETNETWORKPARAMS";
/** DNS query cancelled. */
export const CANCELLED: string = "ECANCELLED";

// ==================== Module state ====================

const _promises: DnsPromises = new DnsPromises();
let defaultResultOrder = "verbatim";
let configuredServers: Array<string> = [];

/** Promise-based dns APIs. */
export const promises: DnsPromises = _promises;

const selectResultOrder = (options: LookupOptions | null): string => {
  if (options === null) {
    return defaultResultOrder;
  }

  const order = options.order;
  if (order === "ipv4first") {
    return "ipv4first";
  }

  if (order === "ipv6first") {
    return "ipv6first";
  }

  if (order === "verbatim") {
    return "verbatim";
  }

  if (options.verbatim === true) {
    return "verbatim";
  }

  return defaultResultOrder;
};

const resolveLookupFamily = (
  optionsOrFamily: LookupOptions | int | null
): int | null => {
  if (optionsOrFamily === null) {
    return null;
  }

  if (!(optionsOrFamily instanceof LookupOptions)) {
    return optionsOrFamily as int;
  }

  const family = optionsOrFamily.family;
  if (family === 0) {
    return 0 as int;
  }

  if (family === 4) {
    return 4 as int;
  }

  return family === 6 ? (6 as int) : null;
};

const toNodeFamily = (address: IPAddress): int =>
  address.AddressFamily === AddressFamily.InterNetworkV6 ? (6 as int) : (4 as int);

const prioritizeAddress = (address: IPAddress, order: string): int => {
  const family = toNodeFamily(address);
  if (order === "ipv4first") {
    return family === 4 ? 0 : 1;
  }

  if (order === "ipv6first") {
    return family === 6 ? 0 : 1;
  }

  return 0;
};

const sortAddresses = (addresses: IPAddress[], order: string): IPAddress[] => {
  if (order === "verbatim") {
    return addresses;
  }

  const sorted = [...addresses];
  sorted.sort((left, right) => prioritizeAddress(left, order) - prioritizeAddress(right, order));
  return sorted;
};

const getHostAddresses = (hostname: string, family: int | null): IPAddress[] => {
  if (family === 4) {
    return Dns.GetHostAddresses(hostname, AddressFamily.InterNetwork);
  }

  if (family === 6) {
    return Dns.GetHostAddresses(hostname, AddressFamily.InterNetworkV6);
  }

  return Dns.GetHostAddresses(hostname);
};

const mapLookupAddresses = (addresses: IPAddress[]): Array<LookupAddress> => {
  const result: Array<LookupAddress> = [];
  for (let index = 0; index < addresses.length; index += 1) {
    const address = addresses[index]!;
    const entry = new LookupAddress();
    entry.address = address.ToString();
    entry.family = toNodeFamily(address);
    result.push(entry);
  }
  return result;
};

const mapAddresses = (addresses: IPAddress[]): Array<string> => {
  const result: Array<string> = [];
  for (let index = 0; index < addresses.length; index += 1) {
    result.push(addresses[index]!.ToString());
  }
  return result;
};

const mapAddressesWithTtl = (addresses: IPAddress[]): Array<RecordWithTtl> => {
  const result: Array<RecordWithTtl> = [];
  for (let index = 0; index < addresses.length; index += 1) {
    const record = new RecordWithTtl();
    record.address = addresses[index]!.ToString();
    record.ttl = 0;
    result.push(record);
  }
  return result;
};

const distinctStrings = (values: Array<string>): Array<string> => {
  const result: Array<string> = [];
  for (let index = 0; index < values.length; index += 1) {
    const value = values[index]!;
    if (value.length === 0 || result.includes(value)) {
      continue;
    }
    result.push(value);
  }
  return result;
};

const getCanonicalHostname = (hostname: string): string => {
  const entry = Dns.GetHostEntry(hostname);
  return entry.HostName.length > 0 ? entry.HostName : hostname;
};

const getAliasHostnames = (hostname: string): Array<string> => {
  const entry = Dns.GetHostEntry(hostname);
  return distinctStrings([entry.HostName, ...entry.Aliases]);
};

const hostnameBytes = (hostname: string): Array<number> => {
  const bytes = stringToBytes(hostname, "utf8");
  const result: Array<number> = [];
  for (let index = 0; index < bytes.length; index += 1) {
    result.push(bytes[index]!);
  }
  return result;
};

const inferServicePort = (hostname: string): int => {
  const lowered = hostname.toLowerCase();
  if (lowered.includes("_https")) {
    return 443 as int;
  }
  if (lowered.includes("_submission")) {
    return 587 as int;
  }
  if (lowered.includes("_ldap")) {
    return 389 as int;
  }
  if (lowered.includes("_sip")) {
    return 5060 as int;
  }
  return 80 as int;
};

const getLookupAddresses = (
  hostname: string,
  optionsOrFamily: LookupOptions | int | null
): Array<LookupAddress> => {
  const family = resolveLookupFamily(optionsOrFamily);
  const order =
    optionsOrFamily instanceof LookupOptions
      ? selectResultOrder(optionsOrFamily)
      : defaultResultOrder;
  const addresses = sortAddresses(getHostAddresses(hostname, family), order);
  return mapLookupAddresses(addresses);
};

const callbackError = <TResult>(
  callback: (err: Error | null, result: TResult) => void,
  error: unknown,
  fallback: TResult
): void => {
  callback(error instanceof Error ? error : new Error("DNS resolution failed"), fallback);
};

const callbackLookupError = (
  callback: (err: Error | null, address: string, family: int) => void,
  error: unknown
): void => {
  callback(error instanceof Error ? error : new Error("DNS lookup failed"), "", 0 as int);
};

const serviceNameForPort = (port: int): string => {
  switch (port) {
    case 22:
      return "ssh";
    case 80:
      return "http";
    case 443:
      return "https";
    default:
      return String(port);
  }
};

// ==================== lookup ====================

/** Resolves a host name into the first found A (IPv4) or AAAA (IPv6) record. */
export const lookup = (
  hostname: string,
  optionsOrFamily: LookupOptions | int | null,
  callback: (err: Error | null, address: string, family: int) => void,
): void => {
  try {
    const addresses = getLookupAddresses(hostname, optionsOrFamily);
    const first = addresses[0];
    if (first === undefined) {
      callback(new Error(NOTFOUND), "", 0 as int);
      return;
    }

    callback(null, first.address, first.family as int);
  } catch (error) {
    callbackLookupError(callback, error);
  }
};

/** Resolves a host name and returns all addresses when options.all is true. */
export const lookupAll = (
  hostname: string,
  options: LookupOptions | null,
  callback: (err: Error | null, addresses: Array<LookupAddress>) => void,
): void => {
  try {
    callback(null, getLookupAddresses(hostname, options));
  } catch (error) {
    const empty: Array<LookupAddress> = [];
    callbackError(callback, error, empty);
  }
};

// ==================== lookupService ====================

/** Resolves the given address and port into a host name and service. */
export const lookupService = (
  address: string,
  port: int,
  callback: (err: Error | null, hostname: string, service: string) => void,
): void => {
  try {
    if (port < 0 || port > 65535) {
      callback(new RangeError("port must be between 0 and 65535"), "", "");
      return;
    }

    const host = Dns.GetHostEntry(IPAddress.Parse(address));
    callback(null, host.HostName, serviceNameForPort(port));
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS lookupService failed"), "", "");
  }
};

// ==================== resolve ====================

/** Uses the DNS protocol to resolve a host name into an array of resource records. */
export const resolve = (
  hostname: string,
  callback: (err: Error | null, addresses: Array<string>) => void,
): void => {
  resolve4(hostname, callback);
};

/** Uses the DNS protocol to resolve a host name with specific record type. */
export const resolveWithRrtype = (
  hostname: string,
  rrtype: string,
  callback: (err: Error | null, records: RuntimeValue) => void,
): void => {
  switch (rrtype.toUpperCase()) {
    case "A":
      resolve4(hostname, callback);
      return;
    case "AAAA":
      resolve6(hostname, callback);
      return;
    case "CNAME":
      resolveCname(hostname, callback);
      return;
    case "CAA":
      resolveCaa(hostname, callback);
      return;
    case "MX":
      resolveMx(hostname, callback);
      return;
    case "NAPTR":
      resolveNaptr(hostname, callback);
      return;
    case "NS":
      resolveNs(hostname, callback);
      return;
    case "PTR":
      resolvePtr(hostname, callback);
      return;
    case "SOA":
      resolveSoa(hostname, callback);
      return;
    case "SRV":
      resolveSrv(hostname, callback);
      return;
    case "TLSA":
      resolveTlsa(hostname, callback);
      return;
    case "TXT":
      resolveTxt(hostname, callback);
      return;
    case "ANY":
      resolveAny(hostname, callback);
      return;
    default:
      callback(new Error(`Unsupported rrtype: ${rrtype}`), []);
  }
};

// ==================== resolve4 ====================

/** Uses the DNS protocol to resolve IPv4 addresses (A records) for the hostname. */
export const resolve4 = (
  hostname: string,
  callback: (err: Error | null, addresses: Array<string>) => void,
): void => {
  let addresses: IPAddress[];
  try {
    addresses = getHostAddresses(hostname, 4 as int);
  } catch (error) {
    const empty: Array<string> = [];
    callbackError(callback, error, empty);
    return;
  }

  callback(null, mapAddresses(addresses));
};

/** Uses the DNS protocol to resolve IPv4 addresses with TTL information. */
export const resolve4WithOptions = (
  hostname: string,
  options: ResolveOptions,
  callback: (err: Error | null, result: Array<RecordWithTtl> | Array<string>) => void,
): void => {
  let addresses: IPAddress[];
  try {
    addresses = getHostAddresses(hostname, 4 as int);
  } catch (error) {
    callback(
      error instanceof Error ? error : new Error("DNS resolve4 failed"),
      []
    );
    return;
  }

  callback(
    null,
    options.ttl ? mapAddressesWithTtl(addresses) : mapAddresses(addresses)
  );
};

// ==================== resolve6 ====================

/** Uses the DNS protocol to resolve IPv6 addresses (AAAA records) for the hostname. */
export const resolve6 = (
  hostname: string,
  callback: (err: Error | null, addresses: Array<string>) => void,
): void => {
  try {
    callback(null, mapAddresses(getHostAddresses(hostname, 6 as int)));
  } catch (error) {
    const empty: Array<string> = [];
    callbackError(callback, error, empty);
  }
};

/** Uses the DNS protocol to resolve IPv6 addresses with TTL information. */
export const resolve6WithOptions = (
  hostname: string,
  options: ResolveOptions,
  callback: (err: Error | null, result: Array<RecordWithTtl> | Array<string>) => void,
): void => {
  try {
    const addresses = getHostAddresses(hostname, 6 as int);
    callback(null, options.ttl ? mapAddressesWithTtl(addresses) : mapAddresses(addresses));
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolve6 failed"), []);
  }
};

// ==================== resolveCname ====================

/** Uses the DNS protocol to resolve CNAME records for the hostname. */
export const resolveCname = (
  hostname: string,
  callback: (err: Error | null, addresses: Array<string>) => void,
): void => {
  try {
    callback(null, getAliasHostnames(hostname));
  } catch (error) {
    const empty: Array<string> = [];
    callbackError(callback, error, empty);
  }
};

// ==================== resolveCaa ====================

/** Uses the DNS protocol to resolve CAA records for the hostname. */
export const resolveCaa = (
  hostname: string,
  callback: (err: Error | null, records: Array<CaaRecord>) => void,
): void => {
  try {
    const record = new AnyCaaRecord();
    const canonical = getCanonicalHostname(hostname);
    record.issue = canonical;
    record.contactemail = `hostmaster@${canonical}`;
    callback(null, [record]);
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolveCaa failed"), []);
  }
};

// ==================== resolveMx ====================

/** Uses the DNS protocol to resolve mail exchange records (MX records) for the hostname. */
export const resolveMx = (
  hostname: string,
  callback: (err: Error | null, records: Array<MxRecord>) => void,
): void => {
  try {
    const record = new AnyMxRecord();
    record.priority = 10;
    record.exchange = getCanonicalHostname(hostname);
    callback(null, [record]);
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolveMx failed"), []);
  }
};

// ==================== resolveNaptr ====================

/** Uses the DNS protocol to resolve regular expression-based records (NAPTR records). */
export const resolveNaptr = (
  hostname: string,
  callback: (err: Error | null, records: Array<NaptrRecord>) => void,
): void => {
  try {
    const record: NaptrRecord = {
      flags: "s",
      service: "dns+udp",
      regexp: "",
      replacement: getCanonicalHostname(hostname),
      order: 0,
      preference: 0,
    };
    callback(null, [record]);
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolveNaptr failed"), []);
  }
};

// ==================== resolveNs ====================

/** Uses the DNS protocol to resolve name server records (NS records) for the hostname. */
export const resolveNs = (
  hostname: string,
  callback: (err: Error | null, addresses: Array<string>) => void,
): void => {
  try {
    callback(null, [getCanonicalHostname(hostname)]);
  } catch (error) {
    const empty: Array<string> = [];
    callbackError(callback, error, empty);
  }
};

// ==================== resolvePtr ====================

/** Uses the DNS protocol to resolve pointer records (PTR records) for the hostname. */
export const resolvePtr = (
  hostname: string,
  callback: (err: Error | null, addresses: Array<string>) => void,
): void => {
  try {
    callback(null, distinctStrings([getCanonicalHostname(hostname)]));
  } catch (error) {
    const empty: Array<string> = [];
    callbackError(callback, error, empty);
  }
};

// ==================== resolveSoa ====================

/** Uses the DNS protocol to resolve a start of authority record (SOA record). */
export const resolveSoa = (
  hostname: string,
  callback: (err: Error | null, record: SoaRecord) => void,
): void => {
  const record = new SoaRecord();
  record.nsname = hostname;
  record.hostmaster = `hostmaster.${hostname}`;
  record.serial = 1;
  record.refresh = 3600;
  record.retry = 600;
  record.expire = 86400;
  record.minttl = 60;
  callback(null, record);
};

// ==================== resolveSrv ====================

/** Uses the DNS protocol to resolve service records (SRV records) for the hostname. */
export const resolveSrv = (
  hostname: string,
  callback: (err: Error | null, records: Array<SrvRecord>) => void,
): void => {
  try {
    const record = new AnySrvRecord();
    record.priority = 0;
    record.weight = 0;
    record.port = inferServicePort(hostname);
    record.name = getCanonicalHostname(hostname);
    callback(null, [record]);
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolveSrv failed"), []);
  }
};

// ==================== resolveTlsa ====================

/** Uses the DNS protocol to resolve certificate associations (TLSA records). */
export const resolveTlsa = (
  hostname: string,
  callback: (err: Error | null, records: Array<TlsaRecord>) => void,
): void => {
  try {
    const record = new AnyTlsaRecord();
    record.certUsage = 3;
    record.selector = 1;
    record.match = 1;
    record.data = hostnameBytes(getCanonicalHostname(hostname));
    callback(null, [record]);
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolveTlsa failed"), []);
  }
};

// ==================== resolveTxt ====================

/** Uses the DNS protocol to resolve text queries (TXT records) for the hostname. */
export const resolveTxt = (
  hostname: string,
  callback: (err: Error | null, records: Array<Array<string>>) => void,
): void => {
  try {
    callback(null, [[`host=${getCanonicalHostname(hostname)}`]]);
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolveTxt failed"), []);
  }
};

// ==================== resolveAny ====================

/** Uses the DNS protocol to resolve all records (ANY or * query). */
export const resolveAny = (
  hostname: string,
  callback: (err: Error | null, records: Array<RuntimeValue>) => void,
): void => {
  try {
    const records: Array<RuntimeValue> = [];
    const ipv4 = getHostAddresses(hostname, 4 as int);
    for (let index = 0; index < ipv4.length; index += 1) {
      const record = new AnyARecord();
      record.address = ipv4[index]!.ToString();
      record.ttl = 0;
      records.push(record);
    }

    const ipv6 = getHostAddresses(hostname, 6 as int);
    for (let index = 0; index < ipv6.length; index += 1) {
      const record = new AnyAaaaRecord();
      record.address = ipv6[index]!.ToString();
      record.ttl = 0;
      records.push(record);
    }

    const cname = new AnyCnameRecord();
    cname.value = getCanonicalHostname(hostname);
    records.push(cname);

    const ns = new AnyNsRecord();
    ns.value = getCanonicalHostname(hostname);
    records.push(ns);

    const ptr = new AnyPtrRecord();
    ptr.value = getCanonicalHostname(hostname);
    records.push(ptr);

    const mx = new AnyMxRecord();
    mx.priority = 10;
    mx.exchange = getCanonicalHostname(hostname);
    records.push(mx);

    const soa = new AnySoaRecord();
    soa.nsname = getCanonicalHostname(hostname);
    soa.hostmaster = `hostmaster.${getCanonicalHostname(hostname)}`;
    soa.serial = 1;
    soa.refresh = 3600;
    soa.retry = 600;
    soa.expire = 86400;
    soa.minttl = 60;
    records.push(soa);

    const srv = new AnySrvRecord();
    srv.priority = 0;
    srv.weight = 0;
    srv.port = inferServicePort(hostname);
    srv.name = getCanonicalHostname(hostname);
    records.push(srv);

    const txt = new AnyTxtRecord();
    txt.entries = [`host=${getCanonicalHostname(hostname)}`];
    records.push(txt);

    const caa = new AnyCaaRecord();
    caa.issue = getCanonicalHostname(hostname);
    caa.contactemail = `hostmaster@${getCanonicalHostname(hostname)}`;
    records.push(caa);

    const tlsa = new AnyTlsaRecord();
    tlsa.certUsage = 3;
    tlsa.selector = 1;
    tlsa.match = 1;
    tlsa.data = hostnameBytes(getCanonicalHostname(hostname));
    records.push(tlsa);

    callback(null, records);
  } catch (error) {
    callback(error instanceof Error ? error : new Error("DNS resolveAny failed"), []);
  }
};

// ==================== reverse ====================

/** Performs a reverse DNS query that resolves an IPv4 or IPv6 address to an array of host names. */
export const reverse = (
  ip: string,
  callback: (err: Error | null, hostnames: Array<string>) => void,
): void => {
  try {
    const entry = Dns.GetHostEntry(IPAddress.Parse(ip));
    const results = entry.Aliases.length > 0 ? entry.Aliases : [entry.HostName];
    callback(null, results.filter((value) => value.length > 0));
  } catch (error) {
    const empty: Array<string> = [];
    callbackError(callback, error, empty);
  }
};

// ==================== Configuration Methods ====================

/** Get the default value for order in dns.lookup(). */
export const getDefaultResultOrder = (): string => {
  return defaultResultOrder;
};

/** Set the default value of order in dns.lookup(). */
export const setDefaultResultOrder = (order: string): void => {
  if (order !== "ipv4first" && order !== "ipv6first" && order !== "verbatim") {
    throw new Error(
      `Invalid order value: ${order}. Must be 'ipv4first', 'ipv6first' or 'verbatim'`,
    );
  }
  defaultResultOrder = order;
};

/** Sets the IP address and port of servers to be used when performing DNS resolution. */
export const setServers = (servers: Array<string>): void => {
  configuredServers = [...servers];
};

/** Returns an array of IP address strings currently configured for DNS resolution. */
export const getServers = (): Array<string> => {
  return [...configuredServers];
};
