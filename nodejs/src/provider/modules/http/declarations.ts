import type {
  ProviderExportDeclaration,
  ProviderMemberDeclaration,
} from "@tsonic/tsts";
import {
  nodejsProviderSignature,
} from "../declarations.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "../../members/target-member-metadata.js";
import {
  nodejsDefaultModuleObjectExports,
} from "../defaults.js";
import {
  nodeHttpCallTargetMembers,
  nodeHttpClassCallTargetMembers,
  nodeHttpClassPropertyTargetMembers,
  nodeHttpClientRequestExportName,
  nodeHttpIncomingMessageExportName,
  nodeHttpModuleSpecifier,
  nodeHttpRequestOptionsExportName,
  nodeHttpServerExportName,
  nodeHttpServerResponseExportName,
} from "./model.js";

export function nodeHttpExports(): readonly ProviderExportDeclaration[] {
  const exports = [
    classDeclaration(nodeHttpIncomingMessageExportName),
    serverResponseDeclaration(),
    classDeclaration(nodeHttpServerExportName),
    classDeclaration(nodeHttpClientRequestExportName),
    requestOptionsDeclaration(),
    ...providerModuleFunctions(nodeHttpCallTargetMembers()),
  ];
  return [
    ...exports,
    ...nodejsDefaultModuleObjectExports(nodeHttpModuleSpecifier, exports),
  ];
}

function classDeclaration(exportName: string): ProviderExportDeclaration {
  return providerClassDeclaration(exportName, [
    ...providerClassCallMembers(nodeHttpClassCallTargetMembers().filter((member) => member.exportName === exportName)),
    ...providerClassPropertyMembers(nodeHttpClassPropertyTargetMembers().filter((member) => member.exportName === exportName)),
  ]);
}

function serverResponseDeclaration(): ProviderExportDeclaration {
  return {
    ...classDeclaration(nodeHttpServerResponseExportName),
    heritage: [{
      kind: "extends",
      type: {
        kind: "provider-ref",
        moduleSpecifier: "node:stream",
        exportName: "Writable",
      },
    }],
  };
}

function requestOptionsDeclaration(): ProviderExportDeclaration {
  return {
    id: `${nodeHttpModuleSpecifier}.${nodeHttpRequestOptionsExportName}`,
    name: nodeHttpRequestOptionsExportName,
    kind: "interface",
    members: providerClassPropertyMembers(
      nodeHttpClassPropertyTargetMembers().filter((member) => member.exportName === nodeHttpRequestOptionsExportName),
    ),
  };
}

function providerClassDeclaration(
  exportName: string,
  members: readonly ProviderMemberDeclaration[],
): ProviderExportDeclaration {
  return {
    id: `${nodeHttpModuleSpecifier}.${exportName}`,
    name: exportName,
    kind: "class",
    members,
  };
}

function providerModuleFunctions(
  members: readonly NodejsModuleCallTargetMetadata[],
): readonly ProviderExportDeclaration[] {
  return groupedBy(members, (member) => member.exportName).map(([exportName, overloads]) => ({
    id: `${nodeHttpModuleSpecifier}.${exportName}`,
    name: exportName,
    kind: "function" as const,
    signatures: overloads.map((member) => nodejsProviderSignature(
      member.signatureId,
      member.providerParameters,
      member.providerReturnType,
    )),
  }));
}

function providerClassCallMembers(
  members: readonly NodejsClassCallTargetMetadata[],
): readonly ProviderMemberDeclaration[] {
  return groupedBy(members, (member) => member.memberId).map(([, overloads]) => {
    const first = overloads[0]!;
    return {
      id: first.memberId,
      name: first.memberName,
      kind: first.memberKind,
      signatures: overloads.map((member) => nodejsProviderSignature(
        member.signatureId,
        member.providerParameters,
        member.providerReturnType,
      )),
    };
  });
}

function providerClassPropertyMembers(
  members: readonly NodejsClassPropertyTargetMetadata[],
): readonly ProviderMemberDeclaration[] {
  return members.map((member) => ({
    id: member.memberId,
    name: member.memberName,
    kind: "property" as const,
    ...(member.readonly === true ? { readonly: true } : {}),
    type: member.providerType,
  }));
}

function groupedBy<T>(entries: readonly T[], key: (entry: T) => string): readonly [string, readonly T[]][] {
  const groups = new Map<string, T[]>();
  for (const entry of entries) {
    const entryKey = key(entry);
    groups.set(entryKey, [...groups.get(entryKey) ?? [], entry]);
  }
  return [...groups.entries()];
}
