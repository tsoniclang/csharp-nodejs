import assert from "node:assert/strict";
import test from "node:test";
import { nodejsCanonicalProviderExports } from "../../../dist/provider/assembly/modules.js";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";
import { nodejsProviderTargetTypeRows } from "../../../dist/provider/modules/target-bindings.js";

test("native NodeError exposes its readonly native Error contract", () => {
  const declaration = nodejsCanonicalProviderExports("node:util").find(entry => entry.name === "NodeError");
  assert.equal(declaration.kind, "interface");
  assert.deepEqual(declaration.heritage, [{ kind: "extends", type: { kind: "source-global", name: "Readonly",
    typeArguments: [{ kind: "source-global", name: "Error" }] } }]);
  assert.deepEqual(declaration.members, [{
    id: "node:util.NodeError.message", name: "message", kind: "property",
    readonly: true, type: { kind: "string" },
  }]);
  const row = nodejsProviderTargetTypeRows.find(entry =>
    entry.moduleSpecifier === "node:util" && entry.exportName === "NodeError");
  assert.equal(row.namespace, "System");
  assert.equal(row.targetName, "Exception");
  assert.equal(row.targetType().csharpThrowable, true);
  const relations = nodejsProviderTargetRelations().filter(entry =>
    entry.kind === "member" && entry.source.moduleSpecifier === "node:util" &&
    entry.source.memberId === "node:util.NodeError.message");
  assert.equal(relations.length, 1);
  assert.equal(relations[0].targetMember.id, "System.Exception.Message");
});

test("compression callbacks preserve independent native error and result absence", () => {
  const declarations = nodejsCanonicalProviderExports("node:zlib");
  for (const name of ["gzip", "gunzip", "deflate", "inflate", "deflateRaw", "inflateRaw",
    "unzip", "brotliCompress", "brotliDecompress"]) {
    const signatures = declarations.find(entry => entry.name === name).signatures;
    assert.equal(signatures.length, 2);
    for (const signature of signatures) {
      const callback = signature.parameters.at(-1).type;
      assert.deepEqual(callback.parameters.map(parameter => parameter.type), [
        { kind: "union", types: [
          { kind: "provider-ref", moduleSpecifier: "node:util", exportName: "NodeError" },
          { kind: "undefined" },
        ] },
        { kind: "union", types: [
          { kind: "provider-ref", moduleSpecifier: "node:buffer", exportName: "Buffer" },
          { kind: "undefined" },
        ] },
      ]);
    }
  }
});
