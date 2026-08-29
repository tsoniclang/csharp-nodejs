import type {
  ProviderExportDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpNullableTargetType,
  csharpNullableValueTargetType,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpVoidTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  booleanProviderType,
  callbackProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  numberProviderType,
  providerRef,
  stringProviderType,
  unknownProviderType,
  voidProviderType,
} from "./capability-module.js";
import {
  nodejsClassCallTargetMetadata,
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";

export const nodeNetModuleSpecifier = "node:net";

const classNames = ["Socket", "Server", "ConnectOptions", "ListenOptions", "ServerOptions"] as const;
const targetTypes = {
  Socket: nodejsTargetNamedType("Tsonic.CSharp.Node", "Socket"),
  Server: nodejsTargetNamedType("Tsonic.CSharp.Node", "Server"),
  ConnectOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "ConnectOptions"),
  ListenOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "ListenOptions"),
  ServerOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "ServerOpts"),
};
const netTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "net");
const bufferProviderType = providerRef("node:buffer", "Buffer");
const bufferTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "Buffer");
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const longTargetType = csharpSourcePrimitiveTargetType("int64");
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const nullableBoolTargetType = csharpNullableValueTargetType(boolTargetType);
const voidTargetType = csharpVoidTargetType();
const actionProviderType = callbackProviderType("node:net.action", []);
const actionTargetType = csharpDelegateTargetType("System.Action", []);
const socketListenerProviderType = callbackProviderType("node:net.connection-listener", [
  { name: "socket", type: providerClass("Socket") },
]);
const socketListenerTargetType = csharpDelegateTargetType("System.Action", [targetTypes.Socket]);
const errorCallbackProviderType = callbackProviderType("node:net.error-callback", [
  { name: "error", type: unknownProviderType },
]);
const errorCallbackTargetType = csharpDelegateTargetType("System.Action", [
  csharpNullableTargetType(nodejsTargetNamedType("System", "Exception")),
]);

export function nodeNetExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeNetModuleSpecifier,
    moduleCalls: nodeNetCallTargetMembers(),
    classCalls: nodeNetClassCallTargetMembers(),
    classProperties: nodeNetClassPropertyTargetMembers(),
    classes: classNames,
    classHeritage: {
      Socket: [providerRef("node:stream", "Duplex")],
      Server: [providerRef("node:events", "EventEmitter")],
    },
  });
}

export function nodeNetCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return Object.freeze([
    moduleCall("createConnection", [numberParameter("port"), optionalString("host"), optionalAction("connectionListener")], providerClass("Socket"), [
      targetParameter("port", intTargetType),
      targetParameter("host", nullableStringTargetType, { optional: true }),
      targetParameter("connectionListener", actionTargetType, { optional: true }),
    ], targetTypes.Socket),
    moduleCall("createConnection", [{ name: "options", type: providerClass("ConnectOptions") }, optionalAction("connectionListener")], providerClass("Socket"), [
      targetParameter("options", targetTypes.ConnectOptions),
      targetParameter("connectionListener", actionTargetType, { optional: true }),
    ], targetTypes.Socket),
    moduleCall("createServer", [optionalProvider("connectionListener", socketListenerProviderType)], providerClass("Server"), [
      targetParameter("connectionListener", socketListenerTargetType, { optional: true }),
    ], targetTypes.Server),
    moduleCall("createServer", [{ name: "options", type: providerClass("ServerOptions") }, optionalProvider("connectionListener", socketListenerProviderType)], providerClass("Server"), [
      targetParameter("options", targetTypes.ServerOptions),
      targetParameter("connectionListener", socketListenerTargetType, { optional: true }),
    ], targetTypes.Server),
    moduleCall("isIP", [stringParameter("input")], numberProviderType, [targetParameter("input", stringTargetType)], intTargetType),
    moduleCall("isIPv4", [stringParameter("input")], booleanProviderType, [targetParameter("input", stringTargetType)], boolTargetType),
    moduleCall("isIPv6", [stringParameter("input")], booleanProviderType, [targetParameter("input", stringTargetType)], boolTargetType),
  ]);
}

export function nodeNetClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const calls: NodejsClassCallTargetMetadata[] = [
    constructor("Socket", [], []),
    constructor("Server", [], []),
    classCall("Socket", "connect", [numberParameter("port"), optionalString("host"), optionalAction("connectionListener")], providerClass("Socket"), [
      targetParameter("port", intTargetType),
      targetParameter("host", nullableStringTargetType, { optional: true }),
      targetParameter("connectionListener", actionTargetType, { optional: true }),
    ], targetTypes.Socket),
    classCall("Socket", "write", [{ name: "data", type: bufferProviderType }, optionalProvider("callback", errorCallbackProviderType)], booleanProviderType, [
      targetParameter("data", bufferTargetType),
      targetParameter("callback", errorCallbackTargetType, { optional: true }),
    ], boolTargetType),
    classCall("Socket", "write", [stringParameter("data"), optionalString("encoding"), optionalProvider("callback", errorCallbackProviderType)], booleanProviderType, [
      targetParameter("data", stringTargetType),
      targetParameter("encoding", nullableStringTargetType, { optional: true }),
      targetParameter("callback", errorCallbackTargetType, { optional: true }),
    ], boolTargetType),
    classCall("Socket", "end", [], providerClass("Socket"), [], targetTypes.Socket),
    classCall("Socket", "end", [{ name: "data", type: bufferProviderType }, optionalAction("callback")], providerClass("Socket"), [
      targetParameter("data", bufferTargetType),
      targetParameter("callback", actionTargetType, { optional: true }),
    ], targetTypes.Socket),
    classCall("Socket", "pause", [], providerClass("Socket"), [], targetTypes.Socket),
    classCall("Socket", "resume", [], providerClass("Socket"), [], targetTypes.Socket),
    classCall("Socket", "destroy", [], providerClass("Socket"), [], targetTypes.Socket),
    classCall("Server", "listen", [numberParameter("port"), optionalString("hostname"), optionalAction("callback")], providerClass("Server"), [
      targetParameter("port", intTargetType),
      targetParameter("hostname", stringTargetType, { optional: true }),
      targetParameter("callback", actionTargetType, { optional: true }),
    ], targetTypes.Server),
    classCall("Server", "listen", [{ name: "options", type: providerClass("ListenOptions") }, optionalAction("callback")], providerClass("Server"), [
      targetParameter("options", targetTypes.ListenOptions),
      targetParameter("callback", actionTargetType, { optional: true }),
    ], targetTypes.Server),
    classCall("Server", "close", [optionalProvider("callback", errorCallbackProviderType)], providerClass("Server"), [
      targetParameter("callback", errorCallbackTargetType, { optional: true }),
    ], targetTypes.Server),
    classCall("Server", "ref", [], providerClass("Server"), [], targetTypes.Server),
    classCall("Server", "unref", [], providerClass("Server"), [], targetTypes.Server),
  ];
  return Object.freeze(calls);
}

