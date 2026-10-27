import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

for (const surface of ["native", "js"]) {
  test(`${surface} imported NodeError retains one native identity through stream and explicit compression callbacks`, { timeout: 300_000 }, () => {
    const compiled = compileCsharpSource({ surface, capabilities: [createTsonicPlugin()], sourceText: `
import type { NodeError } from "node:util";
import type { Readable } from "node:stream";
import type { Buffer } from "node:buffer";
import { gunzip } from "node:zlib";
export function end(stream: Readable, error: NodeError): void { stream.destroy(error); }
export function echo(error: NodeError): NodeError { return error; }
export function message(error: NodeError): string { return error.message; }
export function accept(error: NodeError | undefined, result: Buffer | undefined): void {
  if (error !== undefined) throw error;
}
export function forward(callback: (error: NodeError | undefined, result: Buffer | undefined) => void,
  error: NodeError | undefined, result: Buffer | undefined): void { callback(error, result); }
export function schedule(data: Buffer): void { gunzip(data, accept); }
` });
    assertCsharpCompilationSucceeded(compiled);
    const generated = [...compiled.artifacts.values()].join("\n");
    assert.equal(generated.includes(".destroyChain("), true, "stream destruction uses its original native member");
    assert.equal(generated.includes(".gunzip("), true, "the explicitly typed callback reaches the actual native compression API");
    assert.equal(/DynamicInvoke|TaskCompletionSource|TsValue\.from/u.test(generated), false,
      "native error transport introduces no dynamic box, forwarding task or invocation adapter");
    const output = executeCsharpConstruction(compiled, `${surface}-native-node-error-identities`, false, false, [
      fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
    ], `
var original = new System.InvalidOperationException("original native error");
if (!object.ReferenceEquals(original, Tsonic.Generated.Index.echo(original)) ||
    Tsonic.Generated.Index.message(original) != original.Message)
    throw new System.Exception("standalone NodeError identity or readonly Message changed");
var stream = new Tsonic.CSharp.Node.Readable();
bool destroyedFailed = false;
try { Tsonic.Generated.Index.end(stream, original); }
catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original)) throw new System.Exception("stream error copied", error);
    destroyedFailed = true;
}
if (!destroyedFailed) throw new System.Exception("stream did not expose its original unhandled native error");
System.Exception? observed = null;
Tsonic.CSharp.Node.Buffer? observedBuffer = null;
System.Action<System.Exception?, Tsonic.CSharp.Node.Buffer?> callback = (error, buffer) => {
    observed = error;
    observedBuffer = buffer;
};
var data = Tsonic.CSharp.Node.Buffer.from("original native buffer");
Tsonic.Generated.Index.forward(callback, original, data);
if (!object.ReferenceEquals(observed, original) || !object.ReferenceEquals(observedBuffer, data))
    throw new System.Exception("explicit callback changed error or buffer identity");
Tsonic.Generated.Index.forward(callback, null, null);
if (observed is not null || observedBuffer is not null) throw new System.Exception("callback absence changed");
try {
    Tsonic.Generated.Index.accept(original, null);
    throw new System.Exception("source callback did not throw the original error");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original)) throw new System.Exception("typed source callback error copied", error);
}
for (int index = 0; index < 4096; index++) {
    Tsonic.Generated.Index.end(stream, original);
    Tsonic.Generated.Index.forward(callback, original, data);
    _ = Tsonic.Generated.Index.echo(original);
}
long nativeBefore = System.GC.GetAllocatedBytesForCurrentThread();
for (int index = 0; index < 8192; index++) { stream.destroyChain(original); callback(original, data); _ = original; }
long nativeBytes = System.GC.GetAllocatedBytesForCurrentThread() - nativeBefore;
long allBefore = System.GC.GetTotalAllocatedBytes(true);
long before = System.GC.GetAllocatedBytesForCurrentThread();
for (int index = 0; index < 8192; index++) {
    Tsonic.Generated.Index.end(stream, original);
    Tsonic.Generated.Index.forward(callback, original, data);
    _ = Tsonic.Generated.Index.echo(original);
}
long sourceBytes = System.GC.GetAllocatedBytesForCurrentThread() - before;
long allBytes = System.GC.GetTotalAllocatedBytes(true) - allBefore;
if (nativeBytes != 0 || sourceBytes != nativeBytes || allBytes != 0)
    throw new System.Exception($"error transport allocation: native={nativeBytes}, source={sourceBytes}, all={allBytes}");
bool failed = false;
try { Tsonic.CSharp.Js.JsEventLoop.Run(() => Tsonic.Generated.Index.schedule(data)); }
catch (System.IO.InvalidDataException) { failed = true; }
if (!failed) throw new System.Exception("actual compression failure did not reach the typed source callback");
System.Console.WriteLine("native error identity");
`);
    assert.equal(output.trim(), "native error identity", "both real native interfaces retain their original error and callback ABI");
  });
}
