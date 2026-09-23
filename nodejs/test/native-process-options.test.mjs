import assert from "node:assert/strict";
import test from "node:test";
import { assertCsharpCompilationSucceeded, compileCsharpSource } from "../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { invalidNativeProcessOptionValues, nativeProcessOptionSource, nativeOptionContracts, nativeOptionSource } from "../../../tsonic/test/fixtures/native-process-options.mjs";
import { createTsonicPlugin } from "../../dist/index.js";

test("process buffer options retain native integer storage", () => {
  const compiled = compileCsharpSource({
    surface: "js", capabilities: [createTsonicPlugin()],
    sourceText: nativeProcessOptionSource("maxBuffer", "2147483647"),
  });
  assertCsharpCompilationSucceeded(compiled);
});

for (const expression of [...invalidNativeProcessOptionValues, "2147483648"]) {
  test(`process buffer options reject unrepresentable native input ${expression}`, () => {
    const compiled = compileCsharpSource({
      surface: "js", capabilities: [createTsonicPlugin()],
      sourceText: nativeProcessOptionSource("maxBuffer", expression),
    });
    assert.equal(compiled.sourceDiagnosticsText, "");
    assert.ok(compiled.result.diagnostics.some(diagnostic => diagnostic.category === "error"));
    assert.equal(compiled.result.artifacts.length, 0);
  });
}

for (const [moduleSpecifier, typeName, field] of nativeOptionContracts) {
  test(`${typeName}.${field} retains native option storage and rejects nonintegral values`, () => {
    for (const expression of ["1", ...invalidNativeProcessOptionValues]) {
      const compiled = compileCsharpSource({
        surface: "js", capabilities: [createTsonicPlugin()],
        sourceText: nativeOptionSource(moduleSpecifier, typeName, field, expression),
      });
      if (expression === "1") assertCsharpCompilationSucceeded(compiled);
      else {
        assert.equal(compiled.sourceDiagnosticsText, "");
        assert.ok(compiled.result.diagnostics.some(diagnostic => diagnostic.category === "error"), expression);
        assert.equal(compiled.result.artifacts.length, 0);
      }
    }
  });
}

test("HTTP listen retains a native port and rejects fractional or nonfinite ports", () => {
  for (const expression of ["8080", ...invalidNativeProcessOptionValues]) {
    const compiled = compileCsharpSource({
      surface: "js", capabilities: [createTsonicPlugin()],
      sourceText: `
        import { createServer } from "node:http";
        export function run(): void {
          const port = ${expression};
          createServer((request, response) => { response.end(); }).listen(port);
        }
      `,
    });
    if (expression === "8080") assertCsharpCompilationSucceeded(compiled);
    else {
      assert.equal(compiled.sourceDiagnosticsText, "");
      assert.ok(compiled.result.diagnostics.some(diagnostic => diagnostic.category === "error"), expression);
      assert.equal(compiled.result.artifacts.length, 0);
    }
  }
});
