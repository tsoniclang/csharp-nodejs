import assert from "node:assert/strict";
import { createTsonicPlugin } from "../../dist/index.js";

export function nodejsSourceProvider(selectedSurfaceIds, plugin = createTsonicPlugin()) {
  const contribution = plugin.sourceCompilerContributions({
    project: { entryPoint: "index.ts", rootDir: ".", targets: [] },
    projectDirectory: process.cwd(),
    target: { id: "csharp" },
    selectedCapabilityIds: [plugin.id],
    selectedSurfaceIds,
    capability: plugin,
  });
  assert.equal(contribution.extensions.length, 1);
  const providers = [];
  contribution.extensions[0].initialize({
    registerSourceDeclarationProvider(provider) {
      providers.push(provider);
    },
  });
  assert.equal(providers.length, 1);
  return providers[0];
}

export function nodejsDeclarationModel(provider, moduleSpecifier) {
  const resolution = provider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = provider.getDeclarationModel(resolution, {
    context: {},
    materialization: { kind: "complete" },
  });
  assert.equal(model.moduleSpecifier, moduleSpecifier);
  return model;
}
