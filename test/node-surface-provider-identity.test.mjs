import assert from "node:assert/strict";
import test from "node:test";

import {
  createCsharpProviderRelationCatalog,
} from "../../tsonic-csharp/dist/provider/target-relations/index.js";
import {
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";

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

function requireRelation(predicate) {
  const matches = relations.filter(predicate);
  assert.equal(matches.length, 1);
  return matches[0];
}
