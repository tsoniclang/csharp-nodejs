import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeWatcherCompletionSource, nativeWatcherFailureSource, nativeWatcherNoDemandSource } from "../../../../tsonic/test/fixtures/native-watcher-contexts.mjs";
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

test("generated native Node driver consumes the exact native watcher root", { timeout: 300_000 }, () => {
  const output = execute(nativeWatcherCompletionSource, "native-watcher-context", `
Tsonic.Generated.Index.main();
Tsonic.CSharp.Js.JsEventLoop.Run();
`);
  assert.match(output, /native watcher completion/u);
});

test("native filesystem queries require no watcher roots", { timeout: 300_000 }, () => {
  execute(nativeWatcherNoDemandSource, "native-watcher-no-demand", `
Tsonic.Generated.Index.main();
if (Tsonic.CSharp.Js.ProcessKeepAlive.HasReferences)
    throw new System.Exception("filesystem queries manufactured retained work");
`);
});

test("original source watcher failure preserves reentrant native stat work", { timeout: 300_000 }, () => {
  const output = execute(nativeWatcherFailureSource, "native-watcher-original-error", `
var original = Tsonic.Generated.Index.schedule();
try {
    Tsonic.CSharp.Js.JsEventLoop.Run();
    throw new System.Exception("source callback did not fail");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original) || error.Message != "original watcher failure")
        throw new System.Exception("original error identity lost", error);
}
if (Tsonic.Generated.Index.count() != 0) throw new System.Exception("reentrant watcher already invoked");
Tsonic.CSharp.Js.JsEventLoop.Run();
if (Tsonic.Generated.Index.count() != 1) throw new System.Exception("reentrant watcher lost");
System.Console.WriteLine("native original watcher error");
`);
  assert.match(output, /native original watcher error/u);
});
