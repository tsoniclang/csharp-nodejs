import assert from "node:assert/strict";
import test from "node:test";
import { checkCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";
import { nativeV8HeapFields, nativeV8HeapSource } from "../../../../tsonic/test/fixtures/native-v8-heap.mjs";
import { nodeV8CallTargetMembers, nodeV8Exports, nodeV8PropertyTargetMembers } from "../../../dist/provider/modules/v8.js";

test("native V8 heap imports and uncalled field reads retain exact source declarations", () => {
  const checked = checkCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()],
    sourceText: nativeV8HeapSource });
  assert.equal(checked.sourceDiagnosticsText, "");
  const exported = nodeV8Exports().find(entry => entry.name === "HeapInfo");
  assert.equal(exported.kind, "interface");
  assert.deepEqual(exported.members.map(member => member.name).sort(), [...nativeV8HeapFields].sort());
  assert.equal(exported.members.every(member => member.readonly === undefined), true);
  const properties = nodeV8PropertyTargetMembers();
  assert.equal(properties.length, nativeV8HeapFields.length);
  const call = nodeV8CallTargetMembers().find(entry => entry.exportName === "getHeapStatistics");
  assert.deepEqual(call.providerParameters, []);
  assert.deepEqual(call.providerReturnType, { kind: "provider-ref", moduleSpecifier: "node:v8", exportName: "HeapInfo" });
});

for (const [name, source] of [
  ["non-string flags", 'import { setFlagsFromString } from "node:v8"; export function main(): void { setFlagsFromString(1); }'],
  ["missing flags", 'import { setFlagsFromString } from "node:v8"; export function main(): void { setFlagsFromString(); }'],
  ["heap observation arguments", 'import { getHeapStatistics } from "node:v8"; export function main(): void { getHeapStatistics(1); }'],
  ["unimplemented code measurements", 'import { getHeapCodeStatistics } from "node:v8"; export function main(): void { getHeapCodeStatistics(); }'],
  ["invalid heap flag", 'import type { HeapInfo } from "node:v8"; export function change(info: HeapInfo): void { info.does_zap_garbage = 2; }'],
]) {
  test(`native V8 contract rejects ${name}`, () => {
    const checked = checkCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()],
      sourceText: source });
    assert.notEqual(checked.sourceDiagnosticsText, "");
  });
}
