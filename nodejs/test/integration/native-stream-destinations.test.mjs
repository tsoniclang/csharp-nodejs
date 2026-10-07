import { assertNoTargetDiagnostics } from "../../../../tsonic/test/scripts/diagnostic-assertions.mjs";
import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("native pipe preserves writable, transform and HTTP destination types and identity", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" }, capabilities: [createTsonicPlugin()], sourceText: `
    import { Readable, Writable, Transform } from "node:stream";
    import { Buffer } from "node:buffer";
    import type { ServerResponse } from "node:http";
    import { createGzip } from "node:zlib";
    export function pipeWritable(source: Readable, destination: Writable): Writable {
      return source.pipe(destination);
    }
    export function pipeTransform(source: Readable, destination: Transform): Transform {
      return source.pipe(destination);
    }
    export function pipeResponse(source: Readable, destination: ServerResponse): ServerResponse {
      return source.pipe(destination);
    }
    export function run(): boolean {
      const source = Readable.from([Buffer.from("stream")]);
      const destination = createGzip();
      return source.pipe(destination) === destination;
    }
    if (!run()) throw new Error("native destination identity");
  ` });
  assertNoTargetDiagnostics(compiled.result.diagnostics);
  executeCsharpConstruction(compiled, "native-stream-destinations", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ]);
});

test("native pipe rejects a non-writable destination before publication", () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()], sourceText: `
    import { Readable } from "node:stream";
    export function pipe(source: Readable, destination: Readable): Readable {
      return source.pipe(destination);
    }
  ` });
  assert.match(compiled.sourceDiagnosticsText, /Writable/);
  assert.equal(compiled.result.artifacts.length, 0);
});
