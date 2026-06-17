import { overloads as O } from "@tsonic/core/lang.js";
import type {} from "./type-bootstrap.ts";

import type { byte, int } from "@tsonic/core/types.js";
import { Convert, DateTimeOffset } from "@tsonic/dotnet/System.js";
import type { DateTime } from "@tsonic/dotnet/System.js";
import { Buffer } from "./buffer/index.ts";
import {
  Directory,
  DirectoryNotFoundException,
  DirectoryInfo,
  File,
  FileAccess,
  FileMode,
  FileNotFoundException,
  FileInfo,
  FileShare,
  FileStream,
  Path,
} from "@tsonic/dotnet/System.IO.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { Encoding } from "@tsonic/dotnet/System.Text.js";
import { Monitor } from "@tsonic/dotnet/System.Threading.js";

const parseEncoding = (value?: string): Encoding => {
  const normalized = value === undefined || value === null ? "utf-8" : value.toLowerCase();
  switch (normalized) {
    case "utf-8":
    case "utf8":
      return Encoding.UTF8;
    case "ascii":
      return Encoding.ASCII;
    case "utf-16":
    case "utf16":
    case "utf16le":
    case "utf-16le":
    case "ucs2":
    case "ucs-2":
      return Encoding.Unicode;
    case "utf-32":
    case "utf32":
      return Encoding.UTF32;
    case "latin1":
    case "binary":
      return Encoding.Latin1;
    default:
      return Encoding.UTF8;
  }
};

const resolveWriteLength = (
  buffer: WritableFsBuffer,
  offset: int,
  length?: int
): int =>
  length === undefined
    ? ((getBufferLength(buffer) - offset) as int)
    : length;

const toUnixMilliseconds = (value: DateTime): number =>
  Convert.ToDouble(new DateTimeOffset(value).ToUnixTimeMilliseconds());

const toJsDate = (value: DateTime): Date =>
  new Date(toUnixMilliseconds(value));

const buildFileStats = (info: FileInfo): Stats =>
  new Stats(
    Convert.ToDouble(info.Length),
    true,
    false,
    toJsDate(info.LastAccessTime),
    toJsDate(info.LastWriteTime),
    toJsDate(info.CreationTime),
    toJsDate(info.CreationTime)
  );

const buildDirectoryStats = (info: DirectoryInfo): Stats =>
  new Stats(
    0,
    false,
    true,
    toJsDate(info.LastAccessTime),
    toJsDate(info.LastWriteTime),
    toJsDate(info.CreationTime),
    toJsDate(info.CreationTime)
  );

export class Dirent {
  name: string;
  path: string;
  directoryEntry: boolean;
  fileEntry: boolean;

  constructor(name: string, path: string, directoryEntry: boolean, fileEntry: boolean) {
    this.name = name;
    this.path = path;
    this.directoryEntry = directoryEntry;
    this.fileEntry = fileEntry;
  }

  isDirectory(): boolean {
    return this.directoryEntry;
  }

  isFile(): boolean {
    return this.fileEntry;
  }
}

export type ReaddirSyncOptions = {
  readonly withFileTypes?: boolean;
};

const deleteDirectoryIfExists = (path: string, recursive: boolean): void => {
  if (Directory.Exists(path)) {
    Directory.Delete(path, recursive);
  }
};

type OpenDescriptor = {
  stream: FileStream;
  appendMode: boolean;
};

type WritableFsBuffer = Buffer | byte[];
const openDescriptors = new Map<int, OpenDescriptor>();
const openDescriptorsSync = {};
let nextDescriptor = 3 as int;

const registerDescriptor = (stream: FileStream, appendMode: boolean): int => {
  Monitor.Enter(openDescriptorsSync);
  try {
    const fd = nextDescriptor;
    nextDescriptor = (nextDescriptor + 1) as int;
    openDescriptors.set(fd, { stream, appendMode });
    return fd;
  } finally {
    Monitor.Exit(openDescriptorsSync);
  }
};

const getDescriptor = (fd: int): OpenDescriptor => {
  Monitor.Enter(openDescriptorsSync);
  try {
    const descriptor = openDescriptors.get(fd);
    if (descriptor === undefined) {
      throw new Error(`Bad file descriptor: ${fd.toString()}`);
    }

    return descriptor;
  } finally {
    Monitor.Exit(openDescriptorsSync);
  }
};

const closeDescriptor = (fd: int): void => {
  let descriptor: OpenDescriptor;
  Monitor.Enter(openDescriptorsSync);
  try {
    const current = openDescriptors.get(fd);
    if (current === undefined) {
      throw new Error(`Bad file descriptor: ${fd.toString()}`);
    }
    openDescriptors.delete(fd);
    descriptor = current;
  } finally {
    Monitor.Exit(openDescriptorsSync);
  }

  descriptor.stream.Flush();
  descriptor.stream.Dispose();
};

const getBufferLength = (buffer: WritableFsBuffer): int => {
  if (buffer === null || buffer === undefined) {
    throw new Error("buffer must not be null");
  }

  return buffer instanceof Buffer ? buffer.length : (buffer.length as int);
};

