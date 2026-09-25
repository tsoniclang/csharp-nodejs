import assert from "node:assert/strict";
import { dirname, resolve } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { readSourceInventory } from "../../../../tsonic/test/architecture/tooling/file-inventory.mjs";
import { evaluateNodeProviderContract } from "../../../../tsonic/test/architecture/tooling/node-provider-contract.mjs";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../../..");

test("Csharp Node provider follows the shared module, ownership and public SDK contract", () => {
  const sources = readSourceInventory(resolve(repositoryRoot, "nodejs/src"), {
    extensions: [".ts"],
  });
  const providerSources = new Map([...sources].map(([path, source]) => [`nodejs/src/${path}`, source]));
  assert.deepEqual(evaluateNodeProviderContract(providerSources, {
    targetPackage: "@tsonic/target-csharp",
    factoryName: "createCsharpProviderPackage",
  }), []);
});
