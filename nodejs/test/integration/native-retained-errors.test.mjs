import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeRetainedErrorSource } from "../../../../tsonic/test/fixtures/native-retained-errors.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("retaining native streams preserve the original Error subtype, identity and live fields", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", targetOptions: { outputType: "Exe" },
    capabilities: [createTsonicPlugin()], sourceText: `${nativeRetainedErrorSource}\nrun();`,
  });
  assertCsharpCompilationSucceeded(compiled);
  const execution = executeCsharpConstruction(compiled, "native-retained-errors", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ]);
  assert.equal(typeof execution, "string");
});
