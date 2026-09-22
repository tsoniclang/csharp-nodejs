import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { fileURLToPath } from "node:url";
import test from "node:test";
import { createTsonicPlugin } from "../../dist/index.js";
import {
  csharpProviderPolicyContributionKind,
  csharpCoreRuntimeSource,
  csharpJsRuntimeSource,
} from "../../../tsonic-csharp/dist/public/provider.js";

test("C# Node contributes through the standard target capability hook", () => {
  const plugin = createTsonicPlugin();
  assert.equal(typeof plugin.createTargetContributions, "function");
  assert.equal(Object.hasOwn(plugin, "createOperationMappers"), false);

  const contributions = plugin.createTargetContributions({
    project: { entryPoint: "index.ts", rootDir: ".", targets: [] },
    target: { id: "csharp" },
    targetPack: { id: "csharp", displayName: "C#" },
    selectedCapabilities: [plugin],
    selectedSurfaces: [],
    selectedSurfaceIds: [],
    capability: plugin,
  });
  assert.equal(contributions.length, 1);
  assert.equal(
    contributions[0].kind,
    csharpProviderPolicyContributionKind,
  );
  assert.equal(contributions[0].relations.length, 1818);
  assert.equal(contributions[0].rejections.length, 166);
});

test("C# Node owns its runtime source closure without selecting the JS source surface", () => {
  const plugin = createTsonicPlugin();
  const references = plugin.runtimeContributions({}).references;

  assert.deepEqual(
    references.map((reference) => reference.include),
    [csharpCoreRuntimeSource.projectPath, csharpJsRuntimeSource.projectPath,
      fileURLToPath(import.meta.resolve("@tsonic/csharp-nodejs/runtime.csproj"))],
  );
  for (const reference of references) {
    assert.equal(reference.kind, "csharp-source-project");
    assert.equal(existsSync(reference.include), true);
  }
  assert.equal(references[1].attributes.TsonicCsharpRuntimeProject, references[0].include);
  assert.equal(references[2].attributes.TsonicCsharpJsProject, references[1].include);
  assert.equal(references[2].attributes.TsonicCsharpRuntimeProject, references[0].include);
});
