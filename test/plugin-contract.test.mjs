import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import test from "node:test";
import { createTsonicPlugin } from "../dist/index.js";
import {
  csharpProviderPolicyContributionKind,
} from "../../tsonic-csharp/dist/public/provider.js";

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
  assert.equal(contributions.length, 1);
  assert.equal(
    contributions[0].kind,
    csharpProviderPolicyContributionKind,
  );
  assert.equal(contributions[0].relations.length, 902);
  assert.equal(contributions[0].rejections.length, 182);
});

test("C# Node owns its JS runtime assembly dependency without selecting the JS source surface", () => {
  const plugin = createTsonicPlugin();
  const references = plugin.runtimeContributions({}).references;

  assert.deepEqual(
    references
      .filter((reference) => reference.kind === "assembly")
      .map((reference) => reference.include),
    ["Tsonic.CSharp.Node", "Tsonic.CSharp.Js"],
  );
  assert.match(
    references[1].attributes.HintPath,
    /\/csharp-js\/runtimes\/net10\.0\/Tsonic\.CSharp\.Js\.dll$/u,
  );
  assert.equal(existsSync(references[1].attributes.HintPath), true);
  assert.equal(
    references.some((reference) => reference.kind === "assembly" && reference.include === "Tsonic.CSharp.Runtime"),
    false,
  );
});
