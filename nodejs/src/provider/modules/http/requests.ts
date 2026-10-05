import type { ProviderTypeExpression } from "@tsonic/tsts";
import { csharpNullableTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import type { TargetTypeRef } from "@tsonic/target-csharp/provider";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../../model/target-members.js";
import { promiseProviderType, taskTargetType } from "../filesystem/types.js";
import { nodeBufferProviderType } from "../buffer/provider-types.js";
import { nodeBufferTargetType } from "../buffer/identities.js";
import { errorProviderType as retainedErrorProviderType } from "../../model/source-types.js";
import {
  nodeHttpIncomingMessageExportName,
  nodeHttpClientRequestExportName,
  nodeHttpRequestOptionsExportName,
  stringProviderType,
  numberProviderType,
  voidProviderType,
  boolProviderType,
  stringOrUndefinedProviderType,
  stringTargetType,
  nullableStringTargetType,
  intTargetType,
  boolTargetType,
  voidTargetType,
  nullableIntTargetType,
  incomingMessageTargetType,
  clientRequestTargetType,
  requestOptionsTargetType,
  incomingHeadersTargetType,
  socketTargetType,
  exceptionTargetType,
  incomingMessageProviderType,
  incomingHeadersProviderType,
  incomingHeaderValuesProviderType,
  socketProviderType,
  stringParameter,
} from "./types.js";
import {
  typedEventRows,
  readonlyProperty,
  nodeHttpClassCall,
  nodeHttpClassProperty,
} from "./members.js";
import type { NodeHttpClassCallTargetMetadataRow } from "./members.js";

export function incomingMessageReadCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
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
  ];
}

export function incomingMessageLifecycleCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
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
      providerParameters: [{ name: "error", type: retainedErrorProviderType, optional: true }],
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
        ["error", [{ name: "error", provider: retainedErrorProviderType, target: exceptionTargetType }]],
        ["end", []],
        ["aborted", []],
        ["close", []],
      ],
    ),
  ];
}

export function incomingMessageMethodPropertyTargetMember(): NodejsClassPropertyTargetMetadata {
  return nodeHttpClassProperty({
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
    });
}

export function incomingMessageUrlPropertyTargetMember(): NodejsClassPropertyTargetMetadata {
  return nodeHttpClassProperty({
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
    });
}

export function incomingMessagePropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    readonlyProperty(nodeHttpIncomingMessageExportName, "httpVersion", stringProviderType, stringTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "headers", incomingHeadersProviderType, incomingHeadersTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "headersDistinct", incomingHeaderValuesProviderType, incomingHeadersTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "complete", boolProviderType, boolTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "aborted", boolProviderType, boolTargetType, incomingMessageTargetType),
    readonlyProperty(nodeHttpIncomingMessageExportName, "socket", socketProviderType, socketTargetType, incomingMessageTargetType),
  ];
}

export function clientRequestCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
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

export function requestOptionsPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
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

export function clientRequestPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
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