const validateBufferRange = (
  bufferLength: int,
  offset: int,
  length: int
): void => {
  if (offset < 0 || offset > bufferLength) {
    throw new RangeError(`Invalid offset: ${offset.toString()}`);
  }

  if (length < 0 || offset + length > bufferLength) {
    throw new RangeError(`Invalid length: ${length.toString()}`);
  }
};

const copyByteArrayToBuffer = (
  target: WritableFsBuffer,
  targetOffset: int,
  source: byte[],
  sourceLength: int
): void => {
  if (target instanceof Buffer) {
    const targetBytes = target.buffer;
    for (let index = 0; index < sourceLength; index += 1) {
      targetBytes[targetOffset + index] = source[index]!;
    }
    return;
  }

  for (let index = 0; index < sourceLength; index += 1) {
    target[targetOffset + index] = source[index]!;
  }
};

const copyBufferSliceToByteArray = (
  buffer: WritableFsBuffer,
  offset: int,
  length: int
): byte[] => {
  const result = new List<byte>();

  if (buffer instanceof Buffer) {
    const sourceBytes = buffer.buffer;
    for (let index = 0; index < length; index += 1) {
      result.Add(sourceBytes[offset + index]! as byte);
    }
    return result.ToArray();
  }

  for (let index = 0; index < length; index += 1) {
    result.Add(buffer[offset + index]!);
  }

  return result.ToArray();
};

const createByteArray = (length: int): byte[] => {
  const result = new List<byte>();
  for (let index = 0; index < length; index += 1) {
    result.Add(0 as byte);
  }
  return result.ToArray();
};

const copyDirectoryRecursive = (
  sourcePath: string,
  destinationPath: string
): void => {
  Directory.CreateDirectory(destinationPath);

  for (const filePath of Directory.GetFiles(sourcePath)) {
    const fileName = Path.GetFileName(filePath);
    if (fileName == null || fileName.length === 0) {
      continue;
    }

    File.Copy(filePath, Path.Combine(destinationPath, fileName), true);
  }

  for (const directoryPath of Directory.GetDirectories(sourcePath)) {
    const directoryName = Path.GetFileName(directoryPath);
    if (directoryName == null || directoryName.length === 0) {
      continue;
    }

    copyDirectoryRecursive(
      directoryPath,
      Path.Combine(destinationPath, directoryName)
    );
  }
};

const openStream = (path: string, flags: string): OpenDescriptor => {
  if (path.length === 0) {
    throw new Error("path must not be empty");
  }

  switch (flags) {
    case "r":
    case "rs":
    case "sr":
      return {
        stream: File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
        appendMode: false,
      };
    case "r+":
    case "rs+":
    case "sr+":
      return {
        stream: File.Open(
          path,
          FileMode.Open,
          FileAccess.ReadWrite,
          FileShare.ReadWrite
        ),
        appendMode: false,
      };
    case "w":
      return {
        stream: File.Open(
          path,
          FileMode.Create,
          FileAccess.Write,
          FileShare.ReadWrite
        ),
        appendMode: false,
      };
    case "wx":
    case "xw":
      return {
        stream: File.Open(
          path,
          FileMode.CreateNew,
          FileAccess.Write,
          FileShare.ReadWrite
        ),
        appendMode: false,
      };
    case "w+":
      return {
        stream: File.Open(
          path,
          FileMode.Create,
          FileAccess.ReadWrite,
          FileShare.ReadWrite
        ),
        appendMode: false,
      };
    case "wx+":
    case "xw+":
      return {
        stream: File.Open(
          path,
          FileMode.CreateNew,
          FileAccess.ReadWrite,
          FileShare.ReadWrite
        ),
        appendMode: false,
      };
    case "a": {
      const stream = File.Open(
        path,
        FileMode.OpenOrCreate,
        FileAccess.Write,
        FileShare.ReadWrite
      );
      stream.Position = stream.Length;
      return { stream, appendMode: true };
    }
    case "ax":
    case "xa": {
      const stream = File.Open(
        path,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.ReadWrite
      );
      return { stream, appendMode: true };
    }
    case "a+": {
      const stream = File.Open(
        path,
        FileMode.OpenOrCreate,
        FileAccess.ReadWrite,
        FileShare.ReadWrite
      );
      stream.Position = stream.Length;
      return { stream, appendMode: true };
    }
    case "ax+":
    case "xa+": {
      const stream = File.Open(
        path,
        FileMode.CreateNew,
        FileAccess.ReadWrite,
        FileShare.ReadWrite
      );
      return { stream, appendMode: true };
    }
    default:
      throw new Error(`Unknown file open flag: ${flags}`);
  }
};

export class MkdirOptions {
  recursive?: boolean;
  mode?: int;
}

export class Stats {
  mode: int;
  atimeMs: number;
  mtimeMs: number;
  ctimeMs: number;
  birthtimeMs: number;
  size: number;
  isFile: boolean;
  isDirectory: boolean;
  atime: Date;
  mtime: Date;
  ctime: Date;
  birthtime: Date;

