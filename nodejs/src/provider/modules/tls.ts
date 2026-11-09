import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeParameterMapping } from "../native/authoring.js";
import type { NodejsNativeTypeDeclarationLocator, NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeOwner, nativeParameter, nativeSlot } from "../native/authoring.js";
import { nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";
import { callbackProviderType, nodejsCapabilityModuleExports, providerRef } from "../declarations/exports.js";
import { booleanProviderType, int32ProviderType, stringProviderType, unknownProviderType } from "../model/source-types.js";

export const nodeTlsModuleSpecifier = "node:tls";
const optionClassNames = ["ConnectionOptions", "TlsOptions", "SecureContextOptions"] as const;
const owners = {
  TLSSocket: nativeOwner("Tsonic.CSharp.Node.TLSSocket"), Server: nativeOwner("Tsonic.CSharp.Node.TLSServer"),
  SecureContext: nativeOwner("Tsonic.CSharp.Node.SecureContext"), ConnectionOptions: nativeOwner("Tsonic.CSharp.Node.ConnectionOptions"),
  TlsOptions: nativeOwner("Tsonic.CSharp.Node.TlsOptions"), SecureContextOptions: nativeOwner("Tsonic.CSharp.Node.SecureContextOptions"),
};
const owner = nativeOwner("Tsonic.CSharp.Node.tls");
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const nativeBool: NodejsNativeTypeLocator = { kind: "source-primitive", name: "bool" };
const value = nativeNamedType("Tsonic.CSharp.Runtime.TsValue");
const action = nativeNullableReference(nativeNamedType("System.Action"));
const actionProvider = callbackProviderType("node:tls.secure-connect-listener", []);
const listener = nativeNullableReference(nativeNamedType("System.Action`1", nativeNamedType(owners.TLSSocket.metadataName)));
const listenerProvider = callbackProviderType("node:tls.secure-connection-listener", [{ name: "socket", type: providerClass("TLSSocket") }]);

export function nodeTlsCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    call("connect", [{ name: "options", type: providerClass("ConnectionOptions") }, optional("secureConnectListener", actionProvider)], providerClass("TLSSocket"),
      [nativeParameter(0, nativeNamedType(owners.ConnectionOptions.metadataName)), nativeParameter(1, action)], nativeNamedType(owners.TLSSocket.metadataName)),
    call("connect", [{ name: "port", type: int32ProviderType }, optional("host", stringProviderType),
      optional("options", providerClass("ConnectionOptions")), optional("secureConnectListener", actionProvider)], providerClass("TLSSocket"),
      [nativeParameter(0, nativeInt), nativeParameter(1, nativeNullableReference(nativeString)),
        nativeParameter(2, nativeNullableReference(nativeNamedType(owners.ConnectionOptions.metadataName))), nativeParameter(3, action)], nativeNamedType(owners.TLSSocket.metadataName)),
    call("createServer", [optional("secureConnectionListener", listenerProvider)], providerClass("Server"),
      [nativeParameter(0, listener)], nativeNamedType(owners.Server.metadataName)),
    call("createServer", [{ name: "options", type: providerClass("TlsOptions") }, optional("secureConnectionListener", listenerProvider)], providerClass("Server"),
      [nativeParameter(0, nativeNamedType(owners.TlsOptions.metadataName)), nativeParameter(1, listener)], nativeNamedType(owners.Server.metadataName)),
    call("createSecureContext", [optional("options", providerClass("SecureContextOptions"))], providerClass("SecureContext"),
      [nativeParameter(0, nativeNullableReference(nativeNamedType(owners.SecureContextOptions.metadataName)))], nativeNamedType(owners.SecureContext.metadataName)),
  ];
}

export function nodeTlsClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const properties: NodejsClassPropertyTargetMetadata[] = [
    property("TLSSocket", owners.TLSSocket, "authorized", booleanProviderType, nativeBool, false, true),
    property("TLSSocket", owners.TLSSocket, "encrypted", booleanProviderType, nativeBool, false, true),
    property("TLSSocket", owners.TLSSocket, "alpnProtocol", stringProviderType, nativeNullableReference(nativeString), false, true),
  ];
  for (const exportName of optionClassNames) {
    for (const name of ["ca", "cert", "key"]) properties.push(property(exportName, owners[exportName], name, unknownProviderType, value));
    properties.push(property(exportName, owners[exportName], "passphrase", stringProviderType, nativeNullableReference(nativeString)));
  }
  properties.push(
    property("ConnectionOptions", owners.ConnectionOptions, "host", stringProviderType, nativeNullableReference(nativeString)),
    property("ConnectionOptions", owners.ConnectionOptions, "port", int32ProviderType, nativeNullable(nativeInt)),
    property("ConnectionOptions", owners.ConnectionOptions, "servername", stringProviderType, nativeNullableReference(nativeString)),
    property("ConnectionOptions", owners.ConnectionOptions, "rejectUnauthorized", booleanProviderType, nativeNullable(nativeBool)),
    property("ConnectionOptions", owners.ConnectionOptions, "timeout", int32ProviderType, nativeNullable(nativeInt)),
    property("TlsOptions", owners.TlsOptions, "rejectUnauthorized", booleanProviderType, nativeNullable(nativeBool)),
    property("TlsOptions", owners.TlsOptions, "requestCert", booleanProviderType, nativeNullable(nativeBool)),
    property("TlsOptions", owners.TlsOptions, "handshakeTimeout", int32ProviderType, nativeNullable(nativeInt)),
    property("SecureContextOptions", owners.SecureContextOptions, "pfx", unknownProviderType, value),
    property("SecureContextOptions", owners.SecureContextOptions, "minVersion", stringProviderType, nativeNullableReference(nativeString)),
    property("SecureContextOptions", owners.SecureContextOptions, "maxVersion", stringProviderType, nativeNullableReference(nativeString)),
  );
  return Object.freeze(properties);
}

export function nodeTlsExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeTlsModuleSpecifier, moduleCalls: nodeTlsCallTargetMembers(),
    classProperties: nodeTlsClassPropertyTargetMembers(), classes: ["TLSSocket", "Server", "SecureContext", ...optionClassNames],
    classHeritage: { TLSSocket: [providerRef("node:net", "Socket")], Server: [providerRef("node:net", "Server")] } });
}
function call(
  exportName: string, providerParameters: readonly ProviderParameterDeclaration[], providerReturnType: ProviderTypeExpression,
  parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator,
): NodejsModuleCallTargetMetadata {
  return { exportName, signatureId: `node:tls.${exportName}(${providerParameters.map(parameter => parameter.name).join(",")})`,
    providerParameters, providerReturnType, native: nativeCall({ owner, kind: "method", targetName: exportName, static: true,
      parameters, returnType, receiver: { kind: "none" } }) };
}
function property(
  exportName: string, physicalOwner: NodejsNativeTypeDeclarationLocator, memberName: string, providerType: ProviderTypeExpression,
  type: NodejsNativeTypeLocator, optional = true, readonly = false,
): NodejsClassPropertyTargetMetadata {
  return { exportName, memberName, memberId: `node:tls.${exportName}.${memberName}`, memberKind: "property", providerType,
    ...optional ? { optional: true } : {}, ...readonly ? { readonly: true } : {}, native: nativeSlot(physicalOwner, memberName, type) };
}
function providerClass(exportName: keyof typeof owners): ProviderTypeExpression {
  return providerRef(nodeTlsModuleSpecifier, exportName);
}
function optional(name: string, type: ProviderTypeExpression): ProviderParameterDeclaration {
  return { name, type, optional: true };
}
