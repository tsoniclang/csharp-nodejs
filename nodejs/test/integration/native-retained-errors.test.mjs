import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { assertNativeErrorWriteRefusals, nativeErrorDiagnosticText } from "../../../../tsonic-csharp/test/helpers/native-error-refusals.mjs";
import { nativeRetainedErrorCases, nativeRetainedErrorSourceFor, nativeReadonlyRetainedErrorSourceFor, nativeRetainedTerminalErrorSource } from "../../../../tsonic/test/fixtures/native-retained-errors.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

for (const [parameter, storage, ordering] of nativeRetainedErrorCases) {
    test(`retaining native streams preserve original Error subtype, identity and live fields (${parameter}, ${storage}, ${ordering})`, { timeout: 300_000 }, () => {
      const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
        capabilities: [createTsonicPlugin()], sourceText: `${nativeRetainedErrorSourceFor(parameter, storage, ordering)}\nrun();`,
      });
      assertNativeErrorWriteRefusals(compiled, ["original.name", "original.stack", "original.message"]);
      assert.equal(nativeErrorDiagnosticText(compiled, compiled.result.diagnostics[0]), "original.name",
        "the first refusal belongs to the first original write");
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
