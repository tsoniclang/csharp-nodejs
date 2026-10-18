import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeTimerCompletionSource, nativeTimerOriginalErrorSource, nativeJsTimerCompletionSource } from "../../../../tsonic/test/fixtures/native-timer-contexts.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

function execute(sourceText, name, nativeProgram, node = true) {
  const compiled = compileCsharpSource({ surface: "js", sourceText,
    targetOptions: { outputType: "Library" }, capabilities: node ? [createTsonicPlugin()] : [],
  });
  assertCsharpCompilationSucceeded(compiled);
  return executeCsharpConstruction(compiled, name, false, false, node ? [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ] : [], nativeProgram);
}

test("generated Node driver drains independent Node and JS timer roots", { timeout: 300_000 }, () => {
  const output = execute(nativeTimerCompletionSource, "native-timer-context", `
Tsonic.CSharp.Js.JsEventLoop.Run(() => { Tsonic.Generated.Index.main(); });
`);
  assert.match(output, /native timer completion/u);
});

test("generated JS-only driver selects the same native typed timer owner", { timeout: 300_000 }, () => {
  const output = execute(nativeJsTimerCompletionSource, "native-js-timer-context", `
Tsonic.CSharp.Js.JsEventLoop.Run(() => { Tsonic.Generated.Index.main(); });
`, false);
  assert.match(output, /native timer completion/u);
});

test("original timer error stops dispatch without consuming another selected root", { timeout: 300_000 }, () => {
  const output = execute(nativeTimerOriginalErrorSource, "native-timer-original-error", `
System.Exception? original = null;
try {
    Tsonic.CSharp.Js.JsEventLoop.Run(() => { original = Tsonic.Generated.Index.schedule(); });
    throw new System.Exception("source callback did not fail");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original) || error.Message != "original timer failure")
        throw new System.Exception("original error identity lost", error);
}
if (Tsonic.Generated.Index.count() != 0) throw new System.Exception("later callback consumed on failure");
Tsonic.CSharp.Js.JsEventLoop.Run();
if (Tsonic.Generated.Index.count() != 0) throw new System.Exception("retired callback entered a new owner epoch");
System.Console.WriteLine("native original error");
`);
  assert.match(output, /native original error/u);
});
