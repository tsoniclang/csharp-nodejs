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
  CsharpProviderArgumentAdapter,
} from "@tsonic/target-csharp/provider";
import { nodejsClassCallTargetMetadata, nodejsClassPropertyTargetMetadata, nodejsModuleCallTargetMetadata } from "../../declarations/target-members.js";
import type { NodejsClassCallTargetMetadata, NodejsClassCallTargetMetadataRow, NodejsClassPropertyTargetMetadata, NodejsClassPropertyTargetMetadataRow, NodejsModuleCallTargetMetadata, NodejsModuleCallTargetMetadataRow } from "../../model/target-members.js";
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
export const nodeHttpIncomingHeadersExportName = "IncomingHttpHeaders";
export const nodeHttpIncomingHeaderValuesExportName = "IncomingHttpHeaderValues";
export const nodeHttpOutgoingHeadersExportName = "OutgoingHttpHeaders";
export const nodeHttpAddressInfoExportName = "AddressInfo";
export const nodeHttpServerAddressExportName = "ServerAddress";

const stringProviderType = { kind: "string" } satisfies ProviderTypeExpression;
const numberProviderType = { kind: "number" } satisfies ProviderTypeExpression;
const voidProviderType = { kind: "void" } satisfies ProviderTypeExpression;
const boolProviderType = { kind: "boolean" } satisfies ProviderTypeExpression;
const undefinedProviderType = { kind: "undefined" } satisfies ProviderTypeExpression;
const stringOrUndefinedProviderType = {
  kind: "union",
  types: [stringProviderType, undefinedProviderType],
} satisfies ProviderTypeExpression;
const stringArrayProviderType = {
  kind: "array",
  elementType: stringProviderType,
} satisfies ProviderTypeExpression;
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const integerInputAdapter: CsharpProviderArgumentAdapter = Object.freeze({
  kind: "static-method",
  id: "Tsonic.CSharp.Node.JsNumeric.RequireInteger(System.Double)",
  declaringType: csharpTargetNamedType("Tsonic.CSharp.Node.JsNumeric", undefined,
    csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "JsNumeric")),
  targetName: "RequireInteger",
  inputType: csharpSourcePrimitiveTargetType("float64"),
  resultType: intTargetType,
  nativeIntegerConversion: "checked",
});
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const voidTargetType = csharpVoidTargetType();
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const stringArrayTargetType = { kind: "array", element: stringTargetType } satisfies TargetTypeRef;
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
const incomingHeadersTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.IncomingHttpHeaders",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "IncomingHttpHeaders"),
);
const outgoingHeadersTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.OutgoingHttpHeaders",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "OutgoingHttpHeaders"),
);
const addressInfoTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.AddressInfo",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "AddressInfo"),
);
const serverAddressTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.ServerAddress",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "ServerAddress"),
);
const socketTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Socket",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "Socket"),
);
const exceptionTargetType = csharpTargetNamedType(
  "System.Exception",
  undefined,
  csharpQualifiedTypeRenderShape("System", "Exception"),
);
const incomingMessageProviderType = providerRef(nodeHttpIncomingMessageExportName);
const serverResponseProviderType = providerRef(nodeHttpServerResponseExportName);
const serverProviderType = providerRef(nodeHttpServerExportName);
const clientRequestProviderType = providerRef(nodeHttpClientRequestExportName);
const requestOptionsProviderType = providerRef(nodeHttpRequestOptionsExportName);
const incomingHeadersProviderType = providerRef(nodeHttpIncomingHeadersExportName);
const incomingHeaderValuesProviderType = providerRef(nodeHttpIncomingHeaderValuesExportName);
const outgoingHeadersProviderType = providerRef(nodeHttpOutgoingHeadersExportName);
const addressInfoProviderType = providerRef(nodeHttpAddressInfoExportName);
const serverAddressProviderType = providerRef(nodeHttpServerAddressExportName);
const socketProviderType = {
  kind: "provider-ref",
  moduleSpecifier: "node:net",
  exportName: "Socket",
} satisfies ProviderTypeExpression;
const errorProviderType = {
  kind: "source-global",
  name: "Error",
} satisfies ProviderTypeExpression;
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
    ...httpHeaderCallTargetMembers(),
    ...incomingMessageLifecycleCallTargetMembers(),
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
      memberName: "setHeader",
      memberId: "node:http.ServerResponse.setHeader",
      signatureId: "node:http.ServerResponse.setHeader(System.String,System.String[])",
      targetMemberId: "Tsonic.CSharp.Node.Http.ServerResponse.setHeader(System.String,System.String[])",
      sourceName: "setHeader",
      targetName: "setHeader",
      memberKind: "method",
      providerParameters: [stringParameter("name"), { name: "value", type: stringArrayProviderType }],
      providerReturnType: serverResponseProviderType,
      targetParameters: [targetParameter("name", stringTargetType), targetParameter("value", stringArrayTargetType)],
      targetReturnType: serverResponseTargetType,
      declaringType: serverResponseTargetType,
    }),
    ...serverResponseHeaderCallTargetMembers(),
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
      signatureId: "node:http.Server.listen(System.Int32,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Int32,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [numberParameter("port"), { name: "callback", type: voidCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      argumentAdapters: [integerInputAdapter, undefined],
      targetParameters: [targetParameter("port", intTargetType), targetParameter("callback", voidCallbackTargetType, { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Int32,System.String,System.Int32,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Int32,System.String,System.Int32,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [
        numberParameter("port"),
        stringParameter("hostname"),
        numberParameter("backlog"),
        { name: "callback", type: voidListenHostnameCallbackProviderType, optional: true },
      ],
      providerReturnType: serverProviderType,
      argumentAdapters: [integerInputAdapter, undefined, integerInputAdapter, undefined],
      targetParameters: [
        targetParameter("port", intTargetType),
        targetParameter("hostname", stringTargetType),
        targetParameter("backlog", nullableIntTargetType),
        targetParameter("callback", voidCallbackTargetType, { optional: true }),
      ],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Int32,System.String,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Int32,System.String,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [numberParameter("port"), stringParameter("hostname"), { name: "callback", type: voidListenHostnameCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      argumentAdapters: [integerInputAdapter, undefined, undefined],
      targetParameters: [targetParameter("port", intTargetType), targetParameter("hostname", stringTargetType), targetParameter("callback", voidCallbackTargetType, { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.String,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.String,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [
        stringParameter("path"),
        { name: "callback", type: voidCallbackProviderType, optional: true },
      ],
      providerReturnType: serverProviderType,
      targetParameters: [
        targetParameter("path", stringTargetType),
        targetParameter("callback", voidCallbackTargetType, { optional: true }),
      ],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "close",
      memberId: "node:http.Server.close",
      signatureId: "node:http.Server.close(System.Action<System.Exception>)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.close(System.Action<System.Exception>)",
      sourceName: "close",
      targetName: "close",
      memberKind: "method",
      providerParameters: [{
        name: "callback",
        type: callbackProviderType("node:http.Server.close.callback", [
          { name: "error", type: errorProviderType, optional: true },
        ], voidProviderType),
        optional: true,
      }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter(
        "callback",
        csharpDelegateTargetType("System.Action", [csharpNullableTargetType(exceptionTargetType)]),
        { optional: true },
      )],
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
    ...serverLifecycleCallTargetMembers(),
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
      providerType: stringOrUndefinedProviderType,
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
      providerType: stringOrUndefinedProviderType,
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
    ...incomingMessagePropertyTargetMembers(),
    nodeHttpClassProperty({
      exportName: nodeHttpIncomingHeaderValuesExportName,
      memberName: "Item",
      memberId: `${nodeHttpModuleSpecifier}.${nodeHttpIncomingHeaderValuesExportName}.Item`,
      signatureId: `${nodeHttpModuleSpecifier}.${nodeHttpIncomingHeaderValuesExportName}.Item(System.String)`,
      targetMemberId: "Tsonic.CSharp.Node.Http.IncomingHttpHeaders.Item(System.String)",
      sourceName: "Item",
      targetName: "Item",
      memberKind: "indexer",
      providerType: { kind: "union", types: [stringArrayProviderType, undefinedProviderType] },
      targetParameters: [targetParameter("name", stringTargetType)],
      targetReturnType: csharpNullableTargetType(stringArrayTargetType),
      declaringType: incomingHeadersTargetType,
      readonly: true,
    }),
    ...serverResponsePropertyTargetMembers(),
    ...serverPropertyTargetMembers(),
    ...addressInfoPropertyTargetMembers(),
    ...serverAddressPropertyTargetMembers(),
  ];
}

function httpHeaderCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const rows: NodejsClassCallTargetMetadata[] = [];
  for (const [exportName, targetType] of [
    [nodeHttpIncomingHeadersExportName, incomingHeadersTargetType],
    [nodeHttpOutgoingHeadersExportName, outgoingHeadersTargetType],
  ] as const) {
    rows.push(
      nodeHttpClassCall({
        exportName,
        memberName: "get",
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.get`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.get(System.String)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.get(System.String)`,
        sourceName: "get",
        targetName: "get",
        memberKind: "method",
        providerParameters: [stringParameter("name")],
        providerReturnType: stringOrUndefinedProviderType,
        targetParameters: [targetParameter("name", stringTargetType)],
        targetReturnType: nullableStringTargetType,
        declaringType: targetType,
      }),
      nodeHttpClassCall({
        exportName,
        memberName: "getAll",
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.getAll`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.getAll(System.String)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.getAll(System.String)`,
        sourceName: "getAll",
        targetName: "getAll",
        memberKind: "method",
        providerParameters: [stringParameter("name")],
        providerReturnType: stringArrayProviderType,
        targetParameters: [targetParameter("name", stringTargetType)],
        targetReturnType: stringArrayTargetType,
        declaringType: targetType,
      }),
      nodeHttpClassCall({
        exportName,
        memberName: "names",
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.names`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.names()`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.names()`,
        sourceName: "names",
        targetName: "names",
        memberKind: "method",
        providerParameters: [],
        providerReturnType: stringArrayProviderType,
        targetParameters: [],
        targetReturnType: stringArrayTargetType,
        declaringType: targetType,
      }),
    );
  }
  return rows;
}

function incomingMessageLifecycleCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    nodeHttpClassCall({
      exportName: nodeHttpIncomingMessageExportName,
      memberName: "destroy",
      memberId: "node:http.IncomingMessage.destroy",
      signatureId: "node:http.IncomingMessage.destroy(Error)",
      targetMemberId: "Tsonic.CSharp.Node.Http.IncomingMessage.destroyChain(System.Exception)",
      sourceName: "destroy",
      targetName: "destroyChain",
      memberKind: "method",
      providerParameters: [{ name: "error", type: errorProviderType, optional: true }],
      providerReturnType: incomingMessageProviderType,
      targetParameters: [targetParameter("error", csharpNullableTargetType(exceptionTargetType), { optional: true })],
      targetReturnType: incomingMessageTargetType,
      declaringType: incomingMessageTargetType,
    }),
    ...typedEventRows(
      nodeHttpIncomingMessageExportName,
      incomingMessageProviderType,
      incomingMessageTargetType,
      [
        ["data", [{ name: "chunk", provider: nodeBufferProviderType, target: nodeBufferTargetType }]],
        ["error", [{ name: "error", provider: errorProviderType, target: exceptionTargetType }]],
        ["end", []],
        ["aborted", []],
        ["close", []],
      ],
    ),
  ];
}

function serverResponseHeaderCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const rows: NodejsClassCallTargetMetadata[] = [];
  const responseMethod = (
    memberName: string,
    signatureSuffix: string,
    providerParameters: readonly ProviderParameterDeclaration[],
    providerReturnType: ProviderTypeExpression,
    targetParameters: NodeHttpClassCallTargetMetadataRow["targetParameters"],
    targetReturnType: TargetTypeRef,
    targetName = memberName,
  ): void => {
    rows.push(nodeHttpClassCall({
      exportName: nodeHttpServerResponseExportName,
      memberName,
      memberId: `node:http.ServerResponse.${memberName}`,
      signatureId: `node:http.ServerResponse.${memberName}(${signatureSuffix})`,
      targetMemberId: `Tsonic.CSharp.Node.Http.ServerResponse.${targetName}(${signatureSuffix})`,
      sourceName: memberName,
      targetName,
      memberKind: "method",
      providerParameters,
      providerReturnType,
      targetParameters,
      targetReturnType,
      declaringType: serverResponseTargetType,
    }));
  };

  responseMethod("appendHeader", "System.String,System.String",
    [stringParameter("name"), stringParameter("value")],
    serverResponseProviderType,
    [targetParameter("name", stringTargetType), targetParameter("value", stringTargetType)],
    serverResponseTargetType);
  responseMethod("appendHeader", "System.String,System.String[]",
    [stringParameter("name"), { name: "value", type: stringArrayProviderType }],
    serverResponseProviderType,
    [targetParameter("name", stringTargetType), targetParameter("value", stringArrayTargetType)],
    serverResponseTargetType);
  responseMethod("getHeader", "System.String",
    [stringParameter("name")],
    stringOrUndefinedProviderType,
    [targetParameter("name", stringTargetType)],
    nullableStringTargetType);
  responseMethod("getHeaderNames", "", [], stringArrayProviderType, [], stringArrayTargetType);
  responseMethod("getHeaders", "", [], outgoingHeadersProviderType, [], outgoingHeadersTargetType);
  responseMethod("hasHeader", "System.String",
    [stringParameter("name")],
    boolProviderType,
    [targetParameter("name", stringTargetType)],
    boolTargetType);
  responseMethod("removeHeader", "System.String",
    [stringParameter("name")],
    voidProviderType,
    [targetParameter("name", stringTargetType)],
    voidTargetType);
  responseMethod("flushHeaders", "", [], voidProviderType, [], voidTargetType);
  responseMethod("writeHead", "System.Int32",
    [numberParameter("statusCode")],
    serverResponseProviderType,
    [targetParameter("statusCode", intTargetType)],
    serverResponseTargetType);
  responseMethod("writeHead", "System.Int32,OutgoingHttpHeaders",
    [numberParameter("statusCode"), { name: "headers", type: outgoingHeadersProviderType }],
    serverResponseProviderType,
    [targetParameter("statusCode", intTargetType), targetParameter("headers", outgoingHeadersTargetType)],
    serverResponseTargetType);
  responseMethod("writeHead", "System.Int32,System.String,OutgoingHttpHeaders",
    [
      numberParameter("statusCode"),
      stringParameter("statusMessage"),
      { name: "headers", type: outgoingHeadersProviderType, optional: true },
    ],
    serverResponseProviderType,
    [
      targetParameter("statusCode", intTargetType),
      targetParameter("statusMessage", stringTargetType),
      targetParameter("headers", csharpNullableTargetType(outgoingHeadersTargetType), { optional: true }),
    ],
    serverResponseTargetType);
  rows.push(...typedEventRows(
    nodeHttpServerResponseExportName,
    serverResponseProviderType,
    serverResponseTargetType,
    [
      ["error", [{ name: "error", provider: errorProviderType, target: exceptionTargetType }]],
      ["drain", []],
      ["finish", []],
      ["close", []],
    ],
  ));
  return rows;
}

function serverLifecycleCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "address",
      memberId: "node:http.Server.address",
      signatureId: "node:http.Server.address()",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.address()",
      sourceName: "address",
      targetName: "address",
      memberKind: "method",
      providerParameters: [],
      providerReturnType: { kind: "union", types: [serverAddressProviderType, undefinedProviderType] },
      targetParameters: [],
      targetReturnType: csharpNullableTargetType(serverAddressTargetType),
      declaringType: serverTargetType,
    }),
    ...typedEventRows(
      nodeHttpServerExportName,
      serverProviderType,
      serverTargetType,
      [
        ["error", [{ name: "error", provider: errorProviderType, target: exceptionTargetType }]],
        ["listening", []],
        ["close", []],
      ],
    ),
  ];
}


function typedEventRows(
  exportName: string,
  providerReturnType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  events: readonly [
    string,
    readonly { readonly name: string; readonly provider: ProviderTypeExpression; readonly target: TargetTypeRef }[],
  ][],
): readonly NodejsClassCallTargetMetadata[] {
  const rows: NodejsClassCallTargetMetadata[] = [];
  for (const methodName of ["on", "once", "off"] as const) {
    for (const [eventName, callbackParameters] of events) {
      const callbackProvider = callbackProviderType(
        `${nodeHttpModuleSpecifier}.${exportName}.${methodName}.${eventName}.listener`,
        callbackParameters.map(parameter => ({ name: parameter.name, type: parameter.provider })),
        voidProviderType,
      );
      const callbackTarget = csharpDelegateTargetType(
        "System.Action",
        callbackParameters.map(parameter => parameter.target),
      );
      rows.push(nodeHttpClassCall({
        exportName,
        memberName: methodName,
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.${methodName}`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.${methodName}(${eventName})`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.${methodName}(System.String,System.Action)`,
        sourceName: methodName,
        targetName: methodName,
        memberKind: "method",
        providerParameters: [
          { name: "event", type: { kind: "literal", value: eventName } },
          { name: "listener", type: callbackProvider },
        ],
        providerReturnType,
        targetParameters: [
          targetParameter("eventName", stringTargetType),
          targetParameter("listener", callbackTarget),
        ],
        targetReturnType,
        declaringType: targetReturnType,
      }));
    }
  }
  return rows;
}

function incomingMessagePropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    readonlyProperty(nodeHttpIncomingMessageExportName, "httpVersion", stringProviderType, stringTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "headers", incomingHeadersProviderType, incomingHeadersTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "headersDistinct", incomingHeaderValuesProviderType, incomingHeadersTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "complete", boolProviderType, boolTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "aborted", boolProviderType, boolTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "socket", socketProviderType, socketTargetType, incomingMessageTargetType),
  ];
}

function serverResponsePropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    writableProperty(nodeHttpServerResponseExportName, "statusMessage", stringProviderType, stringTargetType, serverResponseTargetType),
    readonlyProperty(nodeHttpServerResponseExportName, "headersSent", boolProviderType, boolTargetType, serverResponseTargetType),
    readonlyProperty(nodeHttpServerResponseExportName, "finished", boolProviderType, boolTargetType, serverResponseTargetType),
  ];
}

function serverPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [readonlyProperty(nodeHttpServerExportName, "listening", boolProviderType, boolTargetType, serverTargetType)];
}

function addressInfoPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    readonlyProperty(nodeHttpAddressInfoExportName, "address", stringProviderType, stringTargetType, addressInfoTargetType),
    readonlyProperty(nodeHttpAddressInfoExportName, "family", stringProviderType, stringTargetType, addressInfoTargetType),
    readonlyProperty(nodeHttpAddressInfoExportName, "port", numberProviderType, intTargetType, addressInfoTargetType),
  ];
}

function serverAddressPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    readonlyProperty(nodeHttpServerAddressExportName, "address", { kind: "union", types: [addressInfoProviderType, undefinedProviderType] }, csharpNullableTargetType(addressInfoTargetType), serverAddressTargetType),
    readonlyProperty(nodeHttpServerAddressExportName, "path", stringOrUndefinedProviderType, nullableStringTargetType, serverAddressTargetType),
    readonlyProperty(nodeHttpServerAddressExportName, "port", { kind: "union", types: [numberProviderType, undefinedProviderType] }, nullableIntTargetType, serverAddressTargetType),
  ];
}

function readonlyProperty(
  exportName: string,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodeHttpClassProperty({
    exportName,
    memberName,
    memberId: `${nodeHttpModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
    readonly: true,
  });
}

function writableProperty(
  exportName: string,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodeHttpClassProperty({
    exportName,
    memberName,
    memberId: `${nodeHttpModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
  });
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
