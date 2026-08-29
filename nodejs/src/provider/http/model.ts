import type {
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpNullableTargetType,
  csharpNullableValueTargetType,
  csharpQualifiedTypeRenderShape,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpTargetNamedType,
  csharpVoidTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  nodejsClassCallTargetMetadata,
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassCallTargetMetadataRow,
  NodejsClassPropertyTargetMetadata,
  NodejsClassPropertyTargetMetadataRow,
  NodejsModuleCallTargetMetadata,
  NodejsModuleCallTargetMetadataRow,
} from "../members/target-member-metadata.js";
import {
  promiseProviderType,
  taskTargetType,
} from "../filesystem/types.js";
import {
  nodeBufferProviderType,
} from "../buffer/provider-types.js";
import {
  nodeBufferTargetType,
} from "../buffer/identities.js";

export const nodeHttpModuleSpecifier = "node:http";
export const nodeHttpIncomingMessageExportName = "IncomingMessage";
export const nodeHttpServerResponseExportName = "ServerResponse";
export const nodeHttpServerExportName = "Server";
export const nodeHttpClientRequestExportName = "ClientRequest";
export const nodeHttpRequestOptionsExportName = "RequestOptions";

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
const doubleTargetType = csharpSourcePrimitiveTargetType("float64");
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const voidTargetType = csharpVoidTargetType();
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
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
const clientRequestTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.ClientRequest",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "ClientRequest"),
);
const requestOptionsTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.RequestOptions",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "RequestOptions"),
);
const incomingMessageProviderType = providerRef(nodeHttpIncomingMessageExportName);
const serverResponseProviderType = providerRef(nodeHttpServerResponseExportName);
const serverProviderType = providerRef(nodeHttpServerExportName);
const clientRequestProviderType = providerRef(nodeHttpClientRequestExportName);
const requestOptionsProviderType = providerRef(nodeHttpRequestOptionsExportName);
const responseListenerProviderType = (id: string): ProviderTypeExpression =>
  callbackProviderType(id, [
    { name: "response", type: incomingMessageProviderType },
  ], voidProviderType);
