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
