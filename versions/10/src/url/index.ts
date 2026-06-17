/**
 * Node.js url module — URL parsing, formatting, and resolution utilities.
 *
 */

import type {} from "../type-bootstrap.ts";
import type { RuntimeValue } from "../runtime-value.ts";
import { IdnMapping } from "@tsonic/dotnet/System.Globalization.js";
import { Uri } from "@tsonic/dotnet/System.js";
import { Path } from "@tsonic/dotnet/System.IO.js";

export { URL } from "./url.ts";
export { URLSearchParams } from "./urlsearch-params.ts";
export { URLPattern } from "./urlpattern.ts";

import { URL } from "./url.ts";

/**
 * Parses URL input and returns URL instance.
 */
export const parse = (input: string): URL | null => {
  return URL.parse(input);
};

/**
 * Formats URL input to string.
 */
export const format = (input: RuntimeValue): string => {
  if (input === null || input === undefined) {
    return "";
  }
  if (input instanceof URL) {
    return input.href;
  }
  return String(input);
};

/**
 * Resolves relative URL against a base URL.
 */
export const resolve = (from: string, to: string): string => {
  const resolved = new URL(to, from);
  return resolved.href;
};

export const domainToASCII = (domain: string): string => {
  if (domain.length === 0) {
    return "";
  }

  try {
    return new IdnMapping().GetAscii(domain);
  } catch {
    return "";
  }
};

export const domainToUnicode = (domain: string): string => {
  if (domain.length === 0) {
    return "";
  }

  try {
    return new IdnMapping().GetUnicode(domain);
  } catch {
    return "";
  }
};

export const pathToFileURL = (filePath: string): URL => {
  const fullPath = Path.GetFullPath(filePath);
  return new URL(new Uri(fullPath).AbsoluteUri);
};

export const fileURLToPath = (input: string | URL): string => {
  const href = input instanceof URL ? input.href : input;
  return new Uri(href).LocalPath;
};

export class HttpUrlOptions {
  protocol: string;
  hostname: string;
  hash: string;
  search: string;
  pathname: string;
  path: string;
  href: string;
  port: number | undefined;
  auth: string | undefined;

  constructor(
    protocol: string,
    hostname: string,
    hash: string,
    search: string,
    pathname: string,
    path: string,
    href: string,
    port?: number,
    auth?: string,
  ) {
    this.protocol = protocol;
    this.hostname = hostname;
    this.hash = hash;
    this.search = search;
    this.pathname = pathname;
    this.path = path;
    this.href = href;
    this.port = port;
    this.auth = auth;
  }
}

export const urlToHttpOptions = (
  input: URL,
): HttpUrlOptions => {
  const auth =
    input.username.length > 0
      ? `${input.username}:${input.password}`
      : undefined;

  return new HttpUrlOptions(
    input.protocol,
    input.hostname,
    input.hash,
    input.search,
    input.pathname,
    `${input.pathname}${input.search}`,
    input.href,
    input.port.length > 0 ? parseInt(input.port, 10) : undefined,
    auth,
  );
};