const responseListenerTargetType = csharpDelegateTargetType(
  "System.Action",
  [incomingMessageTargetType],
);
const voidCallbackProviderType = callbackProviderType("node:http.listen.callback", [], voidProviderType);
const voidListenHostnameCallbackProviderType = callbackProviderType("node:http.listen-hostname.callback", [], voidProviderType);
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
    ...(["request", "get"] as const).flatMap((exportName) => [
      nodeHttpModuleCall({
        exportName,
        signatureId: `node:http.${exportName}(System.String,System.Action\`1)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.http.${exportName}(System.String,System.Action\`1)`,
        sourceName: exportName,
        targetName: exportName,
        providerParameters: [stringParameter("url"), {
          name: "callback",
          type: responseListenerProviderType(`node:http.${exportName}(System.String,System.Action\`1).callback`),
          optional: true,
        }],
        providerReturnType: clientRequestProviderType,
        targetParameters: [targetParameter("url", stringTargetType), targetParameter("callback", responseListenerTargetType, { optional: true })],
        targetReturnType: clientRequestTargetType,
      }),
      nodeHttpModuleCall({
        exportName,
        signatureId: `node:http.${exportName}(RequestOptions,System.Action\`1)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.http.${exportName}(Tsonic.CSharp.Node.Http.RequestOptions,System.Action\`1)`,
        sourceName: exportName,
        targetName: exportName,
        providerParameters: [{ name: "options", type: requestOptionsProviderType }, {
          name: "callback",
          type: responseListenerProviderType(`node:http.${exportName}(RequestOptions,System.Action\`1).callback`),
          optional: true,
        }],
        providerReturnType: clientRequestProviderType,
        targetParameters: [targetParameter("options", requestOptionsTargetType), targetParameter("callback", responseListenerTargetType, { optional: true })],
        targetReturnType: clientRequestTargetType,
      }),
    ]),
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
      exportName: nodeHttpIncomingMessageExportName,
      memberName: "readAllBuffer",
      memberId: "node:http.IncomingMessage.readAllBuffer",
      signatureId: "node:http.IncomingMessage.readAllBuffer()",
      targetMemberId: "Tsonic.CSharp.Node.Http.IncomingMessage.readAllBuffer()",
      sourceName: "readAllBuffer",
      targetName: "readAllBuffer",
      memberKind: "method",
      providerParameters: [],
      providerReturnType: promiseProviderType(nodeBufferProviderType),
      targetParameters: [],
      targetReturnType: taskTargetType(nodeBufferTargetType),
      declaringType: incomingMessageTargetType,
    }),
    ...clientRequestCallTargetMembers(),
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
      exportName: nodeHttpServerResponseExportName,
      memberName: "end",
      memberId: "node:http.ServerResponse.end",
      signatureId: "node:http.ServerResponse.end(Tsonic.CSharp.Node.Buffer)",
      targetMemberId: "Tsonic.CSharp.Node.Http.ServerResponse.end(Tsonic.CSharp.Node.Buffer)",
      sourceName: "end",
      targetName: "end",
      memberKind: "method",
      providerParameters: [{ name: "chunk", type: nodeBufferProviderType }],
      providerReturnType: serverResponseProviderType,
      targetParameters: [targetParameter("chunk", nodeBufferTargetType)],
      targetReturnType: serverResponseTargetType,
      declaringType: serverResponseTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Double,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Double,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [numberParameter("port"), { name: "callback", type: voidCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter("port", doubleTargetType), targetParameter("callback", voidCallbackTargetType, { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Double,System.String,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Double,System.String,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [numberParameter("port"), stringParameter("hostname"), { name: "callback", type: voidListenHostnameCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter("port", doubleTargetType), targetParameter("hostname", stringTargetType), targetParameter("callback", voidCallbackTargetType, { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "close",
      memberId: "node:http.Server.close",
      signatureId: "node:http.Server.close(System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.close(System.Action)",
      sourceName: "close",
      targetName: "close",
      memberKind: "method",
      providerParameters: [{ name: "callback", type: voidCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter("callback", voidCallbackTargetType, { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    ...(["ref", "unref"] as const).map((memberName) => nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName,
      memberId: `node:http.Server.${memberName}`,
      signatureId: `node:http.Server.${memberName}()`,
      targetMemberId: `Tsonic.CSharp.Node.Http.Server.${memberName}()`,
      sourceName: memberName,
      targetName: memberName,
      memberKind: "method",
      providerParameters: [],
      providerReturnType: serverProviderType,
      targetParameters: [],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    })),
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
    ...requestOptionsPropertyTargetMembers(),
    ...clientRequestPropertyTargetMembers(),
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

function clientRequestCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const rows: NodejsClassCallTargetMetadata[] = [];
  const add = (row: NodeHttpClassCallTargetMetadataRow): void => {
    rows.push(nodeHttpClassCall(row));
  };
  add({
    exportName: nodeHttpClientRequestExportName,
    memberName: "write",
    memberId: "node:http.ClientRequest.write",
    signatureId: "node:http.ClientRequest.write(System.String)",
    targetMemberId: "Tsonic.CSharp.Node.Http.ClientRequest.write(System.String,System.String,System.Action)",
    sourceName: "write",
    targetName: "write",
    memberKind: "method",
    providerParameters: [stringParameter("chunk")],
    providerReturnType: { kind: "boolean" },
    targetParameters: [targetParameter("chunk", stringTargetType)],
    targetReturnType: boolTargetType,
    declaringType: clientRequestTargetType,
  });
  add({
    exportName: nodeHttpClientRequestExportName,
    memberName: "write",
    memberId: "node:http.ClientRequest.write",
    signatureId: "node:http.ClientRequest.write(Buffer)",
    targetMemberId: "Tsonic.CSharp.Node.Http.ClientRequest.write(Tsonic.CSharp.Node.Buffer,System.Action)",
    sourceName: "write",
    targetName: "write",
    memberKind: "method",
    providerParameters: [{ name: "chunk", type: nodeBufferProviderType }],
    providerReturnType: { kind: "boolean" },
    targetParameters: [targetParameter("chunk", nodeBufferTargetType)],
    targetReturnType: boolTargetType,
    declaringType: clientRequestTargetType,
  });
  add({
    exportName: nodeHttpClientRequestExportName,
    memberName: "end",
    memberId: "node:http.ClientRequest.end",
    signatureId: "node:http.ClientRequest.end()",
    targetMemberId: "Tsonic.CSharp.Node.Http.ClientRequest.end(System.String,System.String,System.Action)",
    sourceName: "end",
    targetName: "end",
    memberKind: "method",
    providerParameters: [],
    providerReturnType: promiseProviderType(voidProviderType),
    targetParameters: [],
    targetReturnType: taskTargetType(voidTargetType),
    declaringType: clientRequestTargetType,
  });
  add({
    exportName: nodeHttpClientRequestExportName,
    memberName: "end",
    memberId: "node:http.ClientRequest.end",
    signatureId: "node:http.ClientRequest.end(Buffer)",
    targetMemberId: "Tsonic.CSharp.Node.Http.ClientRequest.end(Tsonic.CSharp.Node.Buffer,System.Action)",
    sourceName: "end",
    targetName: "end",
    memberKind: "method",
    providerParameters: [{ name: "chunk", type: nodeBufferProviderType }],
    providerReturnType: promiseProviderType(voidProviderType),
    targetParameters: [targetParameter("chunk", nodeBufferTargetType)],
    targetReturnType: taskTargetType(voidTargetType),
    declaringType: clientRequestTargetType,
  });
  add({
    exportName: nodeHttpClientRequestExportName,
    memberName: "setHeader",
    memberId: "node:http.ClientRequest.setHeader",
    signatureId: "node:http.ClientRequest.setHeader(System.String,System.String)",
    targetMemberId: "Tsonic.CSharp.Node.Http.ClientRequest.setHeader(System.String,System.String)",
    sourceName: "setHeader",
    targetName: "setHeader",
    memberKind: "method",
    providerParameters: [stringParameter("name"), stringParameter("value")],
    providerReturnType: voidProviderType,
    targetParameters: [targetParameter("name", stringTargetType), targetParameter("value", stringTargetType)],
    targetReturnType: voidTargetType,
    declaringType: clientRequestTargetType,
  });
  add({
    exportName: nodeHttpClientRequestExportName,
    memberName: "removeHeader",
    memberId: "node:http.ClientRequest.removeHeader",
    signatureId: "node:http.ClientRequest.removeHeader(System.String)",
    targetMemberId: "Tsonic.CSharp.Node.Http.ClientRequest.removeHeader(System.String)",
    sourceName: "removeHeader",
    targetName: "removeHeader",
    memberKind: "method",
    providerParameters: [stringParameter("name")],
    providerReturnType: voidProviderType,
    targetParameters: [targetParameter("name", stringTargetType)],
    targetReturnType: voidTargetType,
    declaringType: clientRequestTargetType,
  });
  return rows;
}

function requestOptionsPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const rows: readonly [string, ProviderTypeExpression, TargetTypeRef, boolean][] = [
    ["hostname", stringProviderType, nullableStringTargetType, true],
    ["path", stringProviderType, nullableStringTargetType, true],
    ["method", stringProviderType, stringTargetType, false],
    ["protocol", stringProviderType, stringTargetType, false],
    ["port", numberProviderType, intTargetType, false],
    ["timeout", numberProviderType, nullableIntTargetType, true],
  ];
  return rows.map(([name, providerType, targetType, optional]) => nodeHttpClassProperty({
    exportName: nodeHttpRequestOptionsExportName,
    memberName: name,
    memberId: `node:http.RequestOptions.${name}`,
    targetMemberId: `Tsonic.CSharp.Node.Http.RequestOptions.${name}`,
    sourceName: name,
    targetName: name,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType: targetType,
    declaringType: requestOptionsTargetType,
    ...(optional === true ? { optional: true as const } : {}),
  }));
}

function clientRequestPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return ["path", "method", "host", "protocol"].map((name) => nodeHttpClassProperty({
    exportName: nodeHttpClientRequestExportName,
    memberName: name,
    memberId: `node:http.ClientRequest.${name}`,
    targetMemberId: `Tsonic.CSharp.Node.Http.ClientRequest.${name}`,
    sourceName: name,
    targetName: name,
    memberKind: "property",
    providerType: stringProviderType,
    targetParameters: [],
    targetReturnType: stringTargetType,
    declaringType: clientRequestTargetType,
    readonly: true,
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
