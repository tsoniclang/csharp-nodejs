import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeStreamLifecycleSource, invalidNativeStreamLifecycleSources } from "../../../../tsonic/test/fixtures/native-stream-lifecycle.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("native duplex lifecycle shares identity, closes resources and preserves fallible completion", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({
    surface: "js",
    targetOptions: { outputType: "Exe" },
    capabilities: [createTsonicPlugin()],
    sourceText: `${nativeStreamLifecycleSource}\nrun();`,
  });
  assertCsharpCompilationSucceeded(compiled);
  executeCsharpConstruction(compiled, "native-stream-lifecycle", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ]);
});

for (const [name, source] of invalidNativeStreamLifecycleSources) {
  test(`native duplex lifecycle declarations reject ${name} before publication`, () => {
    const compiled = compileCsharpSource({
      surface: "js",
      capabilities: [createTsonicPlugin()],
      sourceText: source,
    });
    assert.notEqual(compiled.sourceDiagnosticsText, "");
    assert.equal(compiled.result.artifacts.length, 0);
  });
}
