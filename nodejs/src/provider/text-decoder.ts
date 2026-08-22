import type {
  ProviderExportDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
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
  nodejsClassCallTargetMetadata,
  nodejsClassPropertyTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
} from "./members/target-member-metadata.js";

export const nodeUtilTextDecoderExportName = "TextDecoder";

const stringProviderType = { kind: "string" } satisfies ProviderTypeExpression;
const boolProviderType = { kind: "boolean" } satisfies ProviderTypeExpression;
const bufferProviderType = {
  kind: "provider-ref",
  moduleSpecifier: "node:buffer",
  exportName: "Buffer",
} satisfies ProviderTypeExpression;
const stringTargetType = csharpStringTargetType();
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const textDecoderTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.TextDecoder",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "TextDecoder"),
);

export function nodeUtilTextDecoderExportDeclaration(): ProviderExportDeclaration {
  const calls = nodeUtilTextDecoderClassCallTargetMembers();
  const callsByMember = new Map<string, readonly NodejsClassCallTargetMetadata[]>();
  for (const call of calls) {
    callsByMember.set(call.memberId, [...callsByMember.get(call.memberId) ?? [], call]);
  }
  return {
    id: "node:util.TextDecoder",
    name: nodeUtilTextDecoderExportName,
    kind: "class",
    members: [
      ...[...callsByMember.values()].map((group) => ({
        id: group[0]!.memberId,
        name: group[0]!.memberName,
        kind: group[0]!.memberKind,
        signatures: group.map((entry) => ({
          id: entry.signatureId,
          parameters: entry.providerParameters,
          ...(entry.providerReturnType === undefined ? {} : { returnType: entry.providerReturnType }),
        })),
      })),
      ...nodeUtilTextDecoderClassPropertyTargetMembers().map((entry) => ({
        id: entry.memberId,
        name: entry.memberName,
        kind: "property" as const,
        readonly: true as const,
        type: entry.providerType,
      })),
    ],
  };
}

export function nodeUtilTextDecoderClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const constructor = (
    signatureId: string,
    targetMemberId: string,
    providerParameters: readonly ProviderParameterDeclaration[],
    targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  ): NodejsClassCallTargetMetadata => nodejsClassCallTargetMetadata({
    exportName: nodeUtilTextDecoderExportName,
    memberName: "constructor",
    memberId: "node:util.TextDecoder.constructor",
    signatureId,
    targetMemberId,
    sourceName: "constructor",
    targetName: "TextDecoder",
    memberKind: "constructor",
    providerParameters,
    targetParameters,
    targetReturnType: textDecoderTargetType,
    declaringType: textDecoderTargetType,
  });
  return [
    constructor("node:util.TextDecoder.constructor()", "Tsonic.CSharp.Node.TextDecoder..ctor()", [], []),
    nodejsClassCallTargetMetadata({
      exportName: nodeUtilTextDecoderExportName,
      memberName: "decode",
      memberId: "node:util.TextDecoder.decode",
      signatureId: "node:util.TextDecoder.decode(Tsonic.CSharp.Node.Buffer)",
      targetMemberId: "Tsonic.CSharp.Node.TextDecoder.decode(Tsonic.CSharp.Node.Buffer)",
      sourceName: "decode",
      targetName: "decode",
      memberKind: "method",
      providerParameters: [{ name: "input", type: bufferProviderType }],
      providerReturnType: stringProviderType,
      targetParameters: [targetParameter("input", nodeBufferTargetType)],
      targetReturnType: stringTargetType,
      declaringType: textDecoderTargetType,
    }),
  ];
}

export function nodeUtilTextDecoderClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    decoderProperty("encoding", stringProviderType, stringTargetType),
    decoderProperty("fatal", boolProviderType, boolTargetType),
    decoderProperty("ignoreBOM", boolProviderType, boolTargetType),
  ];
}

function decoderProperty(
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName: nodeUtilTextDecoderExportName,
    memberName,
    memberId: `node:util.TextDecoder.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.TextDecoder.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType: textDecoderTargetType,
    readonly: true,
  });
}
