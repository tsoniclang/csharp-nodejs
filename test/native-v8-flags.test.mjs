import assert from "node:assert/strict";
import test from "node:test";
import { checkCsharpSource } from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { createTsonicPlugin } from "../dist/index.js";

for (const [name, source] of [
  ["non-string flags", 'import { setFlagsFromString } from "node:v8"; export function main(): void { setFlagsFromString(1); }'],
  ["missing flags", 'import { setFlagsFromString } from "node:v8"; export function main(): void { setFlagsFromString(); }'],
  ["unimplemented heap measurements", 'import { getHeapStatistics } from "node:v8"; export function main(): void { getHeapStatistics(); }'],
]) {
  test(`native V8 contract rejects ${name}`, () => {
    const checked = checkCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()],
      sourceText: source });
    assert.notEqual(checked.sourceDiagnosticsText, "");
  });
}
