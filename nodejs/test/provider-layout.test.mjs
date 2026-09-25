import assert from "node:assert/strict";
import { dirname, resolve } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { readSourceInventory } from "../../../tsonic/test/architecture/tooling/file-inventory.mjs";
import { evaluateNodeProviderContract } from "../../../tsonic/test/architecture/tooling/node-provider-contract.mjs";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../..");

test("Csharp Node provider follows the shared module, ownership and public SDK contract", () => {
  const sources = readSourceInventory(repositoryRoot, {
    extensions: [".ts"],
    include: ["nodejs/src"],
    exclude: ["dist", "node_modules", ".analysis", ".temp"],
  });
  const providerSources = new Map([...sources].filter(([path]) => path.startsWith("nodejs/src/")));
  assert.deepEqual(evaluateNodeProviderContract(providerSources, {
    targetPackage: "@tsonic/target-csharp",
    factoryName: "createCsharpProviderPackage",
  }), []);
});
