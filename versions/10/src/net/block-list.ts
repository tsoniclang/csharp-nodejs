/**
 * net.BlockList — IP address block list management.
 *
 */
import type { int } from "@tsonic/core/types.js";
import { SocketAddressInitOptions } from "./options.ts";

/**
 * Represents a socket address.
 */
export class SocketAddress {
  address: string;
  family: string;
  flowlabel: number | undefined;
  port: number;

  constructor(options: SocketAddressInitOptions) {
    this.address = options.address ?? "0.0.0.0";
    this.family = options.family ?? "ipv4";
    this.flowlabel = options.flowlabel;
    this.port = options.port ?? 0;
  }
}

type BlockedRange = {
  start: string;
  end: string;
  type: string;
};

type BlockedSubnet = {
  network: string;
  prefix: number;
  type: string;
};

const parseIPv4Octets = (address: string): number[] | undefined => {
  const parts = address.split(".");
  if (parts.length !== 4) {
    return undefined;
  }
  const octets: number[] = [];
  for (const part of parts) {
    const n = parseInt(part, 10);
    if (isNaN(n) || n < 0 || n > 255) {
      return undefined;
    }
    octets.push(n);
  }
  return octets;
};

const compareIPv4 = (a: number[], b: number[]): number => {
  for (let i = 0; i < 4; i = i + 1) {
    if (a[i]! < b[i]!) {
      return -1;
    }
    if (a[i]! > b[i]!) {
      return 1;
    }
  }
  return 0;
};

const isInRange = (addr: string, start: string, end: string): boolean => {
  const addrOctets = parseIPv4Octets(addr);
  const startOctets = parseIPv4Octets(start);
  const endOctets = parseIPv4Octets(end);

  if (
    addrOctets === undefined ||
    startOctets === undefined ||
    endOctets === undefined
  ) {
    return false;
  }

  return (
    compareIPv4(addrOctets, startOctets) >= 0 &&
    compareIPv4(addrOctets, endOctets) <= 0
  );
};

const isInSubnet = (
  addr: string,
  network: string,
  prefix: number
): boolean => {
  const addrOctets = parseIPv4Octets(addr);
  const networkOctets = parseIPv4Octets(network);

  if (addrOctets === undefined || networkOctets === undefined) {
    return false;
  }

  let fullBytes: int = 0;
  let remainingBits = prefix;

  while (remainingBits >= 8) {
    fullBytes = fullBytes + 1;
    remainingBits -= 8;
  }

  for (let i = 0; i < fullBytes; i = i + 1) {
    if (addrOctets[i]! !== networkOctets[i]!) {
      return false;
    }
  }

  if (remainingBits > 0) {
    const mask = (0xff << (8 - remainingBits)) & 0xff;
    if ((addrOctets[fullBytes]! & mask) !== (networkOctets[fullBytes]! & mask)) {
      return false;
    }
  }

  return true;
};

/**
 * The BlockList object can be used with some network APIs to specify rules
 * for disabling inbound or outbound access to specific IP addresses, IP ranges,
 * or IP subnets.
 */
export class BlockList {
  blockedAddresses: Set<string> = new Set<string>();
  blockedAddressList: string[] = [];
  blockedRanges: BlockedRange[] = [];
  blockedSubnets: BlockedSubnet[] = [];

  /**
   * Adds a rule to block the given IP address.
   */
  addAddress(address: string, type: string = "ipv4"): void {
    if (!this.blockedAddresses.has(address)) {
      this.blockedAddressList.push(address);
    }
    this.blockedAddresses.add(address);
  }

  /**
   * Adds a rule to block a range of IP addresses from start (inclusive) to end (inclusive).
   */
  addRange(
    start: string,
    end: string,
    type: string = "ipv4"
  ): void {
    this.blockedRanges.push({ start, end, type });
  }

  /**
   * Adds a rule to block a range of IP addresses specified as a subnet mask.
   */
  addSubnet(
    network: string,
    prefix: number,
    type: string = "ipv4"
  ): void {
    this.blockedSubnets.push({ network, prefix, type });
  }

  /**
   * Returns true if the given IP address matches any of the rules added to the BlockList.
   */
  check(address: string, type: string = "ipv4"): boolean {
    if (this.blockedAddresses.has(address)) {
      return true;
    }

    for (let index = 0; index < this.blockedRanges.length; index += 1) {
      const range = this.blockedRanges[index]!;
      if (range.type === type && isInRange(address, range.start, range.end)) {
        return true;
      }
    }

    for (let index = 0; index < this.blockedSubnets.length; index += 1) {
      const subnet = this.blockedSubnets[index]!;
      if (
        subnet.type === type &&
        isInSubnet(address, subnet.network, subnet.prefix)
      ) {
        return true;
      }
    }

    return false;
  }

  /**
   * Returns an array of rules added to the blocklist.
   */
  getRules(): string[] {
    const rules: string[] = [];
    rules.push(...this.blockedAddressList);
    for (let index = 0; index < this.blockedRanges.length; index += 1) {
      const range = this.blockedRanges[index]!;
      rules.push(`${range.start}-${range.end}`);
    }
    for (let index = 0; index < this.blockedSubnets.length; index += 1) {
      const subnet = this.blockedSubnets[index]!;
      rules.push(`${subnet.network}/${String(subnet.prefix)}`);
    }
    return rules;
  }
}
