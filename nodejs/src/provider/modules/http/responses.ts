import type { ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import { csharpNullableTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import type { TargetTypeRef } from "@tsonic/target-csharp/provider";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../../model/target-members.js";
import { nodeBufferProviderType } from "../buffer/provider-types.js";
import { nodeBufferTargetType } from "../buffer/identities.js";
import { errorProviderType as retainedErrorProviderType } from "../../model/source-types.js";
import {
  nodeHttpServerResponseExportName,
  stringProviderType,
  numberProviderType,
  voidProviderType,
  boolProviderType,
  stringOrUndefinedProviderType,
  stringArrayProviderType,
  stringTargetType,
  nullableStringTargetType,
  intTargetType,
  boolTargetType,
  voidTargetType,
  stringArrayTargetType,
  serverResponseTargetType,
  outgoingHeadersTargetType,
  exceptionTargetType,
  serverResponseProviderType,
  outgoingHeadersProviderType,
  stringParameter,
  numberParameter,
} from "./types.js";
import {
  typedEventRows,
  readonlyProperty,
  writableProperty,
  nodeHttpClassCall,
  nodeHttpClassProperty,
} from "./members.js";
import type { NodeHttpClassCallTargetMetadataRow } from "./members.js";

export function serverResponseCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
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
      ["error", [{ name: "error", provider: retainedErrorProviderType, target: exceptionTargetType }]],
      ["drain", []],
      ["finish", []],
      ["close", []],
    ],
  ));
  return rows;
}

export function serverResponseStatusCodePropertyTargetMember(): NodejsClassPropertyTargetMetadata {
  return nodeHttpClassProperty({
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
    });
}

export function serverResponsePropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    writableProperty(nodeHttpServerResponseExportName, "statusMessage", stringProviderType, stringTargetType, serverResponseTargetType),
    readonlyProperty(nodeHttpServerResponseExportName, "headersSent", boolProviderType, boolTargetType, serverResponseTargetType),
    readonlyProperty(nodeHttpServerResponseExportName, "finished", boolProviderType, boolTargetType, serverResponseTargetType),
  ];
}
