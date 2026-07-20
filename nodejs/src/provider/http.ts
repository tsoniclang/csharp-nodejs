import type {
  ProviderExportDeclaration,
  ProviderMemberDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpNullableTargetType,
  csharpQualifiedTypeRenderShape,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpTargetNamedType,
  targetParameter,
} from "@tsonic/target-csharp";
import {
  nodejsClassCallTargetMetadata,
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassCallTargetMetadataRow,
  NodejsClassPropertyTargetMetadata,
  NodejsClassPropertyTargetMetadataRow,
  NodejsModuleCallTargetMetadata,
  NodejsModuleCallTargetMetadataRow,
} from "./members/target-member-metadata.js";
import {
  nodejsDefaultModuleObjectExports,
} from "./module-defaults.js";
import {
  promiseProviderType,
  taskTargetType,
} from "./filesystem/types.js";
import {
  nodejsProviderTargetIdentity,
} from "./target-bindings.js";

export const nodeHttpModuleSpecifier = "node:http";
export const nodeHttpIncomingMessageExportName = "IncomingMessage";
export const nodeHttpServerResponseExportName = "ServerResponse";
export const nodeHttpServerExportName = "Server";

const stringProviderType = { kind: "string" } satisfies ProviderTypeExpression;
const numberProviderType = { kind: "number" } satisfies ProviderTypeExpression;
const voidProviderType = { kind: "void" } satisfies ProviderTypeExpression;
const nullProviderType = { kind: "literal", value: null } satisfies ProviderTypeExpression;
const stringOrNullProviderType = {
  kind: "union",
  types: [stringProviderType, nullProviderType],
} satisfies ProviderTypeExpression;
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const incomingMessageTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.IncomingMessage",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "IncomingMessage"),
);
const serverResponseTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.ServerResponse",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "ServerResponse"),
);
const serverTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.Server",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "Server"),
);
const httpTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.http",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "http"),
);
const incomingMessageProviderType = providerRef(nodeHttpIncomingMessageExportName);
const serverResponseProviderType = providerRef(nodeHttpServerResponseExportName);
const serverProviderType = providerRef(nodeHttpServerExportName);
const voidCallbackProviderType = callbackProviderType("node:http.listen.callback", [], voidProviderType);
const voidCallbackTargetType = csharpDelegateTargetType("System.Action", []);
const requestListenerProviderType = callbackProviderType("node:http.request-listener", [
  { name: "request", type: incomingMessageProviderType },
  { name: "response", type: serverResponseProviderType },
], voidProviderType);
const requestListenerTargetType = csharpDelegateTargetType(
  "System.Action",
  [incomingMessageTargetType, serverResponseTargetType],
);

type NodeHttpModuleCallTargetMetadataRow = Omit<NodejsModuleCallTargetMetadataRow, "declaringType">;
type NodeHttpClassCallTargetMetadataRow = NodejsClassCallTargetMetadataRow;
type NodeHttpClassPropertyTargetMetadataRow = NodejsClassPropertyTargetMetadataRow;

export function nodeHttpExports(): readonly ProviderExportDeclaration[] {
  const exports = [
    nodeHttpIncomingMessageDeclaration(),
    nodeHttpServerResponseDeclaration(),
    nodeHttpServerDeclaration(),
    ...providerModuleFunctions(nodeHttpCallTargetMembers()),
  ];
  return [
    ...exports,
    ...nodejsDefaultModuleObjectExports(nodeHttpModuleSpecifier, exports),
  ];
}

export function nodeHttpCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    nodeHttpModuleCall({
      exportName: "createServer",
      signatureId: "node:http.createServer(Function)",
      targetMemberId: "Tsonic.CSharp.Node.Http.http.createServer(System.Action`2)",
      sourceName: "createServer",
      targetName: "createServer",
      providerParameters: [{ name: "requestListener", type: requestListenerProviderType, optional: true }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter("requestListener", requestListenerTargetType, { optional: true })],
      targetReturnType: serverTargetType,
    }),
  ];
}

