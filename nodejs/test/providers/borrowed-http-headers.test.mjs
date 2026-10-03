import assert from "node:assert/strict";
import test from "node:test";
import { nodeHttpClassPropertyTargetMembers, nodeHttpClassCallTargetMembers } from "../../../dist/provider/modules/http/model.js";
import { nodeHttpExports } from "../../../dist/provider/modules/http/declarations.js";

test("HTTP header indexer exposes exact unboxed native readonly backing, not an owned mutable array", () => {
  const member = nodeHttpClassPropertyTargetMembers().find(row => row.exportName === "IncomingHttpHeaderValues" && row.memberName === "Item");
  assert.ok(member);
  assert.equal(member.readonly, true);
  assert.equal(member.member.returnType.id, "System.Nullable`1");
  const carrier = member.member.returnType.typeArguments[0];
  assert.equal(carrier.id, "Microsoft.Extensions.Primitives.StringValues");
  assert.equal(carrier.csharpValueType, true);
  assert.equal(carrier.csharpIndexableLengthMemberName, "Count");
  assert.deepEqual(carrier.csharpReadOnlyIndexableElementType, carrier.csharpEnumerableElementType);
  assert.equal(carrier.csharpDenseMutableElementType, undefined);
  const declaration = nodeHttpExports().find(entry => entry.name === "IncomingHttpHeaderValues");
  assert.deepEqual(declaration.members[0].signatures[0].returnType, member.providerType);
  assert.equal(member.providerType.types[0].name, "ReadonlyArray");
  const snapshot = nodeHttpClassCallTargetMembers().find(row => row.exportName === "IncomingHttpHeaders" && row.memberName === "getAll");
  assert.equal(snapshot.member.returnType.kind, "array");
});