  constructor(
    size: number,
    isFile: boolean,
    isDirectory: boolean,
    atime: Date,
    mtime: Date,
    ctime: Date,
    birthtime: Date
  ) {
    this.size = size;
    this.isFile = isFile;
    this.isDirectory = isDirectory;
    this.atime = atime;
    this.mtime = mtime;
    this.ctime = ctime;
    this.birthtime = birthtime;
    this.mode = 0 as int;
    this.atimeMs = atime.getTime();
    this.mtimeMs = mtime.getTime();
    this.ctimeMs = ctime.getTime();
    this.birthtimeMs = birthtime.getTime();
  }

  IsFile(): boolean {
    return this.isFile;
  }

  IsDirectory(): boolean {
    return this.isDirectory;
  }

  IsSymbolicLink(): boolean {
    return false;
  }

  IsBlockDevice(): boolean {
    return false;
  }

  IsCharacterDevice(): boolean {
    return false;
  }

  IsFIFO(): boolean {
    return false;
  }

  IsSocket(): boolean {
    return false;
  }
}

export const accessSync = (path: string, _mode: int = 0 as int): void => {
  if (!File.Exists(path) && !Directory.Exists(path)) {
    throw new FileNotFoundException(`No such file or directory: ${path}`);
  }
};

export const access = async (
  path: string,
  mode: int = 0 as int
): Promise<void> => {
  accessSync(path, mode);
};

export const appendFileSync = (
  path: string,
  data: string,
  encoding: string = "utf-8"
): void => {
  File.AppendAllText(path, data, parseEncoding(encoding));
};

export const appendFile = async (
  path: string,
  data: string,
  encoding: string = "utf-8"
): Promise<void> => {
  await File.AppendAllTextAsync(path, data, parseEncoding(encoding));
};

export const existsSync = (path: string): boolean =>
  File.Exists(path) || Directory.Exists(path);

const mkdirSyncImpl = (path: string, recursive: boolean): void => {
  if (recursive) {
    Directory.CreateDirectory(path);
    return;
  }

  const parent = Path.GetDirectoryName(path);
  if (
    parent != null &&
    parent.length > 0 &&
    !Directory.Exists(parent)
  ) {
    throw new DirectoryNotFoundException(
      `Parent directory does not exist: ${parent}`
    );
  }

  Directory.CreateDirectory(path);
};

export function mkdirSync(path: string): void;
export function mkdirSync(path: string, recursive: boolean): void;
export function mkdirSync(
  path: string,
  options: MkdirOptions
): void;
export function mkdirSync(_path: any, _options?: any): any {
  throw new Error("Unreachable overload stub");
}

function mkdirSync_path(path: string): void {
  mkdirSyncImpl(path, false);
}

function mkdirSync_recursive(path: string, recursive: boolean): void {
  mkdirSyncImpl(path, recursive);
}

function mkdirSync_options(
  path: string,
  options: MkdirOptions
): void {
  mkdirSyncImpl(path, options.recursive ?? false);
}

export function mkdir(path: string): Promise<void>;
export function mkdir(path: string, recursive: boolean): Promise<void>;
export function mkdir(
  path: string,
  options: MkdirOptions
): Promise<void>;
export async function mkdir(_path: any, _options?: any): Promise<any> {
  throw new Error("Unreachable overload stub");
}

async function mkdir_path(path: string): Promise<void> {
  mkdirSyncImpl(path, false);
}

async function mkdir_recursive(path: string, recursive: boolean): Promise<void> {
  mkdirSyncImpl(path, recursive);
}

async function mkdir_options(
  path: string,
  options: MkdirOptions
): Promise<void> {
  mkdirSyncImpl(path, options.recursive ?? false);
}

export const copyFileSync = (sourcePath: string, destinationPath: string): void => {
  File.Copy(sourcePath, destinationPath, true);
};

export const copyFile = async (
  sourcePath: string,
  destinationPath: string
): Promise<void> => {
  copyFileSync(sourcePath, destinationPath);
};

export const chmodSync = (path: string, mode: int): void => {
  if (File.Exists(path)) {
    const fileInfo = new FileInfo(path);
    fileInfo.IsReadOnly = (mode & (0x80 as int)) === (0 as int);
    return;
  }

  if (Directory.Exists(path)) {
    return;
  }

  throw new FileNotFoundException(`No such file or directory: ${path}`);
};

export const chmod = async (path: string, mode: int): Promise<void> => {
  chmodSync(path, mode);
};

export const closeSync = (fd: int): void => {
  closeDescriptor(fd);
};

export const close = async (fd: int): Promise<void> => {
  closeSync(fd);
};

export const cpSync = (
  sourcePath: string,
  destinationPath: string,
  recursive: boolean = false
): void => {
  if (File.Exists(sourcePath)) {
    File.Copy(sourcePath, destinationPath, true);
    return;
  }

  if (Directory.Exists(sourcePath)) {
    if (!recursive) {
      throw new Error(
        `Cannot copy directory without recursive flag: ${sourcePath}`
      );
    }

    copyDirectoryRecursive(sourcePath, destinationPath);
    return;
  }

  throw new FileNotFoundException(`No such file or directory: ${sourcePath}`);
};

