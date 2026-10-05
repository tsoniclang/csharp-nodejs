import assert from "node:assert/strict";
import test from "node:test";
import { nodejsSourceProvider, nodejsDeclarationModel } from "../helpers/provider-package.mjs";
import { nodeStreamClassCallTargetMembers } from "../../../dist/provider/modules/stream.js";

test("inherited stream operations retain the native class hierarchy and owning base declarations", () => {
  const provider = nodejsSourceProvider(["js"]);
  for (const [module, name, parent] of [
    ["node:fs", "WriteStream", ["node:stream", "Writable"]],
    ["node:stream", "Transform", ["node:stream", "Duplex"]],
    ["node:zlib", "ZlibTransform", ["node:stream", "Transform"]],
  ]) {
    const declaration = nodejsDeclarationModel(provider, module).exports.find(row => row.name === name);
    assert.ok(declaration, name);
    assert.deepEqual(declaration.heritage, [{
      kind: "extends",
      type: { kind: "provider-ref", moduleSpecifier: parent[0], exportName: parent[1] },
    }], name);
    assert.equal(declaration.members.some(member =>
      ["on", "once", "off", "write", "end", "destroy"].includes(member.name)), false, name);
  }
  const calls = nodeStreamClassCallTargetMembers();
  for (const [owner, name] of [
    ["Writable", "on"], ["Writable", "once"], ["Writable", "off"],
    ["Writable", "write"], ["Writable", "end"], ["Duplex", "destroy"],
  ]) {
    const selected = calls.filter(row => row.exportName === owner && row.memberName === name);
    assert.ok(selected.length > 0, `${owner}.${name}`);
    assert.equal(selected.every(row => row.member.declaringType.id === `Tsonic.CSharp.Node.${owner}`), true,
      `${owner}.${name}`);
  }
});
