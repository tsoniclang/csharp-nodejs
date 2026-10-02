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
      { kind: "implements", contract: "Tsonic.CSharp.Node.Stream" },
      { kind: "implements", contract: "Tsonic.CSharp.Node.IWritableStream" },
    ] }]);
    assert.deepEqual(relation.methodTypeParameters, [{ sourceTypeParameterIndex: 0, targetTypeParameterIndex: 0 }]);
    assert.equal(relation.selectedTypeParameterCount, 1);
  }
});

test("native stream bindings expose their actual inheritance and writable contracts", () => {
  const relations = nodejsProviderTargetRelations().filter(relation => relation.kind === "type");
  const binding = (moduleSpecifier, exportName) => relations.find(relation =>
    relation.source.providerModuleId === moduleSpecifier && relation.source.exportName === exportName)?.targetBinding;
  for (const [moduleSpecifier, exportName, baseType] of [
    ["node:stream", "Stream", "Tsonic.CSharp.Node.EventEmitter"],
    ["node:stream", "Readable", "Tsonic.CSharp.Node.Stream"],
    ["node:stream", "Writable", "Tsonic.CSharp.Node.Stream"],
    ["node:stream", "Duplex", "Tsonic.CSharp.Node.Readable"],
    ["node:stream", "Transform", "Tsonic.CSharp.Node.Duplex"],
    ["node:zlib", "ZlibTransform", "Tsonic.CSharp.Node.Transform"],
    ["node:fs", "ReadStream", "Tsonic.CSharp.Node.Readable"],
    ["node:fs", "WriteStream", "Tsonic.CSharp.Node.Writable"],
    ["node:http", "IncomingMessage", "Tsonic.CSharp.Node.Readable"],
    ["node:http", "ServerResponse", "Tsonic.CSharp.Node.Writable"],
    ["node:net", "Socket", "Tsonic.CSharp.Node.Stream"],
    ["node:tls", "TLSSocket", "Tsonic.CSharp.Node.Socket"],
  ]) {
    assert.equal(binding(moduleSpecifier, exportName)?.csharpBaseType?.id, baseType);
  }
  for (const exportName of ["Writable", "Duplex"]) {
    assert.deepEqual(binding("node:stream", exportName)?.implementedContracts, [
      { kind: "implements", contract: "Tsonic.CSharp.Node.IWritableStream" },
    ]);
  }
  assert.equal(binding("node:stream", "Readable")?.implementedContracts, undefined);
});
