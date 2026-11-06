import assert from "node:assert/strict";
import test from "node:test";
import { assertCsharpCompilationSucceeded, compileCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { invalidNativeProcessOptionValues, nativeProcessOptionSource, nativeOptionContracts, nativeOptionSource } from "../../../../tsonic/test/fixtures/native-process-options.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("process buffer options retain native integer storage", () => {
  const compiled = compileCsharpSource({
    surface: "js", capabilities: [createTsonicPlugin()],
    sourceText: nativeProcessOptionSource("maxBuffer", "2147483647"),
  });
  assertCsharpCompilationSucceeded(compiled);
});

for (const expression of [...invalidNativeProcessOptionValues, "2147483648"]) {
  test(`process buffer options check non-native input ${expression}`, () => {
    const compiled = compileCsharpSource({
      surface: "js", capabilities: [createTsonicPlugin()],
      sourceText: nativeProcessOptionSource("maxBuffer", expression),
    });
    assertCsharpCompilationSucceeded(compiled);
    assert.match(compiled.artifacts.get("src/Index.cs"), /IntegerConversions\.Checked/u);
  });
}

for (const [moduleSpecifier, typeName, field] of nativeOptionContracts) {
  test(`${typeName}.${field} retains native option storage and checks nonintegral values`, () => {
    for (const expression of ["1", ...invalidNativeProcessOptionValues]) {
      const compiled = compileCsharpSource({
        surface: "js", capabilities: [createTsonicPlugin()],
        sourceText: nativeOptionSource(moduleSpecifier, typeName, field, expression),
      });
      assertCsharpCompilationSucceeded(compiled);
      if (expression !== "1") assert.match(compiled.artifacts.get("src/Index.cs"), /IntegerConversions\.Checked/u);
    }
  });
}

test("HTTP listen exposes native int32 ports on both surfaces without a numeric shim", () => {
  for (const surface of ["csharp", "js"]) {
    const compiled = compileCsharpSource({
      surface, capabilities: [createTsonicPlugin()],
      sourceText: `
        import { createServer } from "node:http";
        import type { int32 } from "@tsonic/core/types.js";
        export function run(port: int32): void {
          createServer((request, response) => { response.end(); }).listen(port);
        }
      `,
    });
    assertCsharpCompilationSucceeded(compiled);
    const source = compiled.artifacts.get("src/Index.cs");
    assert.match(source, /\.listen\(port\)/u);
    assert.doesNotMatch(source, /RequireInteger|Convert\.ToInt32/u);
  }
});
