import assert from "node:assert/strict";
import test from "node:test";
import {
  assertCsharpProviderPolicyIsNonContradictory,
  createCsharpProviderRejectionCatalog,
  createCsharpProviderRelationCatalog,
} from "../../tsonic-csharp/dist/provider/target-relations/index.js";
import {
  getCsharpNullableElementTargetType,
} from "../../tsonic-csharp/dist/index.js";
import {
  compileCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import {
  createTsonicPlugin,
} from "../dist/index.js";
import {
  nodejsProviderTargetRejections,
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";
import {
  nodejsCanonicalProviderExports,
} from "../dist/provider/provider.js";

test("Node provider relations form one contradiction-free exact catalog", () => {
  const relations = nodejsProviderTargetRelations();
  const rejections = nodejsProviderTargetRejections();
  const relationCatalog = createCsharpProviderRelationCatalog([relations]);
  const rejectionCatalog = createCsharpProviderRejectionCatalog([rejections]);

  assert.equal(relations.length, 894);
  assert.equal(rejections.length, 182);
  assert.equal(relationCatalog.relations.length, relations.length);
  assert.equal(rejectionCatalog.rejections.length, rejections.length);
  assert.doesNotThrow(() =>
    assertCsharpProviderPolicyIsNonContradictory(
      relationCatalog,
      rejectionCatalog,
    ));

  const readFile = rejections.filter((rejection) =>
    rejection.source.kind === "signature" &&
    rejection.source.providerModuleId === "node:fs" &&
    rejection.source.moduleSpecifier === "node:fs" &&
    rejection.source.exportName === "readFile" &&
    rejection.source.signatureId ===
      "node:fs.readFile(System.String,System.Object,Function)"
  );
  assert.equal(readFile.length, 1);
  assert.equal(
    readFile[0].diagnostic.extensionCode,
    "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED",
  );
});

test("Node default imports are provider-owned static containers", () => {
  const declarations = nodejsCanonicalProviderExports("node:path");
  const defaultDeclarations = declarations.filter(
    (declaration) => declaration.exportKind === "default",
  );

  assert.equal(defaultDeclarations.length, 1);
  const declaration = defaultDeclarations[0];
  assert.equal(declaration.kind, "class");
  assert.equal(declaration.id, "node:path.default");
  assert.equal(declaration.name, "NodePathModule");
  assert.ok(declaration.members.length > 0);
  assert.ok(declaration.members.every((member) => member.static === true));
  assert.equal(
    declarations.some((candidate) => candidate.kind === "interface" &&
      candidate.name === "NodePathModule"),
    false,
  );
});

test("Node class staticness comes from the exact provider declaration", () => {
  const relations = nodejsProviderTargetRelations().filter(
    (relation) => relation.kind === "signature" &&
      relation.source.signatureId ===
        "node:buffer.Buffer.from(System.String,System.String)",
  );

  assert.equal(relations.length, 2);
  for (const relation of relations) {
    assert.equal(relation.source.memberStatic, true);
    assert.equal(relation.targetMember.static, true);
    assert.deepEqual(relation.receiver, { kind: "none" });
  }
});

test("named, namespace, default, property, and class-static Node operations compile", () => {
  const compiled = compileCsharpSource({
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import path, { join } from "node:path";
      import * as pathNamespace from "node:path";
      import { Buffer } from "node:buffer";

      export function named(left: string, right: string): string {
        return join(left, right);
      }

      export function namespaced(left: string, right: string): string {
        return pathNamespace.join(left, right);
      }

      export function defaulted(left: string, right: string): string {
        return path.join(left, right) + path.sep;
      }

      export function buffered(value: string): Buffer {
        return Buffer.from(value, "utf8");
      }
    `,
  });

  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.extensionDiagnostics, []);
  assert.deepEqual(compiled.result.diagnostics, []);
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /return Tsonic\.CSharp\.Node\.path\.join\(left, right\);/u,
  );
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /return Tsonic\.CSharp\.Node\.path\.join\(left, right\) \+ Tsonic\.CSharp\.Node\.path\.sep;/u,
  );
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /return Tsonic\.CSharp\.Node\.Buffer\.from\(value, "utf8"\);/u,
  );
});

test("Node numeric API parameters preserve the source number carrier", () => {
  const compiled = compileCsharpSource({
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import * as http from "node:http";

      export function start(port: number): void {
        const server = http.createServer((_request, _response) => {});
        server.listen(port, () => {});
      }
    `,
  });

  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.extensionDiagnostics, []);
  assert.deepEqual(compiled.result.diagnostics, []);
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /server\.listen\(port, \(\) =>/u,
  );

  const relations = nodejsProviderTargetRelations().filter(
    (relation) => relation.kind === "signature" &&
      relation.source.signatureId ===
        "node:http.Server.listen(System.Double,System.Action)",
  );
  assert.ok(relations.length > 0);
  assert.ok(relations.every((relation) =>
    relation.targetMember.parameters[0]?.type.kind === "source-primitive" &&
    relation.targetMember.parameters[0].type.name === "float64"
  ));
});

test("Node provider relations declare every source-number target adapter exactly", () => {
  const expectedTargetNames = new Map([
    ["bool", "ToBoolean"],
    ["int8", "ToSByte"],
    ["uint8", "ToByte"],
    ["int16", "ToInt16"],
    ["uint16", "ToUInt16"],
    ["int32", "ToInt32"],
    ["native-int", "ToInt32"],
    ["uint32", "ToUInt32"],
    ["native-uint", "ToUInt32"],
    ["int64", "ToInt64"],
    ["uint64", "ToUInt64"],
    ["float16", "ToSingle"],
    ["float32", "ToSingle"],
    ["decimal", "ToDecimal"],
  ]);
  let adapterCount = 0;

  for (const relation of nodejsProviderTargetRelations()) {
    if (relation.kind !== "signature") continue;
    const sourceSignature = findSourceSignature(relation);
    for (const parameter of relation.parameters) {
      const sourceParameter = sourceSignature.parameters[
        parameter.sourceParameterIndex
      ];
      const targetParameter = relation.targetMember.parameters[
        parameter.targetParameterIndex
      ];
      assert.ok(sourceParameter);
      assert.ok(targetParameter);
      const resultType = getCsharpNullableElementTargetType(
        targetParameter.type,
      ) ?? targetParameter.type;
      const expectedTargetName = sourceParameter.type.kind === "number" &&
          resultType.kind === "source-primitive"
        ? expectedTargetNames.get(resultType.name)
        : undefined;
      const identity = `${relation.source.moduleSpecifier}:${relation.source.signatureId}:parameter[${parameter.sourceParameterIndex}]`;

      assert.equal(
        parameter.argumentAdapter?.targetName,
        expectedTargetName,
        identity,
      );
      if (expectedTargetName === undefined) continue;
      adapterCount += 1;
      assert.deepEqual(parameter.argumentAdapter, {
        kind: "static-method",
        id: `System.Convert.${expectedTargetName}(System.Double)`,
        declaringType: {
          kind: "target-named",
          id: "System.Convert",
          csharpRender: {
            kind: "named",
            namespace: ["System"],
            name: "Convert",
          },
        },
        targetName: expectedTargetName,
        inputType: { kind: "source-primitive", name: "float64" },
        resultType,
      }, identity);
    }
  }

  assert.equal(adapterCount, 244);
});

function findSourceSignature(relation) {
  const declarations = nodejsCanonicalProviderExports(
    relation.source.providerModuleId,
  ) ?? [];
  const declaration = declarations.find((candidate) =>
    candidate.id === relation.source.exportId
  );
  assert.ok(declaration, `missing source export ${relation.source.exportId}`);
  const signatures = relation.source.memberId === undefined
    ? declaration.signatures
    : declaration.members?.find((member) =>
        member.id === relation.source.memberId
      )?.signatures;
  const signature = signatures?.find((candidate) =>
    candidate.id === relation.source.signatureId
  );
  assert.ok(signature, `missing source signature ${relation.source.signatureId}`);
  return signature;
}
