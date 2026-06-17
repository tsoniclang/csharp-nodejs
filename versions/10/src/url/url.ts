/**
 * URL class — parsed URL with properties for accessing and modifying URL components.
 *
 *
 * NOTE: This implementation wraps the global URL constructor which is available
 * in modern JS runtimes. This implementation wraps System.Uri and
 * preserves the same public surface shape.
 */

import {
  Uri,
  UriBuilder,
  UriFormatException,
} from "@tsonic/dotnet/System.js";
import type { int } from "@tsonic/core/types.js";
import { URLSearchParams } from "./urlsearch-params.ts";

const trimLeading = (value: string, prefix: string): string => {
  return value.startsWith(prefix) ? value.slice(prefix.length) : value;
};

const parsePort = (value: string): int => {
  const parsed = parseInt(value, 10);
  if (Number.isInteger(parsed) && parsed >= -1 && parsed <= 65535) {
    return parsed as int;
  }
  return -1 as int;
};

class UserInfoParts {
  username: string;
  password: string;

  constructor(username: string, password: string) {
    this.username = username;
    this.password = password;
  }
}

const splitUserInfo = (userInfo: string): UserInfoParts => {
  if (userInfo.length === 0) {
    return new UserInfoParts("", "");
  }
  const separatorIndex = userInfo.indexOf(":");
  if (separatorIndex < 0) {
    return new UserInfoParts(userInfo, "");
  }
  return new UserInfoParts(
    userInfo.slice(0, separatorIndex),
    userInfo.slice(separatorIndex + 1),
  );
};

const buildUriBuilder = (input: string, base?: string): UriBuilder => {
  if (base !== undefined) {
    const baseUri = new Uri(base);
    return new UriBuilder(new Uri(baseUri, input));
  }

  return new UriBuilder(input);
};

export class URL {
  builder: UriBuilder;
  cachedSearchParams: URLSearchParams | null = null;

  constructor(input: string, base?: string) {
    this.builder = buildUriBuilder(input, base);
  }

  get uri(): Uri {
    return this.builder.Uri;
  }

  get href(): string {
    return this.uri.AbsoluteUri;
  }

  set href(value: string) {
    this.builder = buildUriBuilder(value);
    this.cachedSearchParams = null;
  }

  get protocol(): string {
    return `${this.uri.Scheme}:`;
  }

  set protocol(value: string) {
    this.builder.Scheme = trimLeading(value, ":");
  }

  get username(): string {
    return splitUserInfo(this.uri.UserInfo).username;
  }

  set username(value: string) {
    this.builder.UserName = value;
  }

  get password(): string {
    return splitUserInfo(this.uri.UserInfo).password;
  }

  set password(value: string) {
    this.builder.Password = value;
  }

  get host(): string {
    return this.uri.IsDefaultPort ? this.uri.Host : `${this.uri.Host}:${String(this.uri.Port)}`;
  }

  set host(value: string) {
    const separatorIndex = value.lastIndexOf(":");
    if (separatorIndex > 0) {
      this.builder.Host = value.slice(0, separatorIndex);
      this.builder.Port = parsePort(value.slice(separatorIndex + 1));
      return;
    }

    this.builder.Host = value;
    this.builder.Port = -1;
  }

  get hostname(): string {
    return this.uri.Host;
  }

  set hostname(value: string) {
    this.builder.Host = value;
  }

  get port(): string {
    return this.uri.IsDefaultPort ? "" : String(this.uri.Port);
  }

  set port(value: string) {
    this.builder.Port = value.length === 0 ? -1 : parsePort(value);
  }

  get pathname(): string {
    return this.uri.AbsolutePath;
  }

  set pathname(value: string) {
    this.builder.Path = value.startsWith("/") ? value : `/${value}`;
  }

  get search(): string {
    return this.uri.Query;
  }

  set search(value: string) {
    this.builder.Query = trimLeading(value, "?");
    this.cachedSearchParams = null;
  }

  get searchParams(): URLSearchParams {
    if (this.cachedSearchParams === null) {
      this.cachedSearchParams = new URLSearchParams(this.search);
    }
    return this.cachedSearchParams;
  }

  get hash(): string {
    return this.uri.Fragment;
  }

  set hash(value: string) {
    this.builder.Fragment = trimLeading(value, "#");
  }

  get origin(): string {
    return `${this.protocol}//${this.host}`;
  }

  toString(): string {
    return this.href;
  }

  toJSON(): string {
    return this.href;
  }

  static canParse(input: string, base?: string): boolean {
    try {
      void buildUriBuilder(input, base);
      return true;
    } catch {
      return false;
    }
  }

  static parse(input: string, base?: string): URL | null {
    try {
      return new URL(input, base);
    } catch {
      return null;
    }
  }
}
