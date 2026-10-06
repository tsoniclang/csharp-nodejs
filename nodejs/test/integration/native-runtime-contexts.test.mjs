import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeRuntimeCompletionSource, nativeRuntimeOriginalErrorSource } from "../../../../tsonic/test/fixtures/native-runtime-contexts.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

function execute(sourceText, name, nativeProgram) {
  const compiled = compileCsharpSource({ surface: "js", sourceText,
    targetOptions: { outputType: "Library" }, capabilities: [createTsonicPlugin()],
  });
  assertCsharpCompilationSucceeded(compiled);
  return executeCsharpConstruction(compiled, name, false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ], nativeProgram);
}

test("generated Node driver selects source-thread readline tasks without worker scheduling", { timeout: 300_000 }, () => {
  const output = execute(nativeRuntimeCompletionSource, "native-runtime-context", `
Tsonic.Generated.Index.main();
Tsonic.CSharp.Js.JsEventLoop.Run();
`);
  assert.match(output, /native runtime completion/u);
});

test("generated readline callback retains its original Error identity through native context grouping", { timeout: 300_000 }, () => {
  const output = execute(nativeRuntimeOriginalErrorSource, "native-runtime-original-error", `
var original = Tsonic.Generated.Index.schedule();
try {
    Tsonic.CSharp.Js.JsEventLoop.Run();
    throw new System.Exception("source callback did not fail");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original) || error.Message != "original readline failure")
        throw new System.Exception("original error identity lost", error);
}
System.Console.WriteLine("native original error");
`);
  assert.match(output, /native original error/u);
});