export const cp = async (
  sourcePath: string,
  destinationPath: string,
  recursive: boolean = false
): Promise<void> => {
  cpSync(sourcePath, destinationPath, recursive);
};

export const openSync = (
  path: string,
  flags: string,
  _mode?: int
): int => {
  const descriptor = openStream(path, flags);
  return registerDescriptor(descriptor.stream, descriptor.appendMode);
};

export const open = async (
  path: string,
  flags: string,
  mode?: int
): Promise<int> => openSync(path, flags, mode);

export const fstatSync = (fd: int): Stats => {
  const descriptor = getDescriptor(fd);
  const fileInfo = new FileInfo(descriptor.stream.Name);
  if (!fileInfo.Exists) {
    throw new FileNotFoundException(`No such file: ${descriptor.stream.Name}`);
  }

  return buildFileStats(fileInfo);
};

export const fstat = async (fd: int): Promise<Stats> => fstatSync(fd);

export const readSync = (
  fd: int,
  buffer: WritableFsBuffer,
  offset: int,
  length: int,
  position: int | null = null
): int => {
  const descriptor = getDescriptor(fd);
  validateBufferRange(getBufferLength(buffer), offset, length);

  if (position !== null && descriptor.stream.CanSeek) {
    descriptor.stream.Position = position;
  }

  const scratch = createByteArray(length);
  const bytesRead = descriptor.stream.Read(scratch, 0 as int, length);
  copyByteArrayToBuffer(buffer, offset, scratch, bytesRead);
  return bytesRead as int;
};

export const read = async (
  fd: int,
  buffer: WritableFsBuffer,
  offset: int,
  length: int,
  position: int | null = null
): Promise<int> => readSync(fd, buffer, offset, length, position);

export const readlinkSync = (path: string): string => {
  const fileTarget = new FileInfo(path).LinkTarget;
  if (fileTarget != null) {
    return fileTarget;
  }

  const directoryTarget = new DirectoryInfo(path).LinkTarget;
  if (directoryTarget != null) {
    return directoryTarget;
  }

  if (File.Exists(path) || Directory.Exists(path)) {
    throw new Error(`Not a symbolic link: ${path}`);
  }

  throw new FileNotFoundException(`No such file or directory: ${path}`);
};

export const readlink = async (path: string): Promise<string> =>
  readlinkSync(path);

export const symlinkSync = (
  target: string,
  path: string,
  type?: string
): void => {
  const isDirectoryTarget =
    type === "dir" ||
    type === "junction" ||
    Directory.Exists(target);

  if (isDirectoryTarget) {
    Directory.CreateSymbolicLink(path, target);
    return;
  }

  File.CreateSymbolicLink(path, target);
};

export const symlink = async (
  target: string,
  path: string,
  type?: string
): Promise<void> => {
  symlinkSync(target, path, type);
};

export const truncateSync = (path: string, length: number = 0): void => {
  const stream = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
  try {
    stream.SetLength(Convert.ToInt64(length));
  } finally {
    stream.Dispose();
  }
};

export const truncate = async (
  path: string,
  length: number = 0
): Promise<void> => {
  truncateSync(path, length);
};

export function writeSync(
  fd: int,
  buffer: WritableFsBuffer,
  offset: int,
  length: int,
  position: int | null
): int;
export function writeSync(
  fd: int,
  data: string,
  position?: int | null,
  encoding?: string
): int;
export function writeSync(
  fd: any,
  bufferOrData: any,
  offsetOrPosition?: any,
  lengthOrEncoding?: any,
  position?: any
): any {
  throw new Error("Unreachable overload stub");
}

function writeSync_buffer(
  fd: int,
  buffer: WritableFsBuffer,
  offset: int,
  length: int,
  position: int | null
): int {
  const descriptor = getDescriptor(fd);
  validateBufferRange(getBufferLength(buffer), offset, length);

  if (descriptor.appendMode && descriptor.stream.CanSeek) {
    descriptor.stream.Position = descriptor.stream.Length;
  } else if (position !== null && position !== undefined && descriptor.stream.CanSeek) {
    descriptor.stream.Position = position;
  }

  const bytes = copyBufferSliceToByteArray(buffer, offset, length);
  descriptor.stream.Write(bytes, 0 as int, length);
  return length;
}

function writeSync_text(
  fd: int,
  data: string,
  position?: int | null,
  encoding?: string
): int {
  const descriptor = getDescriptor(fd);
  const bytes = parseEncoding(encoding ?? "utf-8").GetBytes(data);
  if (descriptor.appendMode && descriptor.stream.CanSeek) {
    descriptor.stream.Position = descriptor.stream.Length;
  } else if (position != null && descriptor.stream.CanSeek) {
    descriptor.stream.Position = position;
  }
  descriptor.stream.Write(bytes, 0 as int, bytes.length as int);
  return bytes.length as int;
}

