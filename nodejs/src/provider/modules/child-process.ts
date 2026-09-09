import type {
  ProviderExportDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpNullableValueTargetType,
  csharpQualifiedTypeRenderShape,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpTargetNamedType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type { TargetTypeRef } from "@tsonic/target-csharp/provider";
import {
  nodeBufferTargetType,
} from "./buffer/identities.js";
import {
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";
import type {
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";
import {
  nodejsDefaultModuleObjectExports,
} from "./defaults.js";

export const nodeChildProcessModuleSpecifier = "node:child_process";
export const nodeChildProcessSpawnSyncReturnsExportName = "SpawnSyncReturns";

const stringProviderType = { kind: "string" } satisfies ProviderTypeExpression;
const numberProviderType = { kind: "number" } satisfies ProviderTypeExpression;
const nullProviderType = { kind: "literal", value: null } satisfies ProviderTypeExpression;
const bufferProviderType = {
  kind: "provider-ref",
  moduleSpecifier: "node:buffer",
  exportName: "Buffer",
} satisfies ProviderTypeExpression;
const resultTypeParameter = {
  kind: "type-parameter",
  name: "T",
} satisfies ProviderTypeExpression;
const resultProviderType = {
  kind: "provider-ref",
  moduleSpecifier: nodeChildProcessModuleSpecifier,
  exportName: nodeChildProcessSpawnSyncReturnsExportName,
  typeArguments: [bufferProviderType],
} satisfies ProviderTypeExpression;
const stringArrayProviderType = {
  kind: "array",
  elementType: stringProviderType,
} satisfies ProviderTypeExpression;
const nullableNumberProviderType = {
  kind: "union",
  types: [numberProviderType, nullProviderType],
} satisfies ProviderTypeExpression;
const spawnSyncSignatureId =
  "node:child_process.spawnSync(System.String,System.String[])";
const spawnSyncProviderParameters = [
  { name: "command", type: stringProviderType },
  { name: "args", type: stringArrayProviderType },
] satisfies readonly ProviderParameterDeclaration[];

const stringTargetType = csharpStringTargetType();
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const jsStringArrayTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Js.JSArray`1",
  [stringTargetType],
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Js", "JSArray"),
  {
    arrayLiteralElementType: stringTargetType,
    enumerableElementType: stringTargetType,
    readOnlyIndexableElementType: stringTargetType,
    denseMutableElementType: stringTargetType,
    indexableLengthMemberName: "length",
    collectionSemantics: "js-sparse",
  },
);
const resultTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.SpawnSyncResult",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "SpawnSyncResult"),
);
const childProcessTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.child_process",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "child_process"),
);

export function nodeChildProcessExports(): readonly ProviderExportDeclaration[] {
  const exports = [
    {
      id: `node:child_process.${nodeChildProcessSpawnSyncReturnsExportName}`,
      name: nodeChildProcessSpawnSyncReturnsExportName,
      kind: "interface" as const,
      typeParameters: [{ name: "T" }],
      members: nodeChildProcessClassPropertyTargetMembers().map((entry) => ({
        id: entry.memberId,
        name: entry.memberName,
        kind: "property" as const,
        type: entry.providerType,
      })),
    },
    {
      id: "node:child_process.spawnSync",
      name: "spawnSync",
      kind: "function" as const,
      signatures: [{
        id: spawnSyncSignatureId,
        parameters: spawnSyncProviderParameters,
        returnType: resultProviderType,
      }],
    },
  ];
  return [
    ...exports,
    ...nodejsDefaultModuleObjectExports(nodeChildProcessModuleSpecifier, exports),
  ];
}

export function nodeChildProcessCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  const targetMember = (
    targetMemberId: string,
    argumentType: TargetTypeRef,
  ): NodejsModuleCallTargetMetadata => nodejsModuleCallTargetMetadata({
    exportName: "spawnSync",
    signatureId: spawnSyncSignatureId,
    targetMemberId,
    sourceName: "spawnSync",
    targetName: "spawnSyncResult",
    providerParameters: spawnSyncProviderParameters,
    providerReturnType: resultProviderType,
    targetParameters: [
      targetParameter("command", stringTargetType),
      targetParameter("args", argumentType),
    ],
    targetReturnType: resultTargetType,
    declaringType: childProcessTargetType,
  });
  return [
    targetMember(
      "Tsonic.CSharp.Node.child_process.spawnSyncResult(System.String,System.String[])",
      { kind: "array", element: stringTargetType },
    ),
    targetMember(
      "Tsonic.CSharp.Node.child_process.spawnSyncResult(System.String,Tsonic.CSharp.Js.JSArray`1)",
      jsStringArrayTargetType,
    ),
  ];
}

export function nodeChildProcessClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    childProcessResultProperty("stdout", resultTypeParameter, nodeBufferTargetType),
    childProcessResultProperty("stderr", resultTypeParameter, nodeBufferTargetType),
    childProcessResultProperty("status", nullableNumberProviderType, csharpNullableValueTargetType(intTargetType)),
  ];
}

function childProcessResultProperty(
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName: nodeChildProcessSpawnSyncReturnsExportName,
    memberName,
    memberId: `node:child_process.SpawnSyncReturns.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.SpawnSyncResult.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType: resultTargetType,
  });
}
