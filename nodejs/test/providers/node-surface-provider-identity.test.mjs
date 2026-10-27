import assert from "node:assert/strict";
import test from "node:test";

import {
  createCsharpProviderRelationCatalog,
} from "../../../../tsonic-csharp/dist/public/provider.js";
import {
  nodejsProviderTargetRelations,
} from "../../../dist/provider/target-relations.js";
import { nodeProcessExports } from "../../../dist/provider/modules/process/declarations.js";

const relations = nodejsProviderTargetRelations();
const catalog = createCsharpProviderRelationCatalog([relations]);
const mtime = requireRelation((relation) =>
  relation.kind === "member" &&
  relation.source.providerModuleId === "node:fs" &&
  relation.source.moduleSpecifier === "node:fs" &&
  relation.source.memberId === "node:fs.Stats.mtime"
);
const isFile = requireRelation((relation) =>
  relation.kind === "signature" &&
  relation.source.providerModuleId === "node:fs" &&
  relation.source.moduleSpecifier === "node:fs" &&
  relation.source.signatureId === "node:fs.Stats.isFile()"
);

test("Node provider relations resolve exact member and signature identities", () => {
  assert.deepEqual(catalog.resolveMember(mtime.source), [mtime]);
  assert.deepEqual(catalog.resolveSignature(isFile.source), [isFile]);
});

test("Node provider relations cannot select a same-spelling sibling", () => {
  assert.deepEqual(catalog.resolveSignature({
    ...isFile.source,
    signatureId: "node:fs.OtherStats.isFile()",
  }), []);
});

test("Node provider relations reject contradictory staticness", () => {
  assert.deepEqual(catalog.resolveMember({
    ...mtime.source,
    memberStatic: !mtime.source.memberStatic,
  }), []);
});

test("Node provider relations reject contradictory property keys", () => {
  assert.deepEqual(catalog.resolveMember({
    ...mtime.source,
    memberKey: { kind: "string", name: "sameSpellingElsewhere" },
  }), []);
});

test("process.kill groups three exact typed overloads without a boxed signal", () => {
  const exports = nodeProcessExports().filter(declaration => declaration.name === "kill");
  assert.equal(exports.length, 1, "one canonical process.kill export");
  assert.equal(exports[0].kind, "function");
  assert.equal(exports[0].signatures.length, 3);
  const killRelations = relations.filter(relation => relation.kind === "signature" &&
    relation.source.moduleSpecifier === "node:process" &&
    relation.source.exportName === "kill");
  assert.equal(killRelations.length, 3, "one native relation per source overload");
  assert.deepEqual(killRelations.map(relation => relation.source.signatureId).sort(), [
    "node:process.kill(TPid,System.Nullable`1)",
    "node:process.kill(TPid,System.String)",
    "node:process.kill(TPid,TSignal)",
  ], "exact typed process.kill source signatures");
  for (const [targetName, arity, genericArity] of [["kill", 2, 1], ["killNamed", 2, 1], ["killNumber", 2, 2]]) {
    const matches = killRelations.filter(relation => relation.targetMember.targetName === targetName);
    assert.equal(matches.length, 1, targetName);
    const relation = matches[0];
    assert.equal(relation.targetMember.parameters.length, arity, `${targetName} exact parameters`);
    assert.equal(relation.selectedTypeParameterCount, genericArity, `${targetName} source generic inputs`);
    assert.equal(relation.targetMember.typeParameters.length, genericArity, `${targetName} native generic inputs`);
    assert.equal(relation.targetMember.parameters[0].type.kind, "type-parameter", `${targetName} exact PID input`);
    for (const parameter of relation.targetMember.typeParameters) {
      assert.equal(parameter.csharpAllowsRefLike, false, `${targetName} numeric constraint`);
      assert.equal(parameter.constraints.length, 1);
      assert.equal(parameter.constraints[0].kind, "implements");
      const contract = parameter.constraints[0].contract;
      assert.equal(contract.kind, "target-named");
      assert.equal(contract.id, "System.Numerics.INumberBase`1");
      assert.equal(contract.typeArguments[0].identity, parameter.identity);
      assert.deepEqual(contract.csharpAllowsRefLikeTypeArguments, [false]);
      assert.deepEqual(contract.csharpRender, { kind: "named", namespace: ["System", "Numerics"], name: "INumberBase" });
    }
    if (targetName === "killNamed")
      assert.equal(relation.targetMember.parameters[1].type.id, "System.String");
    if (targetName === "killNumber")
      assert.equal(relation.targetMember.parameters[1].type.kind, "type-parameter");
    if (targetName === "kill") {
      const source = exports[0].signatures.find(signature => signature.id === relation.source.signatureId);
      assert.equal(source !== undefined, true, "actual default/absence source overload");
      assert.equal(source.parameters[1].type.kind, "undefined", "explicit source undefined retains the default overload");
      assert.equal(source.parameters[1].optional, true, "source omission remains supported");
      assert.equal(relation.targetMember.parameters[1].optional, true, "the actual native nullable slot is optional");
      assert.equal(relation.targetMember.parameters[1].type.id, "System.Nullable`1", "canonical native absence carrier");
      assert.equal(relation.targetMember.parameters[1].type.typeArguments[0].name, "int32", "native signal remains int32");
    }
    assert.equal(relation.targetMember.parameters.some(parameter => parameter.type.id === "System.Object"), false);
  }
  assert.equal(relations.some(relation => relation.kind === "signature" &&
    relation.source.signatureId === "node:process.kill(System.Int32,System.Object)"), false,
  "removed boxed API has no provider relation");
});

function requireRelation(predicate) {
  const matches = relations.filter(predicate);
  assert.equal(matches.length, 1);
  return matches[0];
}
