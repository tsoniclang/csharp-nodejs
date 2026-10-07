import assert from "node:assert/strict";
import test from "node:test";
import { nodejsCanonicalProviderExports } from "../../../dist/provider/assembly/modules.js";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";

test("timer cancellation retains exact Timeout declarations and native operations", () => {
  const declarations = nodejsCanonicalProviderExports("node:timers");
  const relations = nodejsProviderTargetRelations();
  for (const name of ["clearTimeout", "clearInterval"]) {
    const declaration = declarations.find(row => row.name === name && row.exportKind !== "default");
    assert.deepEqual(declaration.signatures, [{
      id: `node:timers.${name}(Timeout)`,
      parameters: [{ name: "timeout", type: { kind: "provider-ref", moduleSpecifier: "node:timers", exportName: "Timeout" } }],
      returnType: { kind: "void" },
    }]);
    const operations = relations.filter(row => row.kind === "signature" && row.source.exportName === name &&
      row.source.signatureId === `node:timers.${name}(Timeout)`);
    assert.deepEqual(operations.map(row => row.source.moduleSpecifier).sort(), ["node:timers", "timers"]);
    for (const operation of operations) {
      assert.equal(operation.targetMember.id, `Tsonic.CSharp.Node.timers.${name}(Tsonic.CSharp.Node.Timeout)`);
      assert.equal(operation.targetMember.parameters[0].type.id, "Tsonic.CSharp.Node.Timeout");
      assert.equal(operation.targetMember.returnType.csharpSpecialType, "void");
    }
  }
});