export function write(
  fd: int,
  buffer: WritableFsBuffer,
  offset: int,
  length: int,
  position: int | null
): Promise<int>;
export function write(
  fd: int,
  data: string,
  position?: int | null,
  encoding?: string
): Promise<int>;
export async function write(
  fd: any,
  bufferOrData: any,
  offsetOrPosition?: any,
  lengthOrEncoding?: any,
  position?: any
): Promise<any> {
  throw new Error("Unreachable overload stub");
}

async function write_buffer(
  fd: int,
  buffer: WritableFsBuffer,
  offset: int,
  length: int,
  position: int | null
): Promise<int> {
  return writeSync_buffer(fd, buffer, offset, length, position);
}

async function write_text(
  fd: int,
  data: string,
  position?: int | null,
  encoding?: string
): Promise<int> {
  return writeSync_text(fd, data, position, encoding);
}

export function readFileSync(path: string): Buffer;
export function readFileSync(path: string, encoding: string): string;
export function readFileSync(_path: any, _encoding?: any): any {
  throw new Error("Unreachable overload stub");
}

function readFileSync_buffer(path: string): Buffer {
  return Buffer.fromBytes(File.ReadAllBytes(path));
}

function readFileSync_text(path: string, encoding: string): string {
  return File.ReadAllText(path, parseEncoding(encoding));
}

export const readFileSyncBytes = (path: string): byte[] =>
  File.ReadAllBytes(path);

export function readFile(path: string): Promise<Buffer>;
export function readFile(path: string, encoding: string): Promise<string>;
export async function readFile(_path: any, _encoding?: any): Promise<any> {
  throw new Error("Unreachable overload stub");
}

async function readFile_buffer(path: string): Promise<Buffer> {
  return Buffer.fromBytes(await File.ReadAllBytesAsync(path));
}

async function readFile_text(path: string, encoding: string): Promise<string> {
  return await File.ReadAllTextAsync(path, parseEncoding(encoding));
}

export const readFileBytes = async (path: string): Promise<byte[]> =>
  await File.ReadAllBytesAsync(path);

O(mkdirSync_path).family(mkdirSync);
O(mkdirSync_recursive).family(mkdirSync);
O(mkdirSync_options).family(mkdirSync);
O(mkdir_path).family(mkdir);
O(mkdir_recursive).family(mkdir);
O(mkdir_options).family(mkdir);
O(readFileSync_buffer).family(readFileSync);
O(readFileSync_text).family(readFileSync);
O(readFile_buffer).family(readFile);
O(readFile_text).family(readFile);

export function readdirSync(path: string): string[];
export function readdirSync(
  path: string,
  options: { readonly withFileTypes: true },
): Dirent[];
export function readdirSync(_path: any, _options?: any): any {
  throw new Error("Unreachable overload stub");
}

function readdirSync_names(path: string): string[] {
  return Directory.GetFileSystemEntries(path)
    .map((entry) => Path.GetFileName(entry) ?? "")
    .filter((entry) => entry.length > 0);
}

function readdirSync_dirents(
  path: string,
  _options: { readonly withFileTypes: true },
): Dirent[] {
  return Directory.GetFileSystemEntries(path)
    .map((entry) => {
      const name = Path.GetFileName(entry) ?? "";
      return new Dirent(
        name,
        entry,
        Directory.Exists(entry),
        File.Exists(entry),
      );
    })
    .filter((entry) => entry.name.length > 0);
}

O(readdirSync_names).family(readdirSync);
O(readdirSync_dirents).family(readdirSync);

export const readdir = async (path: string): Promise<string[]> =>
  readdirSync(path);

export const realpathSync = (path: string): string => Path.GetFullPath(path);

export const realpath = async (path: string): Promise<string> =>
  realpathSync(path);

export const renameSync = (oldPath: string, newPath: string): void => {
  if (File.Exists(oldPath)) {
    File.Move(oldPath, newPath);
    return;
  }

  if (Directory.Exists(oldPath)) {
    Directory.Move(oldPath, newPath);
    return;
  }

  throw new FileNotFoundException(`No such file or directory: ${oldPath}`);
};

export const rename = async (
  oldPath: string,
  newPath: string
): Promise<void> => {
  renameSync(oldPath, newPath);
};

export const rmSync = (path: string, recursive: boolean = false): void => {
  if (File.Exists(path)) {
    File.Delete(path);
    return;
  }

  deleteDirectoryIfExists(path, recursive);
};

export const rm = async (
  path: string,
  recursive: boolean = false
): Promise<void> => {
  rmSync(path, recursive);
};

export const statSync = (path: string): Stats => {
  const fileInfo = new FileInfo(path);
  if (fileInfo.Exists) {
    return buildFileStats(fileInfo);
  }

  const directoryInfo = new DirectoryInfo(path);
  if (directoryInfo.Exists) {
    return buildDirectoryStats(directoryInfo);
  }

  throw new FileNotFoundException(`No such file or directory: ${path}`);
};

export const stat = async (path: string): Promise<Stats> => statSync(path);

export const unlinkSync = (path: string): void => {
  File.Delete(path);
};

export const unlink = async (path: string): Promise<void> => {
  unlinkSync(path);
};

export const rmdirSync = (path: string, recursive: boolean = false): void => {
  Directory.Delete(path, recursive);
};

