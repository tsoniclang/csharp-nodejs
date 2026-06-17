/**
 * DNS Resolver class.
 *
 */
import * as dns from "./index.ts";
import type { ResolverOptions, ResolveOptions } from "./options.ts";
import type { RuntimeValue } from "../runtime-value.ts";
import {
  RecordWithTtl,
  SoaRecord,
  type CaaRecord,
  type MxRecord,
  type NaptrRecord,
  type SrvRecord,
  type TlsaRecord,
} from "./records.ts";

/**
 * An independent resolver for DNS requests.
 * Creating a new resolver uses the default server settings.
 */
export class Resolver {
  _options: ResolverOptions | null;
  _cancelled: boolean = false;
  _localIpv4: string | null = null;
  _localIpv6: string | null = null;

  constructor(options: ResolverOptions | null = null) {
    this._options = options;
  }

  /** Cancel all outstanding DNS queries made by this resolver. */
  cancel(): void {
    this._cancelled = true;
  }

  /** Returns an array of IP address strings currently configured for DNS resolution. */
  getServers(): Array<string> {
    return dns.getServers();
  }

  /** Sets the IP address and port of servers to be used when performing DNS resolution. */
  setServers(servers: Array<string>): void {
    dns.setServers(servers);
  }

  /** The resolver instance will send its requests from the specified IP address. */
  setLocalAddress(ipv4: string | null = null, ipv6: string | null = null): void {
    this._localIpv4 = ipv4;
    this._localIpv6 = ipv6;
  }

  /** Uses the DNS protocol to resolve a host name. */
  resolve(hostname: string, callback: (err: Error | null, addresses: Array<string>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolve(hostname, callback);
  }

  /** Uses the DNS protocol to resolve IPv4 addresses (A records). */
  resolve4(hostname: string, callback: (err: Error | null, addresses: Array<string>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolve4(hostname, callback);
  }

  /** Uses the DNS protocol to resolve IPv4 addresses with options. */
  resolve4WithOptions(
    hostname: string,
    options: ResolveOptions,
    callback: (err: Error | null, result: Array<RecordWithTtl> | Array<string>) => void,
  ): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolve4WithOptions(hostname, options, callback);
  }

  /** Uses the DNS protocol to resolve IPv6 addresses (AAAA records). */
  resolve6(hostname: string, callback: (err: Error | null, addresses: Array<string>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolve6(hostname, callback);
  }

  /** Uses the DNS protocol to resolve IPv6 addresses with options. */
  resolve6WithOptions(
    hostname: string,
    options: ResolveOptions,
    callback: (err: Error | null, result: Array<RecordWithTtl> | Array<string>) => void,
  ): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolve6WithOptions(hostname, options, callback);
  }

  /** Uses the DNS protocol to resolve all records (ANY query). */
  resolveAny(hostname: string, callback: (err: Error | null, records: Array<RuntimeValue>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveAny(hostname, callback);
  }

  /** Uses the DNS protocol to resolve CAA records. */
  resolveCaa(hostname: string, callback: (err: Error | null, records: Array<CaaRecord>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveCaa(hostname, callback);
  }

  /** Uses the DNS protocol to resolve CNAME records. */
  resolveCname(hostname: string, callback: (err: Error | null, addresses: Array<string>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveCname(hostname, callback);
  }

  /** Uses the DNS protocol to resolve MX records. */
  resolveMx(hostname: string, callback: (err: Error | null, records: Array<MxRecord>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveMx(hostname, callback);
  }

  /** Uses the DNS protocol to resolve NAPTR records. */
  resolveNaptr(hostname: string, callback: (err: Error | null, records: Array<NaptrRecord>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveNaptr(hostname, callback);
  }

  /** Uses the DNS protocol to resolve NS records. */
  resolveNs(hostname: string, callback: (err: Error | null, addresses: Array<string>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveNs(hostname, callback);
  }

  /** Uses the DNS protocol to resolve PTR records. */
  resolvePtr(hostname: string, callback: (err: Error | null, addresses: Array<string>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolvePtr(hostname, callback);
  }

  /** Uses the DNS protocol to resolve SOA record. */
  resolveSoa(hostname: string, callback: (err: Error | null, record: SoaRecord) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), new SoaRecord());
      return;
    }
    dns.resolveSoa(hostname, callback);
  }

  /** Uses the DNS protocol to resolve SRV records. */
  resolveSrv(hostname: string, callback: (err: Error | null, records: Array<SrvRecord>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveSrv(hostname, callback);
  }

  /** Uses the DNS protocol to resolve TLSA records. */
  resolveTlsa(hostname: string, callback: (err: Error | null, records: Array<TlsaRecord>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveTlsa(hostname, callback);
  }

  /** Uses the DNS protocol to resolve TXT records. */
  resolveTxt(hostname: string, callback: (err: Error | null, records: Array<Array<string>>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.resolveTxt(hostname, callback);
  }

  /** Performs a reverse DNS query. */
  reverse(ip: string, callback: (err: Error | null, hostnames: Array<string>) => void): void {
    if (this._cancelled) {
      callback(new Error("ECANCELLED"), []);
      return;
    }
    dns.reverse(ip, callback);
  }
}
