import type { ProviderExportDeclaration } from "@tsonic/tsts";
import type { NodejsClassPropertyTargetMetadata } from "../../model/target-members.js";
import { nativeSlot } from "../../native/authoring.js";
import { parsedPathOwner, stringProviderType } from "./types.js";
import { nodePathParsedPathBaseMemberId, nodePathParsedPathDirMemberId, nodePathParsedPathExportName,
  nodePathParsedPathExtMemberId, nodePathParsedPathNameMemberId, nodePathParsedPathRootMemberId } from "./identity.js";

const properties = [
  ["root", nodePathParsedPathRootMemberId], ["dir", nodePathParsedPathDirMemberId], ["base", nodePathParsedPathBaseMemberId],
  ["ext", nodePathParsedPathExtMemberId], ["name", nodePathParsedPathNameMemberId],
] as const;

export function nodePathClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return properties.map(([memberName, memberId]) => ({
    exportName: nodePathParsedPathExportName, memberName, memberId, memberKind: "property", providerType: stringProviderType,
    native: nativeSlot(parsedPathOwner, memberName, { kind: "string" }),
  }));
}

export function nodePathParsedPathExportDeclaration(): ProviderExportDeclaration {
  return { id: `node:path.${nodePathParsedPathExportName}`, name: nodePathParsedPathExportName, kind: "interface",
    members: properties.map(([name, id]) => ({ id, name, kind: "property", readonly: true, type: stringProviderType })) };
}

