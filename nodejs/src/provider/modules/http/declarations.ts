import type {
  ProviderExportDeclaration,
  ProviderMemberDeclaration,
} from "@tsonic/tsts";
import {
  nodejsProviderSignature,
} from "../../declarations/exports.js";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../../model/target-members.js";
import {
  nodejsDefaultModuleObjectExports,
} from "../../declarations/defaults.js";
import {
  nodeHttpCallTargetMembers,
  nodeHttpClassCallTargetMembers,
  nodeHttpClassPropertyTargetMembers,
  nodeHttpClientRequestExportName,
  nodeHttpAddressInfoExportName,
  nodeHttpServerAddressExportName,
  nodeHttpIncomingHeadersExportName,
  nodeHttpIncomingHeaderValuesExportName,
  nodeHttpIncomingMessageExportName,
  nodeHttpModuleSpecifier,
  nodeHttpOutgoingHeadersExportName,
  nodeHttpRequestOptionsExportName,
  nodeHttpServerExportName,
  nodeHttpServerResponseExportName,
} from "./model.js";

export function nodeHttpExports(): readonly ProviderExportDeclaration[] {
  const exports = [
    classDeclaration(nodeHttpIncomingHeadersExportName),
    headerValuesDeclaration(),
    classDeclaration(nodeHttpOutgoingHeadersExportName),
    classDeclaration(nodeHttpAddressInfoExportName),
    classDeclaration(nodeHttpServerAddressExportName),
    incomingMessageDeclaration(),
    serverResponseDeclaration(),
    serverDeclaration(),
    classDeclaration(nodeHttpClientRequestExportName),
    requestOptionsDeclaration(),
    ...providerModuleFunctions(nodeHttpCallTargetMembers()),
  ];
  return [
    ...exports,
    ...nodejsDefaultModuleObjectExports(nodeHttpModuleSpecifier, exports),
  ];
}

function incomingMessageDeclaration(): ProviderExportDeclaration {
  return {
    ...classDeclaration(nodeHttpIncomingMessageExportName),
    heritage: [{
      kind: "extends",
      type: {
        kind: "provider-ref",
        moduleSpecifier: "node:stream",
        exportName: "Readable",
      },
    }],
  };
}

function classDeclaration(exportName: string): ProviderExportDeclaration {
  return providerClassDeclaration(exportName, [
    ...providerClassCallMembers(nodeHttpClassCallTargetMembers().filter((member) => member.exportName === exportName)),
    ...providerClassPropertyMembers(nodeHttpClassPropertyTargetMembers().filter((member) => member.exportName === exportName)),
  ]);
}

function headerValuesDeclaration(): ProviderExportDeclaration {
  const id = `${nodeHttpModuleSpecifier}.${nodeHttpIncomingHeaderValuesExportName}`;
  return providerClassDeclaration(nodeHttpIncomingHeaderValuesExportName, [{
    id: `${id}.Item`,
    name: "Item",
    kind: "indexer",
    signatures: [{
      id: `${id}.Item(System.String)`,
      parameters: [{ name: "name", type: { kind: "string" } }],
      returnType: {
        kind: "union",
        types: [{ kind: "array", elementType: { kind: "string" } }, { kind: "undefined" }],
      },
    }],
  }]);
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

function serverDeclaration(): ProviderExportDeclaration {
  return classDeclaration(nodeHttpServerExportName);
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
      ...(first.static === true ? { static: true } : {}),
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
  return members.map((member) => member.memberKind === "indexer"
    ? {
      id: member.memberId,
      name: member.memberName,
      kind: "indexer" as const,
      signatures: [{
        id: member.signatureId ?? member.memberId,
        parameters: [{ name: "name", type: { kind: "string" as const } }],
        returnType: member.providerType,
      }],
    }
    : {
      id: member.memberId,
      name: member.memberName,
      kind: "property" as const,
      ...(member.readonly === true ? { readonly: true } : {}),
      type: member.providerType,
    });
}

function groupedBy<T>(entries: readonly T[], key: (entry: T) => string): readonly [string, readonly T[]][] {
  const groups = new Map<string, T[]>();
  for (const entry of entries) {
    const entryKey = key(entry);
    groups.set(entryKey, [...groups.get(entryKey) ?? [], entry]);
  }
  return [...groups.entries()];
}
