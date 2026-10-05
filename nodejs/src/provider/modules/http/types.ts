import type { ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
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
import type { TargetTypeRef, CsharpTargetMember, CsharpProviderArgumentAdapter } from "@tsonic/target-csharp/provider";
import { nodeErrorProviderType, nodeErrorTargetType } from "../util/node-error.js";

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

export const stringProviderType = { kind: "string" } satisfies ProviderTypeExpression;
export const numberProviderType = { kind: "number" } satisfies ProviderTypeExpression;
export const voidProviderType = { kind: "void" } satisfies ProviderTypeExpression;
export const boolProviderType = { kind: "boolean" } satisfies ProviderTypeExpression;
export const undefinedProviderType = { kind: "undefined" } satisfies ProviderTypeExpression;
export const stringOrUndefinedProviderType = {
  kind: "union",
  types: [stringProviderType, undefinedProviderType],
} satisfies ProviderTypeExpression;
export const stringArrayProviderType = {
  kind: "array",
  elementType: stringProviderType,
} satisfies ProviderTypeExpression;
export const stringTargetType = csharpStringTargetType();
export const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
export const intTargetType = csharpSourcePrimitiveTargetType("int32");
export const integerInputAdapter: CsharpProviderArgumentAdapter = Object.freeze({
  kind: "static-method",
  id: "Tsonic.CSharp.Node.JsNumeric.RequireInteger(System.Double)",
  declaringType: csharpTargetNamedType("Tsonic.CSharp.Node.JsNumeric", undefined,
    csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "JsNumeric")),
  targetName: "RequireInteger",
  inputType: csharpSourcePrimitiveTargetType("float64"),
  resultType: intTargetType,
  nativeIntegerConversion: "checked",
});
export const boolTargetType = csharpSourcePrimitiveTargetType("bool");
export const voidTargetType = csharpVoidTargetType();
export const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
export const stringArrayTargetType = { kind: "array", element: stringTargetType } satisfies TargetTypeRef;
export const borrowedHeaderValuesProviderType = {
  kind: "source-global", name: "ReadonlyArray", typeArguments: [stringProviderType],
} satisfies ProviderTypeExpression;
export const headerValuesStorageType = csharpTargetNamedType(
  "Microsoft.Extensions.Primitives.StringValues", undefined,
  csharpQualifiedTypeRenderShape("Microsoft.Extensions.Primitives", "StringValues"), {
    valueType: true,
    enumerableElementType: stringTargetType,
    readOnlyIndexableElementType: stringTargetType,
    indexableLengthMemberName: "Count",
  },
);
export const headerValuesReadMember = {
  id: "Tsonic.CSharp.Node.Http.HeaderValues.read(Microsoft.Extensions.Primitives.StringValues,System.Int32)",
  sourceName: "read",
  targetName: "read",
  kind: "method",
  static: true,
  readonly: true,
  declaringType: csharpTargetNamedType("Tsonic.CSharp.Node.Http.HeaderValues", undefined,
    csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "HeaderValues")),
  parameters: [targetParameter("values", headerValuesStorageType), targetParameter("index", intTargetType)],
  returnType: stringTargetType,
} satisfies CsharpTargetMember;
export const borrowedHeaderValuesTargetType = csharpTargetNamedType(
  headerValuesStorageType.id, undefined, headerValuesStorageType.csharpRender, {
    valueType: true,
    enumerableElementType: stringTargetType,
    readOnlyIndexableElementType: stringTargetType,
    indexableLengthMemberName: "Count",
    indexableReadMember: headerValuesReadMember,
  },
);
export const incomingMessageTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.IncomingMessage",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "IncomingMessage"),
);
export const serverResponseTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.ServerResponse",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "ServerResponse"),
);
export const serverTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.Server",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "Server"),
);
export const httpTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.http",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "http"),
);
export const clientRequestTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.ClientRequest",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "ClientRequest"),
);
export const requestOptionsTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.RequestOptions",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "RequestOptions"),
);
export const incomingHeadersTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.IncomingHttpHeaders",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "IncomingHttpHeaders"),
);
export const outgoingHeadersTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.OutgoingHttpHeaders",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "OutgoingHttpHeaders"),
);
export const addressInfoTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.AddressInfo",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "AddressInfo"),
);
export const serverAddressTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Http.ServerAddress",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node.Http", "ServerAddress"),
);
export const socketTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Socket",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "Socket"),
);
export const exceptionTargetType = nodeErrorTargetType;
export const incomingMessageProviderType = providerRef(nodeHttpIncomingMessageExportName);
export const serverResponseProviderType = providerRef(nodeHttpServerResponseExportName);
export const serverProviderType = providerRef(nodeHttpServerExportName);
export const clientRequestProviderType = providerRef(nodeHttpClientRequestExportName);
export const requestOptionsProviderType = providerRef(nodeHttpRequestOptionsExportName);
export const incomingHeadersProviderType = providerRef(nodeHttpIncomingHeadersExportName);
export const incomingHeaderValuesProviderType = providerRef(nodeHttpIncomingHeaderValuesExportName);
export const outgoingHeadersProviderType = providerRef(nodeHttpOutgoingHeadersExportName);
export const addressInfoProviderType = providerRef(nodeHttpAddressInfoExportName);
export const serverAddressProviderType = providerRef(nodeHttpServerAddressExportName);
export const socketProviderType = {
  kind: "provider-ref",
  moduleSpecifier: "node:net",
  exportName: "Socket",
} satisfies ProviderTypeExpression;
export const errorProviderType = nodeErrorProviderType;
export const responseListenerProviderType = (id: string): ProviderTypeExpression =>
  callbackProviderType(id, [
    { name: "response", type: incomingMessageProviderType },
  ], voidProviderType);
export const responseListenerTargetType = csharpDelegateTargetType(
  "System.Action",
  [incomingMessageTargetType],
);
export const voidCallbackProviderType = callbackProviderType("node:http.listen.callback", [], voidProviderType);
export const voidListenHostnameCallbackProviderType = callbackProviderType("node:http.listen-hostname.callback", [], voidProviderType);
export const voidCallbackTargetType = csharpDelegateTargetType("System.Action", []);
export const requestListenerProviderType = callbackProviderType("node:http.request-listener", [
  { name: "request", type: incomingMessageProviderType },
  { name: "response", type: serverResponseProviderType },
], voidProviderType);
export const requestListenerTargetType = csharpDelegateTargetType(
  "System.Action",
  [incomingMessageTargetType, serverResponseTargetType],
);

export function providerRef(exportName: string): ProviderTypeExpression {
  return { kind: "provider-ref", moduleSpecifier: nodeHttpModuleSpecifier, exportName };
}

export function callbackProviderType(
  id: string,
  parameters: readonly ProviderParameterDeclaration[],
  returnType: ProviderTypeExpression,
): ProviderTypeExpression {
  return { kind: "function", id, parameters, returnType };
}

export function stringParameter(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType };
}

export function optionalStringParameter(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType, optional: true };
}

export function numberParameter(name: string): ProviderParameterDeclaration {
  return { name, type: numberProviderType };
}
