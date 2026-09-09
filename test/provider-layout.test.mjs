import assert from "node:assert/strict";
import { readFileSync, readdirSync, statSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import {
  evaluateBarrelModules,
  formatArchitectureFindings,
} from "../../tsonic/test/architecture/tooling/architecture-rules.mjs";
import {
  readSourceInventory,
} from "../../tsonic/test/architecture/tooling/file-inventory.mjs";
import {
  buildTypeScriptModuleAnalysis,
} from "../../tsonic/test/architecture/tooling/module-graph.mjs";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const providerRoot = join(repositoryRoot, "nodejs/src/provider");

test("C# Node provider files have explicit semantic ownership", () => {
  const files = sourceFiles(providerRoot);
  assert.deepEqual(
    files
      .filter((path) => /(?:^|\/)helpers\.ts$/u.test(path))
      .map((path) => relative(repositoryRoot, path)),
    [],
  );
  assert.deepEqual(
    files
      .filter((path) => readFileSync(path, "utf8").split("\n").length > 600)
      .map((path) => relative(repositoryRoot, path)),
    [],
  );
});

test("C# Node provider indexes are barrels and target imports use its provider API", () => {
  const sources = readSourceInventory(repositoryRoot, {
    extensions: [".ts"],
    exclude: ["dist", "node_modules", ".analysis", ".temp"],
  });
  const modules = buildTypeScriptModuleAnalysis(sources);
  assert.deepEqual(
    modules.edges.filter((edge) => edge.unresolved).map((edge) => `${edge.source}: ${edge.specifier}`),
    [],
  );
  const findings = evaluateBarrelModules(modules.modules, {
    allowedImplementationFiles: new Set(["nodejs/src/index.ts"]),
  });
  assert.deepEqual(findings, [], formatArchitectureFindings(findings));
  assert.deepEqual(
    modules.edges
      .filter((edge) =>
        edge.kind === "package" &&
        (edge.specifier === "@tsonic/target-csharp" ||
          edge.specifier.startsWith("@tsonic/target-csharp/")) &&
        edge.specifier !== "@tsonic/target-csharp/provider"
      )
      .map((edge) => `${edge.source}: ${edge.specifier}`),
    [],
  );
});

test("Node delegates packaged-provider transport to one SDK factory and nests all module data", () => {
  const sources = new Map(sourceFiles(join(repositoryRoot, "nodejs/src")).map((path) => [
    relative(repositoryRoot, path),
    readFileSync(path, "utf8"),
  ]));
  assert.deepEqual(packageBoundaryFindings(sources), []);
  for (const name of ["buffer", "filesystem", "http", "path", "url"]) {
    assert.equal(sources.has(`nodejs/src/provider/modules/${name}/index.ts`), true, name);
    assert.equal(sources.has(`nodejs/src/provider/modules/${name}.ts`), false, name);
  }
  assert.equal(sources.has("nodejs/src/provider/modules/util/text-decoder.ts"), true);
  assert.equal(sources.has("nodejs/src/provider/modules/util/declarations.ts"), true);
});

test("Node package boundary guard distinguishes data from local transport and stale paths", () => {
  const valid = new Map([
    ["nodejs/src/index.ts", 'import { createNodePackage } from "./provider/package.js";'],
    ["nodejs/src/provider/package.ts", "return createCsharpProviderPackage(definition);"],
    ["nodejs/src/provider/modules/http/declarations.ts", 'const moduleSpecifier = "node:http";'],
    ["nodejs/src/provider/identity.ts", "export const providerIdentity = {};"],
  ]);
  assert.deepEqual(packageBoundaryFindings(valid), []);
  for (const [file, source, expected] of [
    ["nodejs/src/provider/modules/local.ts", "registerSourceDeclarationProvider(provider);", "transport"],
    ["nodejs/src/provider/modules/local.ts", "function getDeclarationModel() {}", "transport"],
    ["nodejs/src/provider/modules/local.ts", "function rebaseProviderType(type) {}", "transport"],
    ["nodejs/src/provider/extension.ts", "export const extension = {};", "owner"],
    ["nodejs/src/provider/http.ts", "export * from './modules/http/index.js';", "owner"],
    ["nodejs/src/provider/modules/second.ts", "createCsharpProviderPackage(definition);", "factory"],
  ]) {
    const mutated = new Map(valid);
    mutated.set(file, source);
    assert.ok(packageBoundaryFindings(mutated).some((finding) => finding.includes(expected)), file);
  }
});

function packageBoundaryFindings(sources) {
  const rootOwners = new Set([
    "package.ts", "runtime.ts", "identity.ts", "metadata-indexes.ts", "target-relations.ts",
  ]);
  const findings = [];
  const factoryOwners = [];
  for (const [file, source] of sources) {
    if (/\b(?:registerSourceDeclarationProvider|SourceDeclarationProvider|ProviderOwnership|resolveModule|ownsModule|getDeclarationModel|rebase\w*Provider\w*|visitProviderType)\b/u.test(source)) {
      findings.push(`${file}: local source-provider transport`);
    }
    const factoryCalls = source.match(/\bcreateCsharpProviderPackage\s*\(/gu) ?? [];
    factoryOwners.push(...factoryCalls.map(() => file));
    if (!file.startsWith("nodejs/src/provider/")) continue;
    const ownedPath = file.slice("nodejs/src/provider/".length);
    if (!rootOwners.has(ownedPath) && !ownedPath.startsWith("modules/") && !ownedPath.startsWith("members/")) {
      findings.push(`${file}: missing coherent module or package owner`);
    }
  }
  if (factoryOwners.length !== 1 || factoryOwners[0] !== "nodejs/src/provider/package.ts") {
    findings.push(`SDK factory must have one canonical package owner: ${factoryOwners.join(", ")}`);
  }
  return findings;
}

function sourceFiles(directory) {
  return readdirSync(directory).flatMap((entry) => {
    const path = join(directory, entry);
    return statSync(path).isDirectory()
      ? sourceFiles(path)
      : path.endsWith(".ts")
        ? [path]
        : [];
  });
}
