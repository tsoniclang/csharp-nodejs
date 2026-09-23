import assert from "node:assert/strict";
import test from "node:test";
import { assertCsharpCompilationSucceeded, compileCsharpSource } from "../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { nativeFilePositionSource } from "../../../tsonic/test/fixtures/native-file-positions.mjs";
import { createTsonicPlugin } from "../../dist/index.js";

test("filesystem calls preserve exact native int64 positions through provider selection", () => {
  const compiled = compileCsharpSource({
    surface: "js", capabilities: [createTsonicPlugin()], sourceText: nativeFilePositionSource,
  });
  assertCsharpCompilationSucceeded(compiled);
  const output = [...compiled.artifacts.values()].join("\n");
  assert.match(output, /long position/u);
  assert.match(output, /9007199254740993L/u);
  assert.match(output, /readSync\(/u);
  assert.match(output, /writeSync\(/u);
  assert.doesNotMatch(output, /\(double\)\s*position|\(int\)\s*position/u);
});