export const rmdir = async (
  path: string,
  recursive: boolean = false
): Promise<void> => {
  rmdirSync(path, recursive);
};

export const writeFileSync = (
  path: string,
  data: string,
  encoding: string = "utf-8"
): void => {
  File.WriteAllText(path, data, parseEncoding(encoding));
};

export const writeFile = async (
  path: string,
  data: string,
  encoding: string = "utf-8"
): Promise<void> => {
  await File.WriteAllTextAsync(path, data, parseEncoding(encoding));
};

export const writeFileSyncBytes = (path: string, data: byte[]): void => {
  File.WriteAllBytes(path, data);
};

export const writeFileBytes = async (
  path: string,
  data: byte[]
): Promise<void> => {
  await File.WriteAllBytesAsync(path, data);
};

export class FsPromises {
  access(path: string, mode: int = 0 as int): Promise<void> {
    return access(path, mode);
  }

  chmod(path: string, mode: int): Promise<void> {
    return chmod(path, mode);
  }

  close(fd: int): Promise<void> {
    return close(fd);
  }

  copyFile(sourcePath: string, destinationPath: string): Promise<void> {
    return copyFile(sourcePath, destinationPath);
  }

  cp(
    sourcePath: string,
    destinationPath: string,
    recursive: boolean = false
  ): Promise<void> {
    return cp(sourcePath, destinationPath, recursive);
  }

  fstat(fd: int): Promise<Stats> {
    return fstat(fd);
  }

  appendFile(
    path: string,
    data: string,
    encoding: string = "utf-8"
  ): Promise<void> {
    return appendFile(path, data, encoding);
  }

  mkdir(path: string): Promise<void>;
  mkdir(path: string, recursive: boolean): Promise<void>;
  mkdir(
    path: string,
    options: MkdirOptions
  ): Promise<void>;
  mkdir(_path: any, _options?: any): any {
    throw new Error("Unreachable overload stub");
  }

  mkdir_path(path: string): Promise<void> {
    return mkdir_path(path);
  }

  mkdir_recursive(path: string, recursive: boolean): Promise<void> {
    return mkdir_recursive(path, recursive);
  }

  mkdir_options(
    path: string,
    options: MkdirOptions
  ): Promise<void> {
    return mkdir_options(path, options);
  }

  open(path: string, flags: string, mode?: int): Promise<int> {
    return open(path, flags, mode);
  }

  read(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null = null
  ): Promise<int> {
    return read(fd, buffer, offset, length, position);
  }

  readFile(path: string): Promise<Buffer>;
  readFile(path: string, encoding: string): Promise<string>;
  readFile(_path: any, _encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  readFile_buffer(path: string): Promise<Buffer> {
    return readFile_buffer(path);
  }

  readFile_text(path: string, encoding: string): Promise<string> {
    return readFile_text(path, encoding);
  }

  readFileBytes(path: string): Promise<byte[]> {
    return readFileBytes(path);
  }

  readdir(path: string): Promise<string[]> {
    return readdir(path);
  }

  realpath(path: string): Promise<string> {
    return realpath(path);
  }

  readlink(path: string): Promise<string> {
    return readlink(path);
  }

  rename(oldPath: string, newPath: string): Promise<void> {
    return rename(oldPath, newPath);
  }

  rm(path: string, recursive: boolean = false): Promise<void> {
    return rm(path, recursive);
  }

  rmdir(path: string, recursive: boolean = false): Promise<void> {
    return rmdir(path, recursive);
  }

  stat(path: string): Promise<Stats> {
    return stat(path);
  }

  symlink(
    target: string,
    path: string,
    type?: string
  ): Promise<void> {
    return symlink(target, path, type);
  }

  truncate(path: string, length: number = 0): Promise<void> {
    return truncate(path, length);
  }

  unlink(path: string): Promise<void> {
    return unlink(path);
  }

  writeFile(
    path: string,
    data: string,
    encoding: string = "utf-8"
  ): Promise<void> {
    return writeFile(path, data, encoding);
  }

  write(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null
  ): Promise<int>;
  write(
    fd: int,
    data: string,
    position?: int | null,
    encoding?: string
  ): Promise<int>;
  write(
    _fd: any,
    _bufferOrData: any,
    _offsetOrPosition?: any,
    _lengthOrEncoding?: any,
    _position?: any
  ): Promise<any> {
    throw new Error("Unreachable overload stub");
  }

  write_buffer(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null
  ): Promise<int> {
    return write_buffer(fd, buffer, offset, length, position);
  }

  write_text(
    fd: int,
    data: string,
    position?: int | null,
    encoding?: string
  ): Promise<int> {
    return write_text(fd, data, position, encoding);
  }

  writeFileBytes(path: string, data: byte[]): Promise<void> {
    return writeFileBytes(path, data);
  }
}

export class FsModuleNamespace {
  promises: FsPromises = new FsPromises();

  access(path: string, mode: int = 0 as int): Promise<void> {
    return access(path, mode);
  }

  accessSync(path: string, mode: int = 0 as int): void {
    return accessSync(path, mode);
  }

