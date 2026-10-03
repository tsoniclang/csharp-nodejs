import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeNodeErrorSource, nativeNodeErrorUnionSource, nativeNodeErrorUnionProofSource } from "../../../../tsonic/test/fixtures/native-node-errors.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("Node callbacks retain native errors and exact optional error storage", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
    capabilities: [createTsonicPlugin()], sourceText: `${nativeNodeErrorSource}\nrun();`,
  });
  assertCsharpCompilationSucceeded(compiled);
  const execution = executeCsharpConstruction(compiled, "native-node-errors", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ]);
  assert.match(execution, /native error/u);
  assert.match(execution, /native success/u);
  assert.match(execution, /native throw/u);
});

test("native callback errors do not promise mutable source Error storage", () => {
  for (const sourceText of [
    `import type { NodeError } from "node:util";
     export function change(error: NodeError): void { error.message = "changed"; }`,
    `import type { Readable } from "node:stream";
     export function observe(source: Readable): void {
       source.on("error", (error: Error) => { console.log(error.stack); });
     }`,
  ]) {
    const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()], sourceText });
    assert.notEqual(compiled.sourceDiagnosticsText, "");
    assert.equal(compiled.result.artifacts.length, 0);
  }
});

test("last-use native errors preserve exception identity and later control-flow uses", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Library" },
    capabilities: [createTsonicPlugin()], sourceText: nativeNodeErrorSource,
  });
  assertCsharpCompilationSucceeded(compiled);
  const execution = executeCsharpConstruction(compiled, "native-node-error-cost", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ], `
var original = new System.Exception("retained message");
Tsonic.Generated.Index.consume(null);
for (var index = 0; index < 10000; index++) {
    try { Tsonic.Generated.Index.consume(original); }
    catch (System.Exception caught) {
        if (!System.Object.ReferenceEquals(original, caught)) throw new System.Exception("native identity lost");
        continue;
    }
    throw new System.Exception("native error not thrown");
}
if (Tsonic.Generated.Index.retained(original) != "retained message" ||
    Tsonic.Generated.Index.finalized(original) != "retained message" ||
    Tsonic.Generated.Index.repeated(original) != "retained message" ||
    !Tsonic.Generated.Index.captured(original)) {
    throw new System.Exception("retained control-flow storage lost");
}
System.Console.WriteLine("native identity retained");
`);
  assert.match(execution, /native identity retained/u);
});

test("closed native error alternatives preserve every original error route", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
    capabilities: [createTsonicPlugin()], sourceText: `${nativeNodeErrorUnionProofSource}\nrunUnions();`,
  });
  assertCsharpCompilationSucceeded(compiled);
  const output = executeCsharpConstruction(compiled, "native-node-error-unions", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ]);
  assert.equal(output.match(/native union routes/gu)?.length, 1);
});

test("native error union dispatch preserves original exception references", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Library" },
    capabilities: [createTsonicPlugin()], sourceText: nativeNodeErrorUnionSource,
  });
  assertCsharpCompilationSucceeded(compiled);
  const output = executeCsharpConstruction(compiled, "native-node-error-union-identity", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ], `
var native = new System.Exception("native union message");
var source = new Tsonic.CSharp.Runtime.Error("source union message");
var caughtCount = 0;
for (var index = 0; index < 10000; index++) {
    try { Tsonic.Generated.Index.throwNative(native); }
    catch (System.Exception caught) {
        if (!System.Object.ReferenceEquals(native, caught)) throw new System.Exception("native union identity lost");
        caughtCount++;
    }
    try { Tsonic.Generated.Index.throwSource(source); }
    catch (System.Exception caught) {
        if (!System.Object.ReferenceEquals(source, caught)) throw new System.Exception("source union identity lost");
        caughtCount++;
    }
}
if (caughtCount != 20000) throw new System.Exception("native union throw omitted");
System.Console.WriteLine("original union identities retained");
`);
  assert.match(output, /original union identities retained/u);
});
