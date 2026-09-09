import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { readFileSync, readdirSync } from "node:fs";
import { dirname, join, relative, resolve, sep } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const nativeRoot = join(repositoryRoot, "csharp/src/Tsonic.CSharp.Node");
const filesystemRoot = join(nativeRoot, "fs");
const packagedFilesystemPrefix = "csharp/src/Tsonic.CSharp.Node/fs/";
const expectedFilesystemSources = [
  "callbacks.cs",
  "constants.cs",
  "descriptor/FileDescriptorManager.cs",
  "descriptor/VectorIOResults.cs",
  "descriptor/close.cs",
  "descriptor/closeSync.cs",
  "descriptor/fstat.cs",
  "descriptor/fstatSync.cs",
  "descriptor/open.cs",
  "descriptor/openSync.cs",
  "descriptor/operations.cs",
  "descriptor/read.cs",
  "descriptor/readSync.cs",
  "descriptor/write.cs",
  "descriptor/writeSync.cs",
  "directory/DirectoryModels.cs",
  "directory/MakeDirectoryOptions.cs",
  "directory/RmOptions.cs",
  "directory/copyDirectory.cs",
  "directory/cp.cs",
  "directory/cpSync.cs",
  "directory/mkdir.cs",
  "directory/mkdirSync.cs",
  "directory/numericOptions.cs",
  "directory/operations.cs",
  "directory/readdir.cs",
  "directory/readdirSync.cs",
  "directory/rm.cs",
  "directory/rmSync.cs",
  "directory/rmdir.cs",
  "directory/rmdirSync.cs",
  "file/appendFile.cs",
  "file/appendFileSync.cs",
  "file/copyFile.cs",
  "file/copyFileSync.cs",
  "file/encoding.cs",
  "file/link.cs",
  "file/readFile.cs",
  "file/readFileBytes.cs",
  "file/readFileSync.cs",
  "file/readFileSyncBytes.cs",
  "file/readlink.cs",
  "file/readlinkSync.cs",
  "file/realpath.cs",
  "file/realpathSync.cs",
  "file/rename.cs",
  "file/renameSync.cs",
  "file/symlink.cs",
  "file/symlinkSync.cs",
  "file/truncate.cs",
  "file/truncateSync.cs",
  "file/unlink.cs",
  "file/unlinkSync.cs",
  "file/writeFile.cs",
  "file/writeFileBytes.cs",
  "file/writeFileSync.cs",
  "file/writeFileSyncBytes.cs",
  "fs.cs",
  "metadata/StatTime.cs",
  "metadata/access.cs",
  "metadata/accessSync.cs",
  "metadata/chmod.cs",
  "metadata/chmodSync.cs",
  "metadata/existsSync.cs",
  "metadata/operations.cs",
  "metadata/stat.cs",
  "metadata/statSync.cs",
  "metadata/stats.cs",
  "promises.cs",
  "stream/FsStreamOpenOptions.cs",
  "stream/StreamOptions.cs",
  "stream/Streams.cs",
  "stream/createStreams.cs",
  "watch/WatchModels.cs",
  "watch/Watchers.cs",
  "watch/watch.cs",
];

test("native filesystem sources retain their domain inventory and namespace", () => {
  const paths = sourceFiles(filesystemRoot);
  assertFilesystemInventory(paths.map((path) => toPosix(relative(filesystemRoot, path))));

  for (const path of paths) {
    const source = readFileSync(path, "utf8");
    assert.match(source, /^namespace Tsonic\.CSharp\.Node;$/mu, path);
    for (const declaration of source.matchAll(/^public static (?:partial )?class fs$/gmu)) {
      assert.equal(declaration[0], "public static partial class fs", path);
    }
  }
});

test("native filesystem compile items include every domain source exactly once", () => {
  const inventory = commandJson("dotnet", [
    "msbuild",
    join(nativeRoot, "Tsonic.CSharp.Node.csproj"),
    "-nologo",
    "-verbosity:quiet",
    "-getItem:Compile",
  ]);
  assertFilesystemInventory(
    inventory.Items.Compile
      .map((item) => toPosix(relative(nativeRoot, item.FullPath)))
      .filter((path) => path.startsWith("fs/") && path.endsWith(".cs"))
      .map((path) => path.slice("fs/".length)),
  );
});

test("native filesystem npm artifact includes every domain source exactly once", () => {
  const packages = commandJson("npm", [
    "pack",
    "--dry-run",
    "--ignore-scripts",
    "--json",
  ]);
  assert.equal(packages.length, 1);
  assertFilesystemInventory(
    packages[0].files
      .map((file) => file.path)
      .filter((path) => path.startsWith(packagedFilesystemPrefix) && path.endsWith(".cs"))
      .map((path) => path.slice(packagedFilesystemPrefix.length)),
  );
});

test("native filesystem inventory rejects omitted, duplicate and flattened sources", () => {
  const nestedSource = "metadata/stats.cs";
  const omitted = expectedFilesystemSources.filter((path) => path !== nestedSource);
  for (const invalid of [
    omitted,
    [...expectedFilesystemSources, nestedSource],
    [...omitted, "stats.cs"],
  ]) {
    assert.throws(() => assertFilesystemInventory(invalid), { code: "ERR_ASSERTION" });
  }
});

function assertFilesystemInventory(paths) {
  assert.deepEqual([...paths].sort(), expectedFilesystemSources);
}

function sourceFiles(directory) {
  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const path = join(directory, entry.name);
    return entry.isDirectory()
      ? sourceFiles(path)
      : entry.isFile() && entry.name.endsWith(".cs")
        ? [path]
        : [];
  });
}

function commandJson(command, args) {
  const result = spawnSync(command, args, {
    cwd: repositoryRoot,
    encoding: "utf8",
    env: {
      ...process.env,
      npm_config_cache: join(repositoryRoot, ".temp/npm-cache"),
    },
    timeout: 120_000,
    maxBuffer: 16 * 1024 * 1024,
  });
  assert.equal(result.error, undefined, `${command}: ${result.error?.message}`);
  assert.equal(result.status, 0, `${command}: ${result.stdout}\n${result.stderr}`);
  return JSON.parse(result.stdout);
}

function toPosix(path) {
  return path.split(sep).join("/");
}
