import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeEventFailureSource, nativeEventReentrantSource, nativePortCompletionSource, nativePortFailureSource } from "../../../../tsonic/test/fixtures/native-event-contexts.mjs";
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

for (const [name, source, expected] of [
  ["native-event-identity", nativeEventFailureSource, "native event identity"],
  ["native-event-reentrancy", nativeEventReentrantSource, "native event reentrancy"],
  ["native-port-completion", nativePortCompletionSource, "native port completion"],
]) {
  test(name, { timeout: 300_000 }, () => {
    const output = execute(source, name, `
Tsonic.Generated.Index.main();
Tsonic.CSharp.Js.JsEventLoop.Run();
`);
    assert.equal(output.includes(expected), true);
  });
}

test("original port error stops dispatch without consuming pending source messages", { timeout: 300_000 }, () => {
  const output = execute(nativePortFailureSource, "native-port-original-error", `
var original = Tsonic.Generated.Index.schedule();
try {
    Tsonic.CSharp.Js.JsEventLoop.Run();
    throw new System.Exception("source callback did not fail");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original) || error.Message != "original port failure")
        throw new System.Exception("original error identity lost", error);
}
if (Tsonic.Generated.Index.count() != 0) throw new System.Exception("later message consumed on failure");
Tsonic.CSharp.Js.JsEventLoop.Run();
if (Tsonic.Generated.Index.count() != 1) throw new System.Exception("pending message lost");
System.Console.WriteLine("native original error");
`);
  assert.match(output, /native original error/u);
});
