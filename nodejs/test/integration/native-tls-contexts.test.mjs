import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeTlsCompletionSource, nativeTlsFailureSource } from "../../../../tsonic/test/fixtures/native-tls-contexts.mjs";
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

test("generated native Node driver consumes exact TLS and background roots", { timeout: 300_000 }, () => {
  const output = execute(nativeTlsCompletionSource, "native-tls-context", `
Tsonic.CSharp.Js.JsEventLoop.Run(Tsonic.Generated.Index.main);
`);
  assert.match(output, /native TLS completion/u);
});

test("original source TLS failure preserves later native listening callbacks", { timeout: 300_000 }, () => {
  const output = execute(nativeTlsFailureSource, "native-tls-original-error", `
System.Exception? original = null;
try {
    Tsonic.CSharp.Js.JsEventLoop.Run(() => original = Tsonic.Generated.Index.schedule());
    throw new System.Exception("source callback did not fail");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original) || error.Message != "original TLS failure")
        throw new System.Exception("original error identity lost", error);
}
if (Tsonic.Generated.Index.count() != 0) throw new System.Exception("later listener consumed on failure");
var freshCalls = 0;
Tsonic.CSharp.Js.JsEventLoop.Run(() => Tsonic.CSharp.Node.process.nextTick(() => freshCalls++));
if (freshCalls != 1) throw new System.Exception("fresh explicit owner did not dispatch");
if (Tsonic.Generated.Index.count() != 0) throw new System.Exception("retired source callback entered a fresh generation");
if (Tsonic.CSharp.Js.ProcessKeepAlive.HasReferences) throw new System.Exception("retired owner retained work");
System.Console.WriteLine("native original TLS error");
`);
  assert.match(output, /native original TLS error/u);
});
