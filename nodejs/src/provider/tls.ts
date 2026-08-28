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
  csharpTsValueTargetType,
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
} from "./capability-module.js";
import {
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";

export const nodeTlsModuleSpecifier = "node:tls";

const optionClassNames = ["ConnectionOptions", "TlsOptions", "SecureContextOptions"] as const;
const targetTypes = {
  TLSSocket: nodejsTargetNamedType("Tsonic.CSharp.Node", "TLSSocket"),
  Server: nodejsTargetNamedType("Tsonic.CSharp.Node", "TLSServer"),
  SecureContext: nodejsTargetNamedType("Tsonic.CSharp.Node", "SecureContext"),
  ConnectionOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "ConnectionOptions"),
  TlsOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "TlsOptions"),
  SecureContextOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "SecureContextOptions"),
};
const tlsTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "tls");
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const nullableBoolTargetType = csharpNullableValueTargetType(boolTargetType);
const tsValueTargetType = csharpTsValueTargetType();
const actionProviderType = callbackProviderType("node:tls.secure-connect-listener", []);
const actionTargetType = csharpDelegateTargetType("System.Action", []);
const socketListenerProviderType = callbackProviderType("node:tls.secure-connection-listener", [
  { name: "socket", type: providerRef(nodeTlsModuleSpecifier, "TLSSocket") },
]);
const socketListenerTargetType = csharpDelegateTargetType("System.Action", [targetTypes.TLSSocket]);

export function nodeTlsExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeTlsModuleSpecifier,
    moduleCalls: nodeTlsCallTargetMembers(),
    classProperties: nodeTlsClassPropertyTargetMembers(),
    classes: ["TLSSocket", "Server", "SecureContext", ...optionClassNames],
    classHeritage: {
      TLSSocket: [providerRef("node:net", "Socket")],
      Server: [providerRef("node:net", "Server")],
    },
  });
}

export function nodeTlsCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return Object.freeze([
    moduleCall("connect", [{ name: "options", type: providerClass("ConnectionOptions") }, optionalAction("secureConnectListener")], providerClass("TLSSocket"), [
      targetParameter("options", targetTypes.ConnectionOptions),
      targetParameter("secureConnectListener", actionTargetType, { optional: true }),
    ], targetTypes.TLSSocket),
    moduleCall("connect", [numberParameter("port"), optionalString("host"), optionalProvider("options", providerClass("ConnectionOptions")), optionalAction("secureConnectListener")], providerClass("TLSSocket"), [
      targetParameter("port", intTargetType),
      targetParameter("host", nullableStringTargetType, { optional: true }),
      targetParameter("options", targetTypes.ConnectionOptions, { optional: true }),
      targetParameter("secureConnectListener", actionTargetType, { optional: true }),
    ], targetTypes.TLSSocket),
    moduleCall("createServer", [optionalProvider("secureConnectionListener", socketListenerProviderType)], providerClass("Server"), [
      targetParameter("secureConnectionListener", socketListenerTargetType, { optional: true }),
    ], targetTypes.Server),
    moduleCall("createServer", [{ name: "options", type: providerClass("TlsOptions") }, optionalProvider("secureConnectionListener", socketListenerProviderType)], providerClass("Server"), [
      targetParameter("options", targetTypes.TlsOptions),
      targetParameter("secureConnectionListener", socketListenerTargetType, { optional: true }),
    ], targetTypes.Server),
    moduleCall("createSecureContext", [optionalProvider("options", providerClass("SecureContextOptions"))], providerClass("SecureContext"), [
      targetParameter("options", targetTypes.SecureContextOptions, { optional: true }),
    ], targetTypes.SecureContext),
  ]);
}

export function nodeTlsClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const properties: NodejsClassPropertyTargetMetadata[] = [
    readonlyProperty("TLSSocket", "authorized", booleanProviderType, boolTargetType, targetTypes.TLSSocket),
    readonlyProperty("TLSSocket", "encrypted", booleanProviderType, boolTargetType, targetTypes.TLSSocket),
    readonlyProperty("TLSSocket", "alpnProtocol", stringProviderType, nullableStringTargetType, targetTypes.TLSSocket),
  ];
  for (const exportName of optionClassNames) {
    for (const memberName of ["ca", "cert", "key"] as const)
      properties.push(optionProperty(exportName, memberName, unknownProviderType, tsValueTargetType));
    properties.push(
      optionProperty(exportName, "passphrase", stringProviderType, nullableStringTargetType),
    );
  }
  properties.push(
    optionProperty("ConnectionOptions", "host", stringProviderType, nullableStringTargetType),
    optionProperty("ConnectionOptions", "port", numberProviderType, nullableIntTargetType),
    optionProperty("ConnectionOptions", "servername", stringProviderType, nullableStringTargetType),
    optionProperty("ConnectionOptions", "rejectUnauthorized", booleanProviderType, nullableBoolTargetType),
    optionProperty("ConnectionOptions", "timeout", numberProviderType, nullableIntTargetType),
    optionProperty("TlsOptions", "rejectUnauthorized", booleanProviderType, nullableBoolTargetType),
    optionProperty("TlsOptions", "requestCert", booleanProviderType, nullableBoolTargetType),
    optionProperty("TlsOptions", "handshakeTimeout", numberProviderType, nullableIntTargetType),
    optionProperty("SecureContextOptions", "pfx", unknownProviderType, tsValueTargetType),
    optionProperty("SecureContextOptions", "minVersion", stringProviderType, nullableStringTargetType),
    optionProperty("SecureContextOptions", "maxVersion", stringProviderType, nullableStringTargetType),
  );
  return Object.freeze(properties);
}

function moduleCall(
  exportName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsModuleCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
): NodejsModuleCallTargetMetadata {
  const shape = providerParameters.map((parameter) => parameter.name).join(",");
  return nodejsModuleCallTargetMetadata({
    exportName,
    signatureId: `${nodeTlsModuleSpecifier}.${exportName}(${shape})`,
    targetMemberId: `Tsonic.CSharp.Node.tls.${exportName}(${shape})`,
    sourceName: exportName,
    targetName: exportName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: tlsTargetType,
  });
}

function optionProperty(
  exportName: (typeof optionClassNames)[number],
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return {
    ...property(exportName, memberName, providerType, targetReturnType, targetTypes[exportName]),
    optional: true,
  };
}

function readonlyProperty(
  exportName: "TLSSocket",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return {
    ...property(exportName, memberName, providerType, targetReturnType, declaringType),
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
    memberId: `${nodeTlsModuleSpecifier}.${exportName}.${memberName}`,
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

function providerClass(exportName: keyof typeof targetTypes): ProviderTypeExpression {
  return providerRef(nodeTlsModuleSpecifier, exportName);
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
