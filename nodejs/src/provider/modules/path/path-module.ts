import type { ProviderExportDeclaration } from "@tsonic/tsts";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../../model/target-members.js";
import { nativeCall, nativeSlot } from "../../native/authoring.js";
import { nativePathModule, pathModuleOwner, pathModuleProviderType, stringProviderType } from "./types.js";
import { pathOperations } from "./operations.js";

export function nodePathPathModuleClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return pathOperations.map(operation => ({
    exportName: "PathModule", memberName: operation.name, memberId: `node:path.PathModule.${operation.name}`,
    signatureId: `node:path.PathModule.${operation.name}${operation.signature}`, memberKind: "method",
    providerParameters: operation.sourceParameters, providerReturnType: operation.sourceReturn,
    native: nativeCall({ owner: pathModuleOwner, kind: "method", targetName: operation.name, static: false,
      parameters: operation.parameters, returnType: operation.returnType, receiver: { kind: "instance" } }),
  }));
}

export function nodePathPathModulePropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    ...["sep", "delimiter"].map(memberName => ({ exportName: "PathModule", memberName, memberId: `node:path.PathModule.${memberName}`,
      memberKind: "property" as const, providerType: stringProviderType, readonly: true as const,
      native: nativeSlot(pathModuleOwner, memberName, { kind: "string" }) })),
    ...["posix", "win32"].map(memberName => ({ exportName: "PathModule", memberName, memberId: `node:path.PathModule.${memberName}`,
      memberKind: "property" as const, providerType: pathModuleProviderType, readonly: true as const,
      native: nativeSlot(pathModuleOwner, memberName, nativePathModule) })),
  ];
}

export function nodePathPathModuleExportDeclaration(): ProviderExportDeclaration {
  return { id: "node:path.PathModule", name: "PathModule", kind: "interface", members: [
    ...nodePathPathModulePropertyTargetMembers().map(entry => ({ id: entry.memberId, name: entry.memberName, kind: "property" as const,
      readonly: true as const, type: entry.providerType })),
    ...nodePathPathModuleClassCallTargetMembers().map(entry => ({ id: entry.memberId, name: entry.memberName, kind: "method" as const,
      signatures: [{ id: entry.signatureId, parameters: entry.providerParameters, returnType: entry.providerReturnType }] })),
  ] };
}

