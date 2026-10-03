import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeNodeErrorSource } from "../../../../tsonic/test/fixtures/native-node-errors.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("Node callbacks retain native errors and exact optional error storage", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
    capabilities: [createTsonicPlugin()], sourceText: `${nativeNodeErrorSource}\nrun();`,
  });
  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.result.diagnostics, []);
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
  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.result.diagnostics, []);
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