  chmod(path: string, mode: int): Promise<void> {
    return chmod(path, mode);
  }

  chmodSync(path: string, mode: int): void {
    return chmodSync(path, mode);
  }

  appendFile(
    path: string,
    data: string,
    encoding: string = "utf-8"
  ): Promise<void> {
    return appendFile(path, data, encoding);
  }

  appendFileSync(
    path: string,
    data: string,
    encoding: string = "utf-8"
  ): void {
    return appendFileSync(path, data, encoding);
  }

  copyFile(sourcePath: string, destinationPath: string): Promise<void> {
    return copyFile(sourcePath, destinationPath);
  }

  copyFileSync(sourcePath: string, destinationPath: string): void {
    return copyFileSync(sourcePath, destinationPath);
  }

  cp(
    sourcePath: string,
    destinationPath: string,
    recursive: boolean = false
  ): Promise<void> {
    return cp(sourcePath, destinationPath, recursive);
  }

  cpSync(
    sourcePath: string,
    destinationPath: string,
    recursive: boolean = false
  ): void {
    return cpSync(sourcePath, destinationPath, recursive);
  }

  close(fd: int): Promise<void> {
    return close(fd);
  }

  closeSync(fd: int): void {
    return closeSync(fd);
  }

  fstat(fd: int): Promise<Stats> {
    return fstat(fd);
  }

  fstatSync(fd: int): Stats {
    return fstatSync(fd);
  }

  existsSync(path: string): boolean {
    return existsSync(path);
  }

  mkdir(path: string): Promise<void>;
  mkdir(path: string, recursive: boolean): Promise<void>;
  mkdir(
    path: string,
    options: MkdirOptions
  ): Promise<void>;
  mkdir(_path: any, _options?: any): any {
    throw new Error("Unreachable overload stub");
  }

  mkdir_path(path: string): Promise<void> {
    return mkdir_path(path);
  }

  mkdir_recursive(path: string, recursive: boolean): Promise<void> {
    return mkdir_recursive(path, recursive);
  }

  mkdir_options(
    path: string,
    options: MkdirOptions
  ): Promise<void> {
    return mkdir_options(path, options);
  }

  mkdirSync(path: string): void;
  mkdirSync(path: string, recursive: boolean): void;
  mkdirSync(
    path: string,
    options: MkdirOptions
  ): void;
  mkdirSync(_path: any, _options?: any): any {
    throw new Error("Unreachable overload stub");
  }

  mkdirSync_path(path: string): void {
    return mkdirSync_path(path);
  }

  mkdirSync_recursive(path: string, recursive: boolean): void {
    return mkdirSync_recursive(path, recursive);
  }

  mkdirSync_options(
    path: string,
    options: MkdirOptions
  ): void {
    return mkdirSync_options(path, options);
  }

  open(path: string, flags: string, mode?: int): Promise<int> {
    return open(path, flags, mode);
  }

  openSync(path: string, flags: string, mode?: int): int {
    return openSync(path, flags, mode);
  }

  read(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null = null
  ): Promise<int> {
    return read(fd, buffer, offset, length, position);
  }

  readSync(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null = null
  ): int {
    return readSync(fd, buffer, offset, length, position);
  }

