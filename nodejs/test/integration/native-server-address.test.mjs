import assert from "node:assert/strict";
import test from "node:test";
import { assertCsharpCompilationSucceeded, compileCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";
import { nodejsCanonicalProviderExports } from "../../../dist/provider/assembly/modules.js";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";

test("every HTTP class declaration has an exact native type binding", () => {
  const relations = nodejsProviderTargetRelations().filter(relation => relation.kind === "type");
  for (const declaration of nodejsCanonicalProviderExports("node:http")) {
    if (declaration.kind !== "class" || declaration.exportKind === "default") continue;
    for (const moduleSpecifier of ["node:http", "http"]) {
      const selected = relations.filter(relation => relation.source.moduleSpecifier === moduleSpecifier &&
        relation.source.exportName === declaration.name);
      assert.equal(selected.length, 1, `${moduleSpecifier}:${declaration.name}`);
      assert.equal(selected[0].targetBinding.csharpType.id, selected[0].targetBinding.id);
    }
  }
});

test("HTTP address properties retain optional native getters and integer ports", () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()], sourceText: `
    import type { ServerAddress, AddressInfo } from "node:http";
    import type { int32 } from "@tsonic/core/types.js";
    export function port(value: ServerAddress): int32 | undefined { return value.port; }
    export function path(value: ServerAddress): string | undefined { return value.path; }
    export function address(value: ServerAddress): AddressInfo | undefined { return value.address; }
  ` });
  assertCsharpCompilationSucceeded(compiled);
  const output = [...compiled.artifacts.values()].join("\n");
  assert.match(output, /int\? port/u);
  for (const getter of ["port", "path", "address"]) assert.match(output, new RegExp(`\\.${getter}\\b`));
  assert.doesNotMatch(output, /\(double\)/u);
});