export function nodeNetClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return Object.freeze([
    optionProperty("ConnectOptions", "port", numberProviderType, intTargetType, false),
    optionProperty("ConnectOptions", "host", stringProviderType, nullableStringTargetType),
    optionProperty("ListenOptions", "port", numberProviderType, nullableIntTargetType),
    optionProperty("ListenOptions", "host", stringProviderType, nullableStringTargetType),
    optionProperty("ListenOptions", "backlog", numberProviderType, nullableIntTargetType),
    optionProperty("ServerOptions", "allowHalfOpen", booleanProviderType, nullableBoolTargetType),
    optionProperty("ServerOptions", "pauseOnConnect", booleanProviderType, nullableBoolTargetType),
    readonlyProperty("Socket", "connecting", booleanProviderType, boolTargetType),
    readonlyProperty("Socket", "destroyed", booleanProviderType, boolTargetType),
    readonlyProperty("Socket", "bytesRead", numberProviderType, longTargetType),
    readonlyProperty("Socket", "bytesWritten", numberProviderType, longTargetType),
    readonlyProperty("Socket", "remoteAddress", stringProviderType, nullableStringTargetType),
    readonlyProperty("Socket", "remotePort", numberProviderType, nullableIntTargetType),
    readonlyProperty("Server", "listening", booleanProviderType, boolTargetType),
  ]);
}

function moduleCall(
  exportName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsModuleCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
): NodejsModuleCallTargetMetadata {
  const shape = signatureShape(providerParameters);
  return nodejsModuleCallTargetMetadata({
    exportName,
    signatureId: `${nodeNetModuleSpecifier}.${exportName}(${shape})`,
    targetMemberId: `Tsonic.CSharp.Node.net.${exportName}(${shape})`,
    sourceName: exportName,
    targetName: exportName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: netTargetType,
  });
}

function constructor(
  exportName: "Socket" | "Server",
  providerParameters: readonly ProviderParameterDeclaration[],
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
): NodejsClassCallTargetMetadata {
  return classCall(exportName, "constructor", providerParameters, undefined, targetParameters, targetTypes[exportName], exportName, "constructor");
}

function classCall(
  exportName: "Socket" | "Server",
  memberName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression | undefined,
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
  targetName = memberName,
  memberKind: "constructor" | "method" = "method",
): NodejsClassCallTargetMetadata {
  const shape = signatureShape(providerParameters);
  return nodejsClassCallTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeNetModuleSpecifier}.${exportName}.${memberName}`,
    signatureId: `${nodeNetModuleSpecifier}.${exportName}.${memberName}(${shape})`,
    targetMemberId: `Tsonic.CSharp.Node.${exportName}.${targetName}(${shape})`,
    sourceName: memberName,
    targetName,
    memberKind,
    providerParameters,
    ...(providerReturnType === undefined ? {} : { providerReturnType }),
    targetParameters,
    targetReturnType,
    declaringType: targetTypes[exportName],
  });
}

function optionProperty(
  exportName: "ConnectOptions" | "ListenOptions" | "ServerOptions",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  optional = true,
): NodejsClassPropertyTargetMetadata {
  return {
    ...property(exportName, memberName, providerType, targetReturnType, targetTypes[exportName]),
    ...(optional ? { optional: true } : {}),
  };
}

function readonlyProperty(
  exportName: "Socket" | "Server",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return {
    ...property(exportName, memberName, providerType, targetReturnType, targetTypes[exportName]),
    readonly: true,
  };
}

function property(
  exportName: string,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeNetModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `${declaringType.kind === "target-named" ? declaringType.id : exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
  });
}

function providerClass(exportName: (typeof classNames)[number]): ProviderTypeExpression {
  return providerRef(nodeNetModuleSpecifier, exportName);
}

function stringParameter(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType };
}

function numberParameter(name: string): ProviderParameterDeclaration {
  return { name, type: numberProviderType };
}

function optionalString(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType, optional: true };
}

function optionalAction(name: string): ProviderParameterDeclaration {
  return { name, type: actionProviderType, optional: true };
}

function optionalProvider(name: string, type: ProviderTypeExpression): ProviderParameterDeclaration {
  return { name, type, optional: true };
}

function signatureShape(parameters: readonly ProviderParameterDeclaration[]): string {
  return parameters.map((parameter) => parameter.name).join(",");
}