export function nodeHttpClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    nodeHttpClassCall({
      exportName: nodeHttpIncomingMessageExportName,
      memberName: "readAll",
      memberId: "node:http.IncomingMessage.readAll",
      signatureId: "node:http.IncomingMessage.readAll()",
      targetMemberId: "Tsonic.CSharp.Node.Http.IncomingMessage.readAll()",
      sourceName: "readAll",
      targetName: "readAll",
      memberKind: "method",
      providerParameters: [],
      providerReturnType: promiseProviderType(stringProviderType),
      targetParameters: [],
      targetReturnType: taskTargetType(stringTargetType),
      declaringType: incomingMessageTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerResponseExportName,
      memberName: "setHeader",
      memberId: "node:http.ServerResponse.setHeader",
      signatureId: "node:http.ServerResponse.setHeader(System.String,System.String)",
      targetMemberId: "Tsonic.CSharp.Node.Http.ServerResponse.setHeader(System.String,System.String)",
      sourceName: "setHeader",
      targetName: "setHeader",
      memberKind: "method",
      providerParameters: [stringParameter("name"), stringParameter("value")],
      providerReturnType: serverResponseProviderType,
      targetParameters: [targetParameter("name", stringTargetType), targetParameter("value", stringTargetType)],
      targetReturnType: serverResponseTargetType,
      declaringType: serverResponseTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerResponseExportName,
      memberName: "writeHead",
      memberId: "node:http.ServerResponse.writeHead",
      signatureId: "node:http.ServerResponse.writeHead(System.Int32,System.String)",
      targetMemberId: "Tsonic.CSharp.Node.Http.ServerResponse.writeHead(System.Int32,System.String)",
      sourceName: "writeHead",
      targetName: "writeHead",
      memberKind: "method",
      providerParameters: [numberParameter("statusCode"), optionalStringParameter("statusMessage")],
      providerReturnType: serverResponseProviderType,
      targetParameters: [targetParameter("statusCode", intTargetType), targetParameter("statusMessage", nullableStringTargetType, { optional: true })],
      targetReturnType: serverResponseTargetType,
      declaringType: serverResponseTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerResponseExportName,
      memberName: "end",
      memberId: "node:http.ServerResponse.end",
      signatureId: "node:http.ServerResponse.end()",
      targetMemberId: "Tsonic.CSharp.Node.Http.ServerResponse.end()",
      sourceName: "end",
      targetName: "end",
      memberKind: "method",
      providerParameters: [],
      providerReturnType: serverResponseProviderType,
      targetParameters: [],
      targetReturnType: serverResponseTargetType,
      declaringType: serverResponseTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerResponseExportName,
      memberName: "end",
      memberId: "node:http.ServerResponse.end",
      signatureId: "node:http.ServerResponse.end(System.String)",
      targetMemberId: "Tsonic.CSharp.Node.Http.ServerResponse.end(System.String)",
      sourceName: "end",
      targetName: "end",
      memberKind: "method",
      providerParameters: [stringParameter("chunk")],
      providerReturnType: serverResponseProviderType,
      targetParameters: [targetParameter("chunk", stringTargetType)],
      targetReturnType: serverResponseTargetType,
      declaringType: serverResponseTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Int32,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Int32,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [numberParameter("port"), { name: "callback", type: voidCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter("port", intTargetType), targetParameter("callback", voidCallbackTargetType, { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
  ];
}

export function nodeHttpClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    nodeHttpClassProperty({
      exportName: nodeHttpIncomingMessageExportName,
      memberName: "method",
      memberId: "node:http.IncomingMessage.method",
      targetMemberId: "Tsonic.CSharp.Node.Http.IncomingMessage.method",
      sourceName: "method",
      targetName: "method",
      memberKind: "property",
      providerType: stringOrNullProviderType,
      targetParameters: [],
      targetReturnType: nullableStringTargetType,
      declaringType: incomingMessageTargetType,
      readonly: true,
    }),
    nodeHttpClassProperty({
      exportName: nodeHttpIncomingMessageExportName,
      memberName: "url",
      memberId: "node:http.IncomingMessage.url",
      targetMemberId: "Tsonic.CSharp.Node.Http.IncomingMessage.url",
      sourceName: "url",
      targetName: "url",
      memberKind: "property",
      providerType: stringOrNullProviderType,
      targetParameters: [],
      targetReturnType: nullableStringTargetType,
      declaringType: incomingMessageTargetType,
      readonly: true,
    }),
    nodeHttpClassProperty({
      exportName: nodeHttpServerResponseExportName,
      memberName: "statusCode",
      memberId: "node:http.ServerResponse.statusCode",
      targetMemberId: "Tsonic.CSharp.Node.Http.ServerResponse.statusCode",
      sourceName: "statusCode",
      targetName: "statusCode",
      memberKind: "property",
      providerType: numberProviderType,
      targetParameters: [],
      targetReturnType: intTargetType,
      declaringType: serverResponseTargetType,
    }),
  ];
}

function nodeHttpIncomingMessageDeclaration(): ProviderExportDeclaration {
  return providerClassDeclaration(nodeHttpIncomingMessageExportName, [
    ...providerClassCallMembers(nodeHttpClassCallTargetMembers().filter((member) => member.exportName === nodeHttpIncomingMessageExportName)),
    ...providerClassPropertyMembers(nodeHttpClassPropertyTargetMembers().filter((member) => member.exportName === nodeHttpIncomingMessageExportName)),
  ]);
}

function nodeHttpServerResponseDeclaration(): ProviderExportDeclaration {
  return providerClassDeclaration(nodeHttpServerResponseExportName, [
    ...providerClassCallMembers(nodeHttpClassCallTargetMembers().filter((member) => member.exportName === nodeHttpServerResponseExportName)),
    ...providerClassPropertyMembers(nodeHttpClassPropertyTargetMembers().filter((member) => member.exportName === nodeHttpServerResponseExportName)),
  ]);
}

function nodeHttpServerDeclaration(): ProviderExportDeclaration {
  return providerClassDeclaration(nodeHttpServerExportName, [
    ...providerClassCallMembers(nodeHttpClassCallTargetMembers().filter((member) => member.exportName === nodeHttpServerExportName)),
  ]);
}

function providerClassDeclaration(
  exportName: string,
  members: readonly ProviderMemberDeclaration[],
): ProviderExportDeclaration {
  return {
    id: `${nodeHttpModuleSpecifier}.${exportName}`,
    name: exportName,
    kind: "class",
    targetIdentity: nodejsProviderTargetIdentity(nodeHttpModuleSpecifier, exportName),
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
    signatures: overloads.map((member) => ({
      id: member.signatureId,
      parameters: member.providerParameters,
      returnType: member.providerReturnType,
    })),
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
      signatures: overloads.map((member) => ({
        id: member.signatureId,
        parameters: member.providerParameters,
        ...(member.providerReturnType === undefined ? {} : { returnType: member.providerReturnType }),
      })),
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

function nodeHttpModuleCall(row: NodeHttpModuleCallTargetMetadataRow): NodejsModuleCallTargetMetadata {
  return nodejsModuleCallTargetMetadata({ ...row, declaringType: httpTargetType });
}

function nodeHttpClassCall(row: NodeHttpClassCallTargetMetadataRow): NodejsClassCallTargetMetadata {
  return nodejsClassCallTargetMetadata(row);
}

function nodeHttpClassProperty(row: NodeHttpClassPropertyTargetMetadataRow): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata(row);
}

function providerRef(exportName: string): ProviderTypeExpression {
  return { kind: "provider-ref", moduleSpecifier: nodeHttpModuleSpecifier, exportName };
}

function callbackProviderType(
  id: string,
  parameters: readonly ProviderParameterDeclaration[],
  returnType: ProviderTypeExpression,
): ProviderTypeExpression {
  return { kind: "function", id, parameters, returnType };
}

function stringParameter(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType };
}

function optionalStringParameter(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType, optional: true };
}

function numberParameter(name: string): ProviderParameterDeclaration {
  return { name, type: numberProviderType };
}

function groupedBy<T>(entries: readonly T[], key: (entry: T) => string): readonly [string, readonly T[]][] {
  const groups = new Map<string, T[]>();
  for (const entry of entries) {
    const entryKey = key(entry);
    groups.set(entryKey, [...groups.get(entryKey) ?? [], entry]);
  }
  return [...groups.entries()];
}
