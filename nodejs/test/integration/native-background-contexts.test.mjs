import assert from "node:assert/strict";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource, assertCsharpCompilationSucceeded } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { nativeBackgroundCompletionSource, nativeBackgroundAsyncSource,
  nativeBackgroundOriginalErrorSource, nativeBackgroundNoDemandSource,
  nativeBackgroundPackageFiles, nativeBackgroundPackageGraph } from "../../../../tsonic/test/fixtures/native-background-contexts.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

function compile(sourceText, outputType = "Library") {
  const compiled = compileCsharpSource({ surface: "js", sourceText,
    targetOptions: { outputType }, capabilities: [createTsonicPlugin()],
  });
  assertCsharpCompilationSucceeded(compiled);
  return compiled;
}

function execute(compiled, name, nativeProgram = `
Tsonic.Generated.Index.main();
Tsonic.CSharp.Js.JsEventLoop.Run();
`) {
  return executeCsharpConstruction(compiled, name, false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ], nativeProgram);
}

test("generated native Node driver consumes the demanded compression context", { timeout: 300_000 }, () => {
  const output = execute(compile(nativeBackgroundCompletionSource), "native-background-context");
  assert.match(output, /native completion/u);
});

test("generated asynchronous Node driver consumes the same component context", { timeout: 300_000 }, () => {
  const output = execute(compile(nativeBackgroundAsyncSource), "native-background-async-context", `
Tsonic.CSharp.Js.JsEventLoop.Run(Tsonic.Generated.Index.main());
`);
  assert.match(output, /native async completion/u);
});

test("generated source compression callback retains its original Error identity", { timeout: 300_000 }, () => {
  const output = execute(compile(nativeBackgroundOriginalErrorSource, "Library"), "native-background-original-error", `
var original = Tsonic.Generated.Index.schedule();
try {
    Tsonic.CSharp.Js.JsEventLoop.Run();
    throw new System.Exception("source callback did not fail");
} catch (System.Exception error) {
    if (!object.ReferenceEquals(error, original) || error.Message != "original compression failure")
        throw new System.Exception("original error identity lost", error);
}
System.Console.WriteLine("native original error");
`);
  assert.match(output, /native original error/u);
});

test("unused background APIs emit no native context storage", { timeout: 300_000 }, () => {
  execute(compile(nativeBackgroundNoDemandSource), "native-background-no-demand");
});

test("component background roots dispatch through exact two-hop package error domains", { timeout: 300_000 }, () => {
  assert.deepEqual(nativeBackgroundPackageGraph.components.find(row => row.id === "source-package-component:root").dependencies,
    ["source-package-component:@acme/middle"]);
  const compiled = compileCsharpSource({ surface: "js", projectRoot: "/src", targetOptions: { outputType: "Library" },
    sourceText: nativeBackgroundPackageFiles["index.ts"], sourcePackages: nativeBackgroundPackageGraph,
    files: Object.fromEntries(Object.entries(nativeBackgroundPackageFiles).filter(([path]) => path !== "index.ts")),
    capabilities: [createTsonicPlugin()],
  });
  const output = execute(compiled, "native-background-package-domains");
  for (const name of ["Root", "Middle", "Leaf"]) assert.match(output, new RegExp(`${name} completion`, "u"));
});
