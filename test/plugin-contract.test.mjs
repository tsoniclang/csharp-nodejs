import assert from "node:assert/strict";
import test from "node:test";
import { createTsonicPlugin } from "../dist/index.js";
import { csharpProviderOperationsContributionKind } from "../../tsonic-csharp/dist/index.js";

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
    capability: plugin,
  });
  assert.deepEqual(contributions.map((contribution) => contribution.kind), [
    csharpProviderOperationsContributionKind,
  ]);
});
