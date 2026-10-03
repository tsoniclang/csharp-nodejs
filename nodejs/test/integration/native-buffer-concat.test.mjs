import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeBufferConcatSource } from "../../../../tsonic/test/fixtures/native-buffer-concat.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

const references = [fileURLToPath(new URL(
  "../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url,
))];

test("native Buffer concat consumes dense input and preserves bounded output", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()],
    sourceText: nativeBufferConcatSource });
  assertCsharpCompilationSucceeded(compiled);
  executeCsharpConstruction(compiled, "native-buffer-concat", false, false, references);
});

test("native Buffer concat does not allocate an input copy", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()],
    sourceText: nativeBufferConcatSource });
  assertCsharpCompilationSucceeded(compiled);
  const output = executeCsharpConstruction(compiled, "native-buffer-concat-cost", false, false,
    references, `
var buffers = new Tsonic.CSharp.Js.JSArray<Tsonic.CSharp.Node.Buffer>(new[] {
    Tsonic.CSharp.Node.Buffer.from("ab"), Tsonic.CSharp.Node.Buffer.from("cd"),
});
for (var index = 0; index < 100; index++) {
    System.GC.KeepAlive(Tsonic.Generated.Index.concatenate(buffers));
    System.GC.KeepAlive(Tsonic.CSharp.Node.Buffer.concat(buffers));
}
var generatedStart = System.GC.GetAllocatedBytesForCurrentThread();
for (var index = 0; index < 10000; index++) {
    System.GC.KeepAlive(Tsonic.Generated.Index.concatenate(buffers));
}
var generatedBytes = System.GC.GetAllocatedBytesForCurrentThread() - generatedStart;
var nativeStart = System.GC.GetAllocatedBytesForCurrentThread();
for (var index = 0; index < 10000; index++) {
    System.GC.KeepAlive(Tsonic.CSharp.Node.Buffer.concat(buffers));
}
var nativeBytes = System.GC.GetAllocatedBytesForCurrentThread() - nativeStart;
if (generatedBytes != nativeBytes) throw new System.Exception("concat copied its input");
System.Console.WriteLine("concat native allocation equality");
`);
  assert.match(output, /concat native allocation equality/u);
});

test("native Buffer concat rejects unrelated element storage", () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()],
    sourceText: `import { Buffer } from "node:buffer";
      export function invalid(values: string[]): Buffer { return Buffer.concat(values); }` });
  assert.notEqual(compiled.sourceDiagnosticsText, "");
  assert.equal(compiled.result.artifacts.length, 0);
});
