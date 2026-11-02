import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeRetainedErrorCases, nativeRetainedErrorSourceFor, nativeReadonlyRetainedErrorSourceFor, nativeRetainedTerminalErrorSource } from "../../../../tsonic/test/fixtures/native-retained-errors.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

for (const [parameter, storage, ordering] of nativeRetainedErrorCases) {
    test(`retaining native streams preserve original Error subtype, identity and live fields (${parameter}, ${storage}, ${ordering})`, { timeout: 300_000 }, () => {
      const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
        capabilities: [createTsonicPlugin()], sourceText: `${nativeRetainedErrorSourceFor(parameter, storage, ordering)}\nrun();`,
      });
      assert.equal(compiled.sourceDiagnosticsText, "", "the original write program remains checked");
      assert.equal(compiled.result.diagnostics.length, 3, "the three original native Error writes are local refusals");
      for (const diagnostic of compiled.result.diagnostics) {
        assert.equal(diagnostic.code, "CSHARP_ERROR_READONLY_STORAGE");
        assert.equal(diagnostic.message,
          "C# exceptions are immutable after construction; create a new Error or pass the message to super instead.");
        assert.equal(diagnostic.sourceNode !== undefined, true, "each refusal retains its actual source write");
      }
      const firstWrite = compiled.result.diagnostics[0].sourceNode;
      const access = compiled.source.ast.as.AsPropertyAccessExpression(firstWrite);
      assert.equal(access !== undefined, true, "the first refusal belongs to the first property write");
      assert.equal(compiled.source.ast.text(compiled.source.ast.name(firstWrite)), "name");
      assert.equal(compiled.source.ast.text(access.Expression), "original");
      assert.equal(compiled.result.program === undefined && compiled.result.plan === undefined && compiled.artifacts.size === 0,
        true, "the first invalid write prevents target publication");
    });
    test(`retaining native streams observe readonly Error subtype, identity, payload and live own fields (${parameter}, ${storage}, ${ordering})`, { timeout: 300_000 }, () => {
      const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
        capabilities: [createTsonicPlugin()], sourceText: `${nativeReadonlyRetainedErrorSourceFor(parameter, storage, ordering)}\nrun();`,
      });
      assertCsharpCompilationSucceeded(compiled);
      const emitted = [...compiled.artifacts.values()].join("\n");
      assert.doesNotMatch(emitted, /Tsonic\.CSharp\.Runtime\.Error\b|ErrorObject/u);
      const execution = executeCsharpConstruction(compiled, `native-retained-errors-${parameter}-${storage}-${ordering}`, false, false, [
        fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
      ]);
      assert.equal(typeof execution, "string");
    });
}

test("terminal source failures preserve identity, native cleanup and subsequent close delivery", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
    capabilities: [createTsonicPlugin()], sourceText: `${nativeRetainedTerminalErrorSource}\nif (!run()) throw new Error("terminal source failure lost");`,
  });
  assertCsharpCompilationSucceeded(compiled);
  const execution = executeCsharpConstruction(compiled, "native-retained-terminal-errors", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ]);
  assert.equal(typeof execution, "string");
});
