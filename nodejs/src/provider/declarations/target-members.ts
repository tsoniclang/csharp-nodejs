import type { CsharpTargetMember, TargetParameter, TargetTypeRef } from "@tsonic/target-csharp/provider";
import type { CsharpTargetInvocation, NodejsModuleCallTargetMetadata, NodejsModulePropertyTargetMetadata, NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadataRow, NodejsModulePropertyTargetMetadataRow, NodejsClassCallTargetMetadataRow, NodejsClassPropertyTargetMetadataRow } from "../model/target-members.js";

export function nodejsModuleCallTargetMetadata(
  row: NodejsModuleCallTargetMetadataRow,
): NodejsModuleCallTargetMetadata {
  return {
    exportName: row.exportName,
    signatureId: row.signatureId,
    targetMemberId: row.targetMemberId,
    targetName: row.targetName,
    providerParameters: row.providerParameters,
    providerReturnType: row.providerReturnType,
    ...(row.providerTypeParameters === undefined ? {} : { providerTypeParameters: row.providerTypeParameters }),
    ...(row.argumentAdapters === undefined ? {} : { argumentAdapters: row.argumentAdapters }),
    member: nodejsTargetMember({
      targetMemberId: row.targetMemberId,
      sourceName: row.sourceName,
      targetName: row.targetName,
      kind: "method",
      targetParameters: row.targetParameters,
      targetReturnType: row.targetReturnType,
      declaringType: row.declaringType,
      ...(row.targetTypeParameters === undefined ? {} : { typeParameters: row.targetTypeParameters }),
      static: true,
    }),
  };
}

export function nodejsModulePropertyTargetMetadata(
  row: NodejsModulePropertyTargetMetadataRow,
): NodejsModulePropertyTargetMetadata {
  return {
    exportName: row.exportName,
    targetMemberId: row.targetMemberId,
    targetName: row.targetName,
    providerType: row.providerType,
    member: nodejsTargetMember({
      targetMemberId: row.targetMemberId,
      sourceName: row.sourceName,
      targetName: row.targetName,
      kind: "property",
      targetParameters: [],
      targetReturnType: row.targetReturnType,
      declaringType: row.declaringType,
      static: true,
    }),
  };
}

export function nodejsClassCallTargetMetadata(
  row: NodejsClassCallTargetMetadataRow,
): NodejsClassCallTargetMetadata {
  return {
    exportName: row.exportName,
    memberName: row.memberName,
    memberId: row.memberId,
    signatureId: row.signatureId,
    targetMemberId: row.targetMemberId,
    targetName: row.targetName,
    memberKind: row.memberKind,
    ...(row.argumentAdapters === undefined ? {} : { argumentAdapters: row.argumentAdapters }),
    providerParameters: row.providerParameters,
    ...(row.providerReturnType !== undefined ? { providerReturnType: row.providerReturnType } : {}),
    ...(row.providerTypeParameters === undefined ? {} : { providerTypeParameters: row.providerTypeParameters }),
    ...(row.static === true ? { static: true } : {}),
    member: nodejsTargetMember({
      targetMemberId: row.targetMemberId,
      sourceName: row.sourceName,
      targetName: row.targetName,
      kind: row.memberKind,
      targetParameters: row.targetParameters,
      targetReturnType: row.targetReturnType,
      declaringType: row.declaringType,
      ...(row.targetTypeParameters === undefined ? {} : { typeParameters: row.targetTypeParameters }),
      ...(row.static === true ? { static: true } : {}),
      ...(row.csharpInvocation === undefined
        ? {}
        : { csharpInvocation: row.csharpInvocation }),
    }),
  };
}

export function nodejsClassPropertyTargetMetadata(
  row: NodejsClassPropertyTargetMetadataRow,
): NodejsClassPropertyTargetMetadata {
  return {
    exportName: row.exportName,
    memberName: row.memberName,
    memberId: row.memberId,
    ...(row.signatureId === undefined ? {} : { signatureId: row.signatureId }),
    targetMemberId: row.targetMemberId,
    targetName: row.targetName,
    memberKind: row.memberKind,
    providerType: row.providerType,
    ...(row.readonly === true ? { readonly: true } : {}),
    ...(row.optional === true ? { optional: true } : {}),
    member: nodejsTargetMember({
      targetMemberId: row.targetMemberId,
      sourceName: row.sourceName,
      targetName: row.targetName,
      kind: row.memberKind,
      targetParameters: row.targetParameters,
      targetReturnType: row.targetReturnType,
      declaringType: row.declaringType,
      ...(row.readonly === true ? { readonly: true } : {}),
    }),
  };
}

function nodejsTargetMember(row: {
  readonly targetMemberId: string;
  readonly sourceName: string;
  readonly targetName: string;
  readonly kind: CsharpTargetMember["kind"];
  readonly targetParameters: readonly TargetParameter[];
  readonly targetReturnType: TargetTypeRef;
  readonly declaringType: TargetTypeRef;
  readonly static?: true;
  readonly readonly?: true;
  readonly csharpInvocation?: CsharpTargetInvocation;
  readonly typeParameters?: CsharpTargetMember["typeParameters"];
}): CsharpTargetMember {
  return {
    id: row.targetMemberId,
    sourceName: row.sourceName,
    targetName: row.targetName,
    kind: row.kind,
    parameters: row.targetParameters,
    returnType: row.targetReturnType,
    declaringType: row.declaringType,
    ...(row.static === true ? { static: true } : {}),
    ...(row.readonly === true ? { readonly: true } : {}),
    ...(row.csharpInvocation === undefined
      ? {}
      : { csharpInvocation: row.csharpInvocation }),
    ...(row.typeParameters === undefined ? {} : { typeParameters: row.typeParameters }),
  };
}