  readFile(path: string): Promise<Buffer>;
  readFile(path: string, encoding: string): Promise<string>;
  readFile(_path: any, _encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  readFile_buffer(path: string): Promise<Buffer> {
    return readFile_buffer(path);
  }

  readFile_text(path: string, encoding: string): Promise<string> {
    return readFile_text(path, encoding);
  }

  readFileBytes(path: string): Promise<byte[]> {
    return readFileBytes(path);
  }

  readFileSync(path: string): Buffer;
  readFileSync(path: string, encoding: string): string;
  readFileSync(_path: any, _encoding?: any): any {
    throw new Error("Unreachable overload stub");
  }

  readFileSync_buffer(path: string): Buffer {
    return readFileSync_buffer(path);
  }

  readFileSync_text(path: string, encoding: string): string {
    return readFileSync_text(path, encoding);
  }

  readFileSyncBytes(path: string): byte[] {
    return readFileSyncBytes(path);
  }

  readdir(path: string): Promise<string[]> {
    return readdir(path);
  }

  readdirSync(path: string): string[] {
    return readdirSync(path);
  }

  realpath(path: string): Promise<string> {
    return realpath(path);
  }

  realpathSync(path: string): string {
    return realpathSync(path);
  }

  readlink(path: string): Promise<string> {
    return readlink(path);
  }

  readlinkSync(path: string): string {
    return readlinkSync(path);
  }

  rename(oldPath: string, newPath: string): Promise<void> {
    return rename(oldPath, newPath);
  }

  renameSync(oldPath: string, newPath: string): void {
    return renameSync(oldPath, newPath);
  }

  rm(path: string, recursive: boolean = false): Promise<void> {
    return rm(path, recursive);
  }

  rmSync(path: string, recursive: boolean = false): void {
    return rmSync(path, recursive);
  }

  rmdir(path: string, recursive: boolean = false): Promise<void> {
    return rmdir(path, recursive);
  }

  rmdirSync(path: string, recursive: boolean = false): void {
    return rmdirSync(path, recursive);
  }

  stat(path: string): Promise<Stats> {
    return stat(path);
  }

  statSync(path: string): Stats {
    return statSync(path);
  }

  symlink(
    target: string,
    path: string,
    type?: string
  ): Promise<void> {
    return symlink(target, path, type);
  }

  symlinkSync(target: string, path: string, type?: string): void {
    return symlinkSync(target, path, type);
  }

  truncate(path: string, length: number = 0): Promise<void> {
    return truncate(path, length);
  }

  truncateSync(path: string, length: number = 0): void {
    return truncateSync(path, length);
  }

  unlink(path: string): Promise<void> {
    return unlink(path);
  }

  unlinkSync(path: string): void {
    return unlinkSync(path);
  }

  writeFile(
    path: string,
    data: string,
    encoding: string = "utf-8"
  ): Promise<void> {
    return writeFile(path, data, encoding);
  }

  write(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null
  ): Promise<int>;
  write(
    fd: int,
    data: string,
    position?: int | null,
    encoding?: string
  ): Promise<int>;
  write(
    _fd: any,
    _bufferOrData: any,
    _offsetOrPosition?: any,
    _lengthOrEncoding?: any,
    _position?: any
  ): Promise<any> {
    throw new Error("Unreachable overload stub");
  }

  write_buffer(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null
  ): Promise<int> {
    return write_buffer(fd, buffer, offset, length, position);
  }

  write_text(
    fd: int,
    data: string,
    position?: int | null,
    encoding?: string
  ): Promise<int> {
    return write_text(fd, data, position, encoding);
  }

  writeSync(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null
  ): int;
  writeSync(
    fd: int,
    data: string,
    position?: int | null,
    encoding?: string
  ): int;
  writeSync(
    _fd: any,
    _bufferOrData: any,
    _offsetOrPosition?: any,
    _lengthOrEncoding?: any,
    _position?: any
  ): any {
    throw new Error("Unreachable overload stub");
  }

  writeSync_buffer(
    fd: int,
    buffer: WritableFsBuffer,
    offset: int,
    length: int,
    position: int | null
  ): int {
    return writeSync_buffer(fd, buffer, offset, length, position);
  }

  writeSync_text(
    fd: int,
    data: string,
    position?: int | null,
    encoding?: string
  ): int {
    return writeSync_text(fd, data, position, encoding);
  }

  writeFileBytes(path: string, data: byte[]): Promise<void> {
    return writeFileBytes(path, data);
  }

  writeFileSync(
    path: string,
    data: string,
    encoding: string = "utf-8"
  ): void {
    return writeFileSync(path, data, encoding);
  }

  writeFileSyncBytes(path: string, data: byte[]): void {
    return writeFileSyncBytes(path, data);
  }
}

export const promises: FsPromises = new FsPromises();
export const fs: FsModuleNamespace = new FsModuleNamespace();

O<FsPromises>().method(x => x.mkdir_path).family(x => x.mkdir);
O<FsPromises>().method(x => x.mkdir_recursive).family(x => x.mkdir);
O<FsPromises>().method(x => x.mkdir_options).family(x => x.mkdir);
O<FsPromises>().method(x => x.readFile_buffer).family(x => x.readFile);
O<FsPromises>().method(x => x.readFile_text).family(x => x.readFile);
O<FsPromises>().method(x => x.write_buffer).family(x => x.write);
O<FsPromises>().method(x => x.write_text).family(x => x.write);
O<FsModuleNamespace>().method(x => x.mkdir_path).family(x => x.mkdir);
O<FsModuleNamespace>().method(x => x.mkdir_recursive).family(x => x.mkdir);
O<FsModuleNamespace>().method(x => x.mkdir_options).family(x => x.mkdir);
O<FsModuleNamespace>().method(x => x.mkdirSync_path).family(x => x.mkdirSync);
O<FsModuleNamespace>().method(x => x.mkdirSync_recursive).family(x => x.mkdirSync);
O<FsModuleNamespace>().method(x => x.mkdirSync_options).family(x => x.mkdirSync);
O<FsModuleNamespace>().method(x => x.readFile_buffer).family(x => x.readFile);
O<FsModuleNamespace>().method(x => x.readFile_text).family(x => x.readFile);
O<FsModuleNamespace>().method(x => x.readFileSync_buffer).family(x => x.readFileSync);
O<FsModuleNamespace>().method(x => x.readFileSync_text).family(x => x.readFileSync);
O<FsModuleNamespace>().method(x => x.write_buffer).family(x => x.write);
O<FsModuleNamespace>().method(x => x.write_text).family(x => x.write);
O<FsModuleNamespace>().method(x => x.writeSync_buffer).family(x => x.writeSync);
O<FsModuleNamespace>().method(x => x.writeSync_text).family(x => x.writeSync);
O(writeSync_buffer).family(writeSync);
O(writeSync_text).family(writeSync);
O(write_buffer).family(write);
O(write_text).family(write);
