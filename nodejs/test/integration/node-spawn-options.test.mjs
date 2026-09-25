import assert from "node:assert/strict";
import test from "node:test";
import { nodeChildProcessExports } from "../../../dist/provider/modules/child-process.js";
import { assertCsharpCompilationSucceeded, compileCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";
import { nativeNodeSpawnSource } from "../../../../tsonic/test/fixtures/native-node-spawn.mjs";

test("spawnSync options and result absence are exact public declarations", () => {
  for (const js of [false, true]) {
    const exports = nodeChildProcessExports(js);
    const result = exports.find(entry => entry.name === "SpawnSyncReturns");
    for (const name of ["stdout", "stderr", "status", "signal"]) {
      assert.ok(result.members.find(member => member.name === name).type.types.some(type => type.kind === "literal" && type.value === null));
    }
    for (const name of ["pid", "error"]) assert.equal(result.members.find(member => member.name === name).optional, true);
    const options = exports.find(entry => entry.name === "SpawnSyncOptionsWithBufferEncoding");
    assert.deepEqual(options.members.map(member => member.name), ["encoding", "cwd", "env", "maxBuffer", "uid", "gid", "timeout", "killSignal", "input", "stdio"]);
    assert.ok(options.members.every(member => member.optional === true));
    const input = options.members.find(member => member.name === "input").type;
    assert.equal(input.kind, js ? "union" : "provider-ref");
    assert.equal(exports.find(entry => entry.name === "spawnSync").signatures.length, 2);
  }
});

test("spawnSync compiles standard mutable Node options without source rewriting", () => {
  const result = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()], sourceText: nativeNodeSpawnSource(process.execPath) });
  assertCsharpCompilationSucceeded(result);
  const text = [...result.artifacts.values()].join("\n");
  assert.match(text, /SpawnSyncOptions/);
  assert.match(text, /spawnSyncResult/);
  assert.doesNotMatch(text, /System\.Reflection|dynamic\b/);
});

test("native-profile spawn input uses Buffer without adding JavaScript globals", () => {
  const result = compileCsharpSource({ capabilities: [createTsonicPlugin()], sourceText: `
    import { spawnSync } from "node:child_process";
    import { Buffer } from "node:buffer";
    export function run(): boolean {
      const result = spawnSync("program", [], { input: Buffer.from("input"), maxBuffer: 4096 });
      return result.status === 0;
    }
  ` });
  assertCsharpCompilationSucceeded(result);
});

test("spawnSync cannot silently accept wrong option kinds", () => {
  const result = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()], sourceText: `
    import { spawnSync } from "node:child_process";
    export const result = spawnSync("program", [], { uid: "0", stdio: [true], input: "bytes" });
  ` });
  assert.notEqual(result.sourceDiagnosticsText, "");
  assert.equal(result.artifacts.size, 0);
});
