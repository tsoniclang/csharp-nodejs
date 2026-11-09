import type { ProviderExportDeclaration } from "@tsonic/tsts";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../../model/target-members.js";
import { nativeCall, nativeOwner, nativeParameter, nativeSlot } from "../../native/authoring.js";
import { nativeNamedType } from "../../native/locators.js";
import { stringProviderType, boolProviderType } from "../../model/source-types.js";

export const nodeUtilTextDecoderExportName = "TextDecoder";
const owner = nativeOwner("Tsonic.CSharp.Node.TextDecoder");
const bufferProviderType = { kind: "provider-ref", moduleSpecifier: "node:buffer", exportName: "Buffer" } as const;

export function nodeUtilTextDecoderClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    { exportName: nodeUtilTextDecoderExportName, memberName: "constructor", memberId: "node:util.TextDecoder.constructor",
      signatureId: "node:util.TextDecoder.constructor()", memberKind: "constructor", accessibility: "public", providerParameters: [],
      native: nativeCall({ owner, kind: "constructor", targetName: ".ctor", static: false, parameters: [],
        returnType: nativeNamedType(owner.metadataName), receiver: { kind: "none" } }) },
    { exportName: nodeUtilTextDecoderExportName, memberName: "decode", memberId: "node:util.TextDecoder.decode",
      signatureId: "node:util.TextDecoder.decode(Tsonic.CSharp.Node.Buffer)", memberKind: "method",
      providerParameters: [{ name: "input", type: bufferProviderType }], providerReturnType: stringProviderType,
      native: nativeCall({ owner, kind: "method", targetName: "decode", static: false,
        parameters: [nativeParameter(0, nativeNamedType("Tsonic.CSharp.Node.Buffer"))], returnType: { kind: "string" }, receiver: { kind: "instance" } }) },
  ];
}

export function nodeUtilTextDecoderClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    { exportName: nodeUtilTextDecoderExportName, memberName: "encoding", memberId: "node:util.TextDecoder.encoding",
      memberKind: "property", providerType: stringProviderType, readonly: true, native: nativeSlot(owner, "encoding", { kind: "string" }) },
    ...["fatal", "ignoreBOM"].map(memberName => ({ exportName: nodeUtilTextDecoderExportName, memberName, memberId: `node:util.TextDecoder.${memberName}`,
      memberKind: "property" as const, providerType: boolProviderType, readonly: true as const,
      native: nativeSlot(owner, memberName, { kind: "source-primitive", name: "bool" }) })),
  ];
}

export function nodeUtilTextDecoderExportDeclaration(): ProviderExportDeclaration {
  return { id: "node:util.TextDecoder", name: nodeUtilTextDecoderExportName, kind: "class", members: [
    ...nodeUtilTextDecoderClassCallTargetMembers().map(entry => ({ id: entry.memberId, name: entry.memberName, kind: entry.memberKind,
      signatures: [{ id: entry.signatureId, parameters: entry.providerParameters,
        ...entry.accessibility === undefined ? {} : { accessibility: entry.accessibility },
        ...entry.providerReturnType === undefined ? {} : { returnType: entry.providerReturnType } }] })),
    ...nodeUtilTextDecoderClassPropertyTargetMembers().map(entry => ({ id: entry.memberId, name: entry.memberName, kind: "property" as const,
      readonly: true as const, type: entry.providerType })),
  ] };
}

