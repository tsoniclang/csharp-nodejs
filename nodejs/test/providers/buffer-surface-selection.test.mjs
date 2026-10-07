import assert from "node:assert/strict";
import test from "node:test";
import { nodejsCanonicalProviderExports } from "../../../dist/provider/assembly/modules.js";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";
import { nodeBufferFromUint8ArraySignatureId } from "../../../dist/provider/modules/buffer/identities.js";

for (const enabled of [false, true]) {
  test(`Buffer typed-array declarations and exact relations follow the selected JS surface (${enabled})`, () => {
    const declarations = nodejsCanonicalProviderExports("node:buffer", enabled);
    const buffer = declarations.find(declaration => declaration.name === "Buffer");
    const from = buffer.members.find(member => member.name === "from");
    assert.equal(from.signatures.some(signature => signature.id === nodeBufferFromUint8ArraySignatureId), enabled);
    assert.equal(from.signatures.length, enabled ? 4 : 3);
    const relations = nodejsProviderTargetRelations(enabled);
    assert.equal(relations.some(relation => relation.kind === "signature" &&
      relation.source.signatureId === nodeBufferFromUint8ArraySignatureId), enabled);
    assert.equal(from.signatures.filter(signature => signature.parameters[0].type.kind === "source-global").length,
      enabled ? 1 : 0);
  });
}
