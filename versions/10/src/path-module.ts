
import type {} from "./type-bootstrap.ts";

import { Directory, Path } from "@tsonic/dotnet/System.IO.js";
import {
  OSPlatform,
  RuntimeInformation,
} from "@tsonic/dotnet/System.Runtime.InteropServices.js";

export type ParsedPath = {
  root: string;
  dir: string;
  base: string;
  ext: string;
  name: string;
};

const isWindows = (): boolean =>
  RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

const escapeRegexChar = (value: string): string => {
  if ("\\^$|+()[]{}".includes(value)) {
    return `\\${value}`;
  }

  return value;
};

const globToRegex = (value: string): string => {
  let result = "^";

  for (let i = 0; i < value.length; i += 1) {
    const ch = value[i]!;
    if (ch === "*") {
      if (value[i + 1] === "*") {
        result += ".*";
        i += 1;
      } else {
        result += "[^/\\\\]*";
      }
      continue;
    }

    if (ch === "?") {
      result += ".";
      continue;
    }

    result += escapeRegexChar(ch);
  }

  return `${result}$`;
};

export const sep = Path.DirectorySeparatorChar;
export const delimiter = Path.PathSeparator;

export const basename = (value: string, suffix?: string): string => {
  if (value.length === 0) {
    return "";
  }

  const name = Path.GetFileName(value) ?? "";
  if (suffix !== undefined && name.endsWith(suffix)) {
    return name.slice(0, name.length - suffix.length);
  }

  return name;
};

export const dirname = (value: string): string => {
  if (value.length === 0) {
    return ".";
  }

  const result = Path.GetDirectoryName(value);
  return result == null || result.length === 0 ? "." : result;
};

export const extname = (value: string): string => {
  if (value.length === 0) {
    return "";
  }

  return Path.GetExtension(value) ?? "";
};

export const join = (...values: string[]): string => {
  const filtered = values.filter((value) => value.length > 0);
  if (filtered.length === 0) {
    return ".";
  }

  return Path.Combine(...filtered);
};

export const normalize = (value: string): string => {
  if (value.length === 0) {
    return ".";
  }

  return Path.GetFullPath(value);
};

export const resolve = (...values: string[]): string => {
  const filtered = values.filter((value) => value.length > 0);
  if (filtered.length === 0) {
    return Directory.GetCurrentDirectory();
  }

  let current = Directory.GetCurrentDirectory();
  for (const value of filtered) {
    current = Path.IsPathRooted(value) ? value : Path.Combine(current, value);
  }

  return Path.GetFullPath(current);
};

export const isAbsolute = (value: string): boolean => {
  if (value.length === 0) {
    return false;
  }

  return Path.IsPathRooted(value);
};

export const relative = (from: string, to: string): string => {
  const fromPath =
    from.length === 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(from);
  const toPath =
    to.length === 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(to);

  if (fromPath === toPath) {
    return "";
  }

  return Path.GetRelativePath(fromPath, toPath);
};

export const parse = (value: string): ParsedPath => {
  if (value.length === 0) {
    return { root: "", dir: "", base: "", ext: "", name: "" };
  }

  const dir = Path.GetDirectoryName(value) ?? "";
  const base = Path.GetFileName(value) ?? "";
  const ext = Path.GetExtension(value) ?? "";
  const name = ext.length > 0 ? base.slice(0, base.length - ext.length) : base;
  const root = Path.IsPathRooted(value)
    ? (Path.GetPathRoot(value) ?? (isWindows() ? "" : "/"))
    : "";

  return { root, dir, base, ext, name };
};

export const format = (value: ParsedPath): string => {
  if (value.dir.length === 0) {
    return value.base;
  }

  if (value.dir === value.root) {
    return `${value.dir}${value.base}`;
  }

  return Path.Combine(value.dir, value.base);
};

export const matchesGlob = (value: string, pattern: string): boolean => {
  if (value.length === 0 || pattern.length === 0) {
    return false;
  }

  return new RegExp(globToRegex(pattern)).test(value);
};

export const toNamespacedPath = (value: string): string => {
  if (value.length === 0) {
    return value;
  }

  if (!isWindows()) {
    return value;
  }

  const fullPath = Path.GetFullPath(value);
  if (fullPath.startsWith("\\\\?\\") || fullPath.startsWith("\\\\.\\"))
  {
    return fullPath;
  }

  if (fullPath.startsWith("\\\\")) {
    return `\\\\?\\UNC\\${fullPath.slice(2)}`;
  }

  return `\\\\?\\${fullPath}`;
};

export class PathModuleNamespace {
  get sep(): string {
    return sep;
  }

  get delimiter(): string {
    return delimiter;
  }

  basename(value: string, suffix?: string): string {
    return basename(value, suffix);
  }

  dirname(value: string): string {
    return dirname(value);
  }

  extname(value: string): string {
    return extname(value);
  }

  join(...values: string[]): string {
    return join(...values);
  }

  normalize(value: string): string {
    return normalize(value);
  }

  resolve(...values: string[]): string {
    return resolve(...values);
  }

  isAbsolute(value: string): boolean {
    return isAbsolute(value);
  }

  relative(from: string, to: string): string {
    return relative(from, to);
  }

  parse(value: string): ParsedPath {
    return parse(value);
  }

  format(value: ParsedPath): string {
    return format(value);
  }

  matchesGlob(value: string, pattern: string): boolean {
    return matchesGlob(value, pattern);
  }

  toNamespacedPath(value: string): string {
    return toNamespacedPath(value);
  }
}

const namespace = new PathModuleNamespace();

export const posix = namespace;
export const win32 = namespace;
