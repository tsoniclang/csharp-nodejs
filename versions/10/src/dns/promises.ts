/**
 * Promise-based DNS APIs.
 *
 */
import type { int } from "@tsonic/core/types.js";
import * as dns from "./index.ts";
import type { RuntimeValue } from "../runtime-value.ts";
import { LookupAddress, LookupOptions } from "./options.ts";
import {
  SoaRecord,
  type CaaRecord,
  type MxRecord,
  type NaptrRecord,
  type SrvRecord,
  type TlsaRecord,
} from "./records.ts";

/**
 * Result of a lookupService call.
 */
export class LookupServiceResult {
  hostname: string = "";
  service: string = "";
}

/**
 * Promise-based wrappers over dns callback APIs.
 */
export class DnsPromises {
  lookup(hostname: string, options: LookupOptions | null = null): Promise<LookupAddress> {
    return new Promise((resolve, reject) => {
      dns.lookup(hostname, options, (err, address, family) => {
        if (err !== null) {
          reject(err);
          return;
        }

        const result = new LookupAddress();
        result.address = address;
        result.family = family;
        resolve(result);
      });
    });
  }

  lookupAll(hostname: string, options: LookupOptions | null = null): Promise<Array<LookupAddress>> {
    return new Promise((resolve, reject) => {
      dns.lookupAll(hostname, options, (err, addresses) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(addresses);
      });
    });
  }

  lookupService(address: string, port: int): Promise<LookupServiceResult> {
    return new Promise((resolve, reject) => {
      dns.lookupService(address, port, (err, hostname, service) => {
        if (err !== null) {
          reject(err);
          return;
        }

        const result = new LookupServiceResult();
        result.hostname = hostname;
        result.service = service;
        resolve(result);
      });
    });
  }

  resolve(hostname: string): Promise<Array<string>> {
    return new Promise((resolve, reject) => {
      dns.resolve(hostname, (err, addresses) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(addresses);
      });
    });
  }

  resolveWithRrtype(hostname: string, rrtype: string): Promise<RuntimeValue> {
    return new Promise((resolve, reject) => {
      dns.resolveWithRrtype(hostname, rrtype, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  resolve4(hostname: string): Promise<Array<string>> {
    return new Promise((resolve, reject) => {
      dns.resolve4(hostname, (err, addresses) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(addresses);
      });
    });
  }

  resolve6(hostname: string): Promise<Array<string>> {
    return new Promise((resolve, reject) => {
      dns.resolve6(hostname, (err, addresses) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(addresses);
      });
    });
  }

  resolveCname(hostname: string): Promise<Array<string>> {
    return new Promise((resolve, reject) => {
      dns.resolveCname(hostname, (err, addresses) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(addresses);
      });
    });
  }

  resolveCaa(hostname: string): Promise<Array<CaaRecord>> {
    return new Promise((resolve, reject) => {
      dns.resolveCaa(hostname, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  resolveMx(hostname: string): Promise<Array<MxRecord>> {
    return new Promise((resolve, reject) => {
      dns.resolveMx(hostname, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  resolveNaptr(hostname: string): Promise<Array<NaptrRecord>> {
    return new Promise((resolve, reject) => {
      dns.resolveNaptr(hostname, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  resolveNs(hostname: string): Promise<Array<string>> {
    return new Promise((resolve, reject) => {
      dns.resolveNs(hostname, (err, addresses) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(addresses);
      });
    });
  }

  resolvePtr(hostname: string): Promise<Array<string>> {
    return new Promise((resolve, reject) => {
      dns.resolvePtr(hostname, (err, addresses) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(addresses);
      });
    });
  }

  resolveSoa(hostname: string): Promise<SoaRecord> {
    return new Promise((resolve, reject) => {
      dns.resolveSoa(hostname, (err, record) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(record);
      });
    });
  }

  resolveSrv(hostname: string): Promise<Array<SrvRecord>> {
    return new Promise((resolve, reject) => {
      dns.resolveSrv(hostname, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  resolveTlsa(hostname: string): Promise<Array<TlsaRecord>> {
    return new Promise((resolve, reject) => {
      dns.resolveTlsa(hostname, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  resolveTxt(hostname: string): Promise<Array<Array<string>>> {
    return new Promise((resolve, reject) => {
      dns.resolveTxt(hostname, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  resolveAny(hostname: string): Promise<Array<RuntimeValue>> {
    return new Promise((resolve, reject) => {
      dns.resolveAny(hostname, (err, records) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(records);
      });
    });
  }

  reverse(ip: string): Promise<Array<string>> {
    return new Promise((resolve, reject) => {
      dns.reverse(ip, (err, hostnames) => {
        if (err !== null) {
          reject(err);
          return;
        }

        resolve(hostnames);
      });
    });
  }

  getDefaultResultOrder(): string {
    return dns.getDefaultResultOrder();
  }

  setDefaultResultOrder(order: string): void {
    dns.setDefaultResultOrder(order);
  }

  getServers(): Array<string> {
    return dns.getServers();
  }

  setServers(servers: Array<string>): void {
    dns.setServers(servers);
  }
}
