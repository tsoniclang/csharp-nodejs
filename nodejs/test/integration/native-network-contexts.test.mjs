import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeNetworkCompletionSource, nativeNetworkFailureSource, nativeNetworkNoDemandSource } from "../../../../tsonic/test/fixtures/native-network-contexts.mjs";
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

test("generated native Node driver consumes the exact native network root", { timeout: 300_000 }, () => {
  const output = execute(nativeNetworkCompletionSource, "native-network-context", `
Tsonic.Generated.Index.main();
Tsonic.CSharp.Js.JsEventLoop.Run();
`);
  assert.match(output, /native network completion/u);
});

test("native network queries require no callback roots", { timeout: 300_000 }, () => {
  execute(nativeNetworkNoDemandSource, "native-network-no-demand", `
Tsonic.Generated.Index.main();
if (Tsonic.CSharp.Js.ProcessKeepAlive.HasReferences)
    throw new System.Exception("native queries manufactured retained work");
`);
});

test("original source network failure preserves later native listening callbacks", { timeout: 300_000 }, () => {
  const output = execute(nativeNetworkFailureSource, "native-network-original-error", `
var original = Tsonic.Generated.Index.schedule();
try {
    Tsonic.CSharp.Js.JsEventLoop.Run();
    throw new System.Exception("source callback did not fail");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original) || error.Message != "original network failure")
        throw new System.Exception("original error identity lost", error);
}
if (Tsonic.Generated.Index.count() != 0) throw new System.Exception("later listener consumed on failure");
Tsonic.CSharp.Js.JsEventLoop.Run();
if (Tsonic.Generated.Index.count() != 1) throw new System.Exception("pending listener lost");
System.Console.WriteLine("native original network error");
`);
  assert.match(output, /native original network error/u);
});
