import type { int } from "@tsonic/core/types.js";

/**
 * DNS option types.
 *
 */

/**
 * Options for DNS lookup operations.
 */
export class LookupOptions {
  /**
   * The record family. Must be 4, 6, or 0.
   * 0 indicates that either an IPv4 or IPv6 address is returned.
   */
  family: int | null = null;

  /**
   * One or more supported getaddrinfo flags. Multiple flags may be passed by bitwise OR.
   */
  hints: number | null = null;

  /**
   * When true, the callback returns all resolved addresses in an array.
   */
  all: boolean | null = null;

  /**
   * The order in which to return addresses: "ipv4first", "ipv6first", or "verbatim".
   */
  order: string | null = null;

  /**
   * When true, the callback receives IPv4 and IPv6 addresses in the order the DNS resolver returned them.
   * Deprecated in favor of order.
   */
  verbatim: boolean | null = null;
}

/**
 * Address returned by DNS lookup.
 */
export class LookupAddress {
  /** A string representation of an IPv4 or IPv6 address. */
  address: string = "";

  /** 4 or 6, denoting the family of address. */
  family: int = 0 as int;
}

/**
 * Options for DNS resolve operations with TTL support.
 */
export class ResolveOptions {
  /** When true, includes TTL (time-to-live) information in results. */
  ttl: boolean = false;
}

/**
 * Options for creating a Resolver instance.
 */
export class ResolverOptions {
  /** Query timeout in milliseconds, or -1 to use the default timeout. */
  timeout: number | null = null;

  /** The number of tries the resolver will try contacting each name server before giving up. */
  tries: number | null = null;

  /** The max retry timeout, in milliseconds. */
  maxTimeout: number | null = null;
}
