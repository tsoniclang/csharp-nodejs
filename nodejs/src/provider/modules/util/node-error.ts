import type { ProviderExportDeclaration } from "@tsonic/tsts";
import { csharpExceptionTargetType, csharpStringTargetType } from "@tsonic/target-csharp/provider";
import { nodejsClassPropertyTargetMetadata } from "../../declarations/target-members.js";
import { providerRef, unionProviderType } from "../../declarations/exports.js";
import { stringProviderType, undefinedProviderType } from "../../model/source-types.js";

export const nodeErrorProviderType = providerRef("node:util", "NodeError");
export const optionalNodeErrorProviderType = unionProviderType(nodeErrorProviderType, undefinedProviderType);
export const nodeErrorTargetType = csharpExceptionTargetType();

export function nodeErrorExportDeclaration(): ProviderExportDeclaration {
  return {
    id: "node:util.NodeError",
    name: "NodeError",
    kind: "interface",
    members: [{
      id: "node:util.NodeError.message",
      name: "message",
      kind: "property",
      readonly: true,
      type: stringProviderType,
    }],
  };
}

export function nodeErrorClassPropertyTargetMembers() {
  return [nodejsClassPropertyTargetMetadata({
    exportName: "NodeError",
    memberName: "message",
    memberId: "node:util.NodeError.message",
    targetMemberId: "System.Exception.Message",
    sourceName: "message",
    targetName: "Message",
    memberKind: "property",
    providerType: stringProviderType,
    targetParameters: [],
    targetReturnType: csharpStringTargetType(),
    declaringType: nodeErrorTargetType,
    readonly: true,
  })];
}
