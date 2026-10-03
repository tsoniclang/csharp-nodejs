import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeFileOffsetDeclarations, nativeFileOffsetsSource } from "../../../../tsonic/test/fixtures/native-file-offsets.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("selected native file offsets retain width through nullish utilities and aliases", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()], sourceText: nativeFileOffsetsSource });
  assertCsharpCompilationSucceeded(compiled);
  const output = [...compiled.artifacts.values()].join("\n");
  assert.match(output, /long exact\(long value\)/u);
  assert.match(output, /long alias\(long value\)/u);
  assert.match(output, /long present\(long\? value\)/u);
  assert.doesNotMatch(output, /double|Convert\.ToDouble/u);
  const references = [fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url))];
  executeCsharpConstruction(compiled, "native-file-offsets", false, false, references, `
foreach (var value in new long[] { 0, 9007199254740993L, long.MaxValue }) {
    if (Tsonic.Generated.Index.exact(value) != value || Tsonic.Generated.Index.alias(value) != value ||
        Tsonic.Generated.Index.present(value) != value || Tsonic.Generated.Index.decimalQuotient(value) != value / 10)
        throw new System.Exception("native file offset precision");
}
if (Tsonic.Generated.Index.present(null) != 0) throw new System.Exception("native offset absence");
for (var index = 0; index < 100; index++) Tsonic.Generated.Index.alias(long.MaxValue);
var before = System.GC.GetAllocatedBytesForCurrentThread();
for (var index = 0; index < 10000; index++) {
    if (Tsonic.Generated.Index.exact(long.MaxValue) != long.MaxValue ||
        Tsonic.Generated.Index.alias(long.MaxValue) != long.MaxValue ||
        Tsonic.Generated.Index.decimalQuotient(long.MaxValue) != long.MaxValue / 10)
        throw new System.Exception("native offset cost control");
}
if (System.GC.GetAllocatedBytesForCurrentThread() != before) throw new System.Exception("offset utility allocation");
`);
});

test("non-nullish native offsets reject an absent source return", () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()],
    sourceText: `${nativeFileOffsetDeclarations} export function invalid(value: Offset | null): Offset { return value; }` });
  assert.notEqual(compiled.sourceDiagnosticsText, "");
  assert.equal(compiled.result.artifacts.length, 0);
});
