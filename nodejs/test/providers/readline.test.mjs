import assert from "node:assert/strict";
import test from "node:test";
import { csharpNullableTargetType } from "@tsonic/target-csharp/provider";
import { nodejsTargetNamedType } from "../../../dist/provider/declarations/exports.js";
import { nodeReadlineClassPropertyTargetMembers } from "../../../dist/provider/modules/readline.js";

test("optional readline output retains its native nullable Writable storage", () => {
  const rows = nodeReadlineClassPropertyTargetMembers().filter(row =>
    row.exportName === "ReadLineOptions" && row.memberName === "output");
  assert.equal(rows.length, 1);
  assert.equal(rows[0].optional, true);
  assert.deepEqual(rows[0].providerType, { kind: "provider-ref", moduleSpecifier: "node:stream", exportName: "Writable" });
  assert.deepEqual(rows[0].member.returnType, csharpNullableTargetType(nodejsTargetNamedType("Tsonic.CSharp.Node", "Writable")));
});
