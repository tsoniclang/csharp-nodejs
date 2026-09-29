import assert from "node:assert/strict";
import test from "node:test";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";

test("Readable pipe preserves its destination binding through parameters, result and requirements", () => {
  const relations = nodejsProviderTargetRelations().filter(relation =>
    relation.kind === "signature" && relation.source.signatureId === "node:stream.Readable.pipe(destination)");
  assert.equal(relations.length, 2);
  assert.deepEqual(relations.map(relation => relation.source.moduleSpecifier).sort(), ["node:stream", "stream"]);
  const identity = "node:stream:Readable:pipe:TDestination";
  const carrier = { kind: "type-parameter", identity, name: "TDestination" };
  for (const relation of relations) {
    assert.deepEqual(relation.targetMember.parameters.map(parameter => parameter.type), [carrier]);
    assert.deepEqual(relation.targetMember.returnType, carrier);
    assert.deepEqual(relation.targetMember.typeParameters, [{ identity, name: "TDestination", constraints: [
      { kind: "implements", contract: "Tsonic.CSharp.Node.Writable" },
    ] }]);
    assert.deepEqual(relation.methodTypeParameters, [{ sourceTypeParameterIndex: 0, targetTypeParameterIndex: 0 }]);
    assert.equal(relation.selectedTypeParameterCount, 1);
  }
});
