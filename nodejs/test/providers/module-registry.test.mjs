import assert from "node:assert/strict";
import { isDeepStrictEqual } from "node:util";
import test from "node:test";
import { createTsonicPlugin } from "../../../dist/index.js";
import { nodejsProviderModules } from "../../../dist/provider/assembly/modules.js";
import { nodejsProviderModuleSpecifiers } from "../../../dist/provider/declarations/specifiers.js";
import { nodejsDeclarationModel, nodejsSourceProvider } from "../helpers/provider-package.mjs";

const expectedCanonicalModules = [
  "node:assert", "node:buffer", "node:child_process", "node:crypto", "node:dns",
  "node:dns/promises", "node:events", "node:fs", "node:fs/promises", "node:http",
  "node:https", "node:net", "node:os", "node:path", "node:process", "node:readline",
  "node:stream", "node:timers", "node:tls", "node:url", "node:util", "node:v8",
  "node:worker_threads", "node:zlib",
];

test("Node module assembly owns the exact canonical registry on both source profiles", () => {
  assert.deepEqual(nodejsProviderModules.map((module) => module.moduleSpecifier).sort(), expectedCanonicalModules);
  assert.equal(new Set(nodejsProviderModules.map((module) => module.providerModuleId)).size, nodejsProviderModules.length);
  for (const selectedSurfaceIds of [[], ["js"]]) {
    const provider = nodejsSourceProvider(selectedSurfaceIds);
    for (const module of nodejsProviderModules) {
      const model = nodejsDeclarationModel(provider, module.moduleSpecifier);
      assert.equal(model.providerModuleId, module.providerModuleId, module.moduleSpecifier);
      const exports = module.getExports(selectedSurfaceIds);
      assert.equal(isDeepStrictEqual(model.exports, exports), true, module.moduleSpecifier);
      assert.equal(new Set(exports.map((entry) => entry.id)).size, exports.length, module.moduleSpecifier);
    }
  }
});

test("Node module aliases resolve to one registered canonical declaration owner", () => {
  const plugin = createTsonicPlugin();
  const aliases = nodejsProviderModuleSpecifiers();
  const canonicalModules = new Map(nodejsProviderModules.map((module) => [module.moduleSpecifier, module]));
  assert.equal(new Set(aliases.map((alias) => alias.moduleSpecifier)).size, aliases.length);
  assert.deepEqual(plugin.moduleOwnership.map((ownership) => ownership.specifierPrefix), aliases.map((alias) => alias.moduleSpecifier));
  for (const selectedSurfaceIds of [[], ["js"]]) {
    const provider = nodejsSourceProvider(selectedSurfaceIds, plugin);
    for (const alias of aliases) {
      const module = canonicalModules.get(alias.canonicalModuleSpecifier);
      assert.equal(module !== undefined, true, alias.moduleSpecifier);
      const resolution = provider.resolveModule(alias.moduleSpecifier, {});
      assert.equal(resolution.kind, "virtual", alias.moduleSpecifier);
      assert.equal(resolution.providerModuleId, module.providerModuleId, alias.moduleSpecifier);
    }
    assert.deepEqual(provider.ownsModule("node:unregistered", {}), { kind: "unowned" });
  }
});
