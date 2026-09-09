import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { fileURLToPath } from "node:url";
import test from "node:test";
import * as packageExports from "../dist/index.js";
import {
  csharpNodejsProviderPackageProviderIdentity,
  csharpNodejsVirtualDeclarationFileName,
} from "../dist/provider/identity.js";
import { nodejsCanonicalProviderExports } from "../dist/provider/modules/catalog.js";
import { nodejsProviderModuleSpecifiers } from "../dist/provider/modules/specifiers.js";
import { createCsharpNodejsProviderPolicyContribution } from "../dist/provider/target-relations.js";
import { nodejsDeclarationModel, nodejsSourceProvider } from "./helpers/provider-package.mjs";
import {
  assertCsharpCompilationSucceeded,
  compileCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";

test("Node has one public plugin construction path and retains exact module ownership messages", () => {
  assert.deepEqual(Object.keys(packageExports), ["createTsonicPlugin"]);
  const plugin = packageExports.createTsonicPlugin();
  assert.equal(plugin.id, "@tsonic/csharp-nodejs");
  assert.equal(plugin.targetId, "csharp");
  assert.equal(plugin.displayName, "C# NodeJS capability package");
  assert.equal(plugin.requiredSurfaces, undefined);
  assert.deepEqual(plugin.moduleOwnership, nodejsProviderModuleSpecifiers().map(({ moduleSpecifier }) => ({
    specifierPrefix: moduleSpecifier,
    message: `target 'csharp' capability '@tsonic/csharp-nodejs' must be installed to import Node.js built-in provider module '${moduleSpecifier}'`,
  })));
  assert.equal(Object.isFrozen(plugin.moduleOwnership), true);
  assert.equal(plugin.moduleOwnership.every(Object.isFrozen), true);
});

test("every Node alias preserves canonical declarations, default containers, module IDs and policy identities", () => {
  const plugin = packageExports.createTsonicPlugin();
  const provider = nodejsSourceProvider(["js"], plugin);
  assert.deepEqual(provider.identity, csharpNodejsProviderPackageProviderIdentity);
  assert.equal(Object.isFrozen(provider.identity), true);
  assert.equal(provider.declarationMaterialization, "complete");
  const policy = plugin.createTargetContributions({})[0];
  assert.deepEqual(policy, createCsharpNodejsProviderPolicyContribution());
  for (const { moduleSpecifier, canonicalModuleSpecifier } of nodejsProviderModuleSpecifiers()) {
    assert.deepEqual(provider.ownsModule(moduleSpecifier, {}), { kind: "owned" });
    assert.deepEqual(provider.resolveModule(moduleSpecifier, {}), {
      kind: "virtual",
      moduleSpecifier,
      providerModuleId: canonicalModuleSpecifier,
      virtualFileName: csharpNodejsVirtualDeclarationFileName(moduleSpecifier),
      evidence: [{ message: "C# NodeJS provider package supplied virtual module." }],
    });
    const model = nodejsDeclarationModel(provider, moduleSpecifier);
    assert.equal(model.providerModuleId, canonicalModuleSpecifier);
    assert.deepEqual(model.evidence, [{ message: "C# NodeJS provider package virtual declaration model." }]);
    assert.deepEqual(model.exports, expectedPublicReferences(
      nodejsCanonicalProviderExports(canonicalModuleSpecifier),
      canonicalModuleSpecifier,
      moduleSpecifier,
    ), moduleSpecifier);
    const canonicalModel = nodejsDeclarationModel(provider, canonicalModuleSpecifier);
    assert.deepEqual(model.imports, canonicalModel.imports, moduleSpecifier);
    const aliasRelations = policy.relations.filter((relation) => relation.source.moduleSpecifier === moduleSpecifier);
    const canonicalRelations = policy.relations.filter((relation) => relation.source.moduleSpecifier === canonicalModuleSpecifier);
    assert.deepEqual(aliasRelations, canonicalRelations.map((relation) => ({
      ...relation,
      source: { ...relation.source, moduleSpecifier },
    })), moduleSpecifier);
  }
  assert.equal(plugin.createTargetContributions({})[0], policy);
});

test("native and JS provider sessions remain isolated without changing native runtime requirements", () => {
  const plugin = packageExports.createTsonicPlugin();
  const nativeProvider = nodejsSourceProvider([], plugin);
  const jsProvider = nodejsSourceProvider(["js"], plugin);
  for (const specifier of ["fs", "node:fs"]) {
    const nativeStats = nodejsDeclarationModel(nativeProvider, specifier).exports.find((entry) => entry.name === "Stats");
    const jsStats = nodejsDeclarationModel(jsProvider, specifier).exports.find((entry) => entry.name === "Stats");
    assert.equal(nativeStats.members.some((member) => member.name === "mtime"), false);
    assert.equal(nativeStats.members.some((member) => member.name === "mtimeMs"), true);
    assert.deepEqual(jsStats.members.find((member) => member.name === "mtime").type, { kind: "source-global", name: "Date" });
    assert.deepEqual(nodejsDeclarationModel(nativeProvider, specifier).exports.find((entry) => entry.name === "Stats"), nativeStats);
  }
  const nativeRuntime = plugin.runtimeContributions({ selectedSurfaceIds: [] });
  assert.equal(plugin.runtimeContributions({ selectedSurfaceIds: ["js"] }), nativeRuntime);
  assert.deepEqual(nativeRuntime.references, [
    {
      kind: "assembly", include: "Tsonic.CSharp.Node",
      attributes: { HintPath: new URL("../runtimes/net10.0/Tsonic.CSharp.Node.dll", import.meta.url).pathname },
    },
    {
      kind: "assembly", include: "Tsonic.CSharp.Js",
      attributes: { HintPath: fileURLToPath(new URL("runtimes/net10.0/Tsonic.CSharp.Js.dll", import.meta.resolve("@tsonic/csharp-js/package.json"))) },
    },
    { kind: "package", include: "BouncyCastle.Cryptography", version: "2.4.0" },
    { kind: "framework", include: "Microsoft.AspNetCore.App" },
  ]);
  for (const reference of nativeRuntime.references.filter((entry) => entry.kind === "assembly")) {
    assert.equal(existsSync(reference.attributes.HintPath), true, reference.include);
    assert.doesNotMatch(reference.attributes.HintPath, /\/dist\/runtimes\//u);
  }
});

test("unowned lookalikes and mismatched canonical module IDs keep exact Node diagnostics", () => {
  const provider = nodejsSourceProvider([]);
  for (const moduleSpecifier of ["node:fs-extra", "fs/unknown", "node:assert/strict/unknown", "unrelated"]) {
    assert.deepEqual(provider.ownsModule(moduleSpecifier, {}), { kind: "unowned" });
    assert.deepEqual(provider.resolveModule(moduleSpecifier, {}), {
      extensionId: csharpNodejsProviderPackageProviderIdentity.id,
      extensionCode: "NODEJS_PROVIDER_PACKAGE_MODULE_UNOWNED",
      numericCode: 9300001,
      category: "error",
      message: `C# NodeJS provider package does not own '${moduleSpecifier}'.`,
    });
    assert.deepEqual(provider.getDeclarationModel({ kind: "virtual", moduleSpecifier, providerModuleId: "node:fs", virtualFileName: "ignored" }, {}), missingDiagnostic(moduleSpecifier));
  }
  const resolution = provider.resolveModule("fs", {});
  assert.deepEqual(provider.getDeclarationModel({ ...resolution, providerModuleId: "fs" }, {}), missingDiagnostic("fs"));
});

test("the real TSTS/Node capability path compiles native default, named and namespace aliases", () => {
  const compiled = compileCsharpSource({
    capabilities: [packageExports.createTsonicPlugin()],
    sourceText: `
      import fs from "fs";
      import * as canonicalFs from "node:fs";
      import path from "path";
      import { join } from "node:path";

      export function inspect(root: string): number {
        const first = fs.statSync(join(root, "first"));
        return first.mtimeMs + canonicalFs.statSync(path.join(root, "second")).mtimeMs;
      }
    `,
  });
  assertCsharpCompilationSucceeded(compiled);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(source, /Tsonic\.CSharp\.Node\.fs\.statSync/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.path\.join/u);
  assert.match(source, /\.mtimeMs/u);
});

test("the real native TSTS/Node capability path rejects JS-only members without emitting artifacts", () => {
  const compiled = compileCsharpSource({
    capabilities: [packageExports.createTsonicPlugin()],
    sourceText: `
      import { statSync } from "fs";
      export const stamp = statSync("file.txt").mtime;
    `,
  });
  assert.match(compiled.sourceDiagnosticsText, /Property 'mtime' does not exist on type/u);
  assert.equal(compiled.artifacts.size, 0);
});

function missingDiagnostic(moduleSpecifier) {
  return {
    extensionId: csharpNodejsProviderPackageProviderIdentity.id,
    extensionCode: "NODEJS_PROVIDER_PACKAGE_MODULE_MISSING",
    numericCode: 9300002,
    category: "error",
    message: `C# NodeJS provider package has no declaration model for '${moduleSpecifier}'.`,
  };
}

function expectedPublicReferences(value, canonicalModuleSpecifier, publicModuleSpecifier) {
  if (Array.isArray(value)) {
    return value.map((entry) => expectedPublicReferences(entry, canonicalModuleSpecifier, publicModuleSpecifier));
  }
  if (value === null || typeof value !== "object") return value;
  return Object.fromEntries(Object.entries(value).map(([key, entry]) => [
    key,
    value.kind === "provider-ref" && key === "moduleSpecifier" && entry === canonicalModuleSpecifier
      ? publicModuleSpecifier
      : expectedPublicReferences(entry, canonicalModuleSpecifier, publicModuleSpecifier),
  ]));
}
