import assert from "node:assert/strict";
import test from "node:test";
import {
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
} from "../../../dist/provider/declarations/exports.js";
import { nodejsClassCallTargetMetadata } from "../../../dist/provider/declarations/target-members.js";
import { nodejsCanonicalProviderExports } from "../../../dist/provider/assembly/modules.js";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";

test("every owned Node constructor publishes its exact public native and source accessibility", () => {
  const constructors = nodejsProviderTargetRelations().filter(relation =>
    relation.kind === "signature" && relation.targetMember.kind === "constructor"
  );
  const signatures = new Set();
  assert.equal(constructors.length, 18, "nine physical signatures have two module spellings");
  for (const relation of constructors) {
    signatures.add(relation.source.signatureId);
    assert.equal(relation.targetMember.csharpConstructorAccessibility, "public", relation.targetMember.id);
    const declaration = nodejsCanonicalProviderExports(relation.source.providerModuleId)
      ?.find(candidate => candidate.id === relation.source.exportId);
    const signature = declaration?.members?.flatMap(member => member.signatures ?? [])
      .find(candidate => candidate.id === relation.source.signatureId);
    assert.equal(signature?.accessibility, "public", relation.source.signatureId);
  }
  assert.deepEqual([...signatures].sort(), [
    "node:events.EventEmitter.constructor()",
    "node:net.Server.constructor()",
    "node:net.Socket.constructor()",
    "node:url.URL.constructor(System.String,System.String)",
    "node:url.URL.constructor(System.String,Tsonic.CSharp.Node.URL)",
    "node:url.URLSearchParams.constructor(System.String)",
    "node:util.TextDecoder.constructor()",
    "node:worker_threads.MessageChannel.constructor()",
    "node:worker_threads.Worker.constructor(modulePath,options)",
  ].sort());
});

test("constructor metadata preserves the explicit accessibility of each physical overload", () => {
  const target = nodejsTargetNamedType("Fixture", "Owned");
  const calls = ["public", "protected", "protected-internal"].map(accessibility =>
    nodejsClassCallTargetMetadata({
      exportName: "Owned",
      memberName: "constructor",
      memberId: "fixture.Owned.constructor",
      signatureId: `fixture.Owned.constructor.${accessibility}`,
      targetMemberId: `Fixture.Owned..ctor.${accessibility}`,
      sourceName: "constructor",
      targetName: "Owned",
      memberKind: "constructor",
      csharpConstructorAccessibility: accessibility,
      providerParameters: [],
      targetParameters: [],
      targetReturnType: target,
      declaringType: target,
    })
  );
  const exports = nodejsCapabilityModuleExports({
    moduleSpecifier: "fixture",
    classCalls: calls,
    includeDefaultExports: false,
  });
  const signatures = exports[0].members[0].signatures;
  for (const [index, accessibility] of ["public", "protected", "protected-internal"].entries()) {
    assert.equal(calls[index].member.csharpConstructorAccessibility, accessibility, `target overload ${index}`);
    assert.equal(signatures[index].accessibility, accessibility, `source overload ${index}`);
  }
});
