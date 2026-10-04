import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeRetainedErrorCases, nativeRetainedErrorSourceFor } from "../../../../tsonic/test/fixtures/native-retained-errors.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

for (const [parameter, storage, ordering] of nativeRetainedErrorCases) {
    test(`retaining native streams preserve original Error subtype, identity and live fields (${parameter}, ${storage}, ${ordering})`, { timeout: 300_000 }, () => {
      const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
        capabilities: [createTsonicPlugin()], sourceText: `${nativeRetainedErrorSourceFor(parameter, storage, ordering)}\nrun();`,
      });
      assertCsharpCompilationSucceeded(compiled);
      const execution = executeCsharpConstruction(compiled, `native-retained-errors-${parameter}-${storage}-${ordering}`, false, false, [
        fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
      ]);
      assert.equal(typeof execution, "string");
    });
}
