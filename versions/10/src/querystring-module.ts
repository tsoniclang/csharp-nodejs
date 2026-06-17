/**
 * Node.js querystring module — utilities for parsing and formatting URL query strings.
 *
 */

import type {} from "./type-bootstrap.ts";

/**
 * Performs URL percent-encoding on the given string.
 */
export const escape = (str: string): string => {
  if (str === null || str === undefined || str.length === 0) {
    return str;
  }
  return encodeURIComponent(str);
};

/**
 * Performs decoding of URL percent-encoded characters on the given string.
 */
export const unescape = (str: string): string => {
  if (str === null || str === undefined || str.length === 0) {
    return str;
  }
  try {
    return decodeURIComponent(str);
  } catch {
    return str;
  }
};

export type QueryStringValue = string | string[] | null | undefined;
export type QueryStringInput = Map<string, QueryStringValue>;
export type QueryStringOutput = Map<string, string | string[]>;

const convertToString = (value: string | null | undefined): string => {
  if (value === null || value === undefined) {
    return "";
  }
  return String(value);
};

/**
 * Produces a URL query string from a given object by iterating through the object's properties.
 *
 * @param obj - The object to serialize into a URL query string.
 * @param sep - The substring used to delimit key and value pairs. Default is '&'.
 * @param eq - The substring used to delimit keys and values. Default is '='.
 * @returns A URL query string.
 */
export const stringify = (
  obj: QueryStringInput | null | undefined,
  sep?: string | null,
  eq?: string | null
): string => {
  if (obj === null || obj === undefined) {
    return "";
  }

  const actualSep = sep ?? "&";
  const actualEq = eq ?? "=";

  const parts: string[] = [];

  for (const rawKey of obj.keys()) {
    const key = escape(rawKey);
    const value = obj.get(rawKey);
    if (value === undefined || value === null) {
      parts.push(key + actualEq);
      continue;
    }

    if (Array.isArray(value)) {
      for (const item of value) {
        parts.push(key + actualEq + escape(convertToString(item)));
      }
      continue;
    }

    parts.push(key + actualEq + escape(convertToString(value)));
  }

  return parts.join(actualSep);
};

/**
 * Parses a URL query string into a collection of key and value pairs.
 *
 * @param str - The URL query string to parse.
 * @param sep - The substring used to delimit key and value pairs. Default is '&'.
 * @param eq - The substring used to delimit keys and values. Default is '='.
 * @param maxKeys - Maximum number of keys to parse. 0 removes the limit. Default is 1000.
 * @returns A record of key-value pairs.
 */
export const parse = (
  str: string,
  sep?: string | null,
  eq?: string | null,
  maxKeys?: number
): QueryStringOutput => {
  const result: QueryStringOutput = new Map<string, string | string[]>();

  if (str === null || str === undefined || str.length === 0) {
    return result;
  }

  let input = str;
  if (input.startsWith("?")) {
    input = input.slice(1);
  }

  const actualSep = sep ?? "&";
  const actualEq = eq ?? "=";
  let effectiveMaxKeys = 1000;
  if (maxKeys !== undefined) {
    effectiveMaxKeys = maxKeys;
  }

  const pairs = input.split(actualSep);
  let count = 0;

  for (let i = 0; i < pairs.length; i += 1) {
    if (effectiveMaxKeys > 0 && count >= effectiveMaxKeys) {
      break;
    }

    const pair = pairs[i]!;
    const eqIndex = pair.indexOf(actualEq);
    let key: string;
    let value: string;

    if (eqIndex >= 0) {
      key = unescape(pair.slice(0, eqIndex));
      value = unescape(pair.slice(eqIndex + actualEq.length));
    } else {
      key = unescape(pair);
      value = "";
    }

    const existing = result.get(key);
    if (existing !== undefined) {
      if (Array.isArray(existing)) {
        existing.push(value);
      } else {
        result.set(key, [existing, value]);
      }
    } else {
      result.set(key, value);
    }

    count += 1;
  }

  return result;
};

/**
 * Alias for stringify().
 */
export const encode = (
  obj: QueryStringInput | null | undefined,
  sep?: string | null,
  eq?: string | null
): string => stringify(obj, sep, eq);

/**
 * Alias for parse().
 */
export const decode = (
  str: string,
  sep?: string | null,
  eq?: string | null,
  maxKeys?: number
): QueryStringOutput => parse(str, sep, eq, maxKeys);
