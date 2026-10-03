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
