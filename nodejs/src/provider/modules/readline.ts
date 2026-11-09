import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeParameterMapping } from "../native/authoring.js";
import type { NodejsNativeTypeDeclarationLocator, NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeDefault, nativeOwner, nativeParameter, nativeSlot } from "../native/authoring.js";
import { nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";
import { callbackProviderType, nodejsCapabilityModuleExports, providerRef } from "../declarations/exports.js";
import { booleanProviderType, int32ProviderType, stringProviderType, voidProviderType } from "../model/source-types.js";

export const nodeReadlineModuleSpecifier = "node:readline";
const moduleOwner = nativeOwner("Tsonic.CSharp.Node.readline");
const interfaceOwner = nativeOwner("Tsonic.CSharp.Node.Interface");
const optionsOwner = nativeOwner("Tsonic.CSharp.Node.InterfaceOptions");
const interfaceType = nativeNamedType(interfaceOwner.metadataName);
const interfaceProviderType = providerRef(nodeReadlineModuleSpecifier, "Interface");
const optionsProviderType = providerRef(nodeReadlineModuleSpecifier, "ReadLineOptions");
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const nativeBool: NodejsNativeTypeLocator = { kind: "source-primitive", name: "bool" };
const nativeVoid: NodejsNativeTypeLocator = { kind: "void" };
const questionCallback = callbackProviderType("node:readline.question-callback", [{ name: "answer", type: stringProviderType }]);

export function nodeReadlineCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [{ exportName: "createInterface", signatureId: "node:readline.createInterface(InterfaceOptions)",
    providerParameters: [{ name: "options", type: optionsProviderType }], providerReturnType: interfaceProviderType,
    native: nativeCall({ owner: moduleOwner, kind: "method", targetName: "createInterface", static: true,
      parameters: [nativeParameter(0, nativeNamedType(optionsOwner.metadataName))], returnType: interfaceType, receiver: { kind: "none" } }) }];
}

export function nodeReadlineClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    method("question", [{ name: "query", type: stringProviderType }, { name: "callback", type: questionCallback }], voidProviderType,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeNamedType("System.Action`1", nativeString))], nativeVoid),
    method("write", [{ name: "data", type: stringProviderType }], voidProviderType, [nativeParameter(0, nativeString)], nativeVoid),
    method("pause", [], interfaceProviderType, [], interfaceType),
    method("resume", [], interfaceProviderType, [], interfaceType),
    method("close", [], voidProviderType, [], nativeVoid),
    method("setPrompt", [{ name: "prompt", type: stringProviderType }], voidProviderType, [nativeParameter(0, nativeString)], nativeVoid),
    method("getPrompt", [], stringProviderType, [], nativeString),
    method("prompt", [], voidProviderType, [nativeDefault(nativeBool)], nativeVoid),
  ];
}

export function nodeReadlineClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const rows: readonly (readonly [string, NodejsNativeTypeDeclarationLocator, string, ProviderTypeExpression, NodejsNativeTypeLocator, boolean, boolean])[] = [
    ["Interface", interfaceOwner, "line", stringProviderType, nativeString, false, true],
    ["Interface", interfaceOwner, "cursor", int32ProviderType, nativeInt, false, true],
    ["ReadLineOptions", optionsOwner, "input", providerRef("node:stream", "Readable"), nativeNullableReference(nativeNamedType("Tsonic.CSharp.Node.Readable")), false, false],
    ["ReadLineOptions", optionsOwner, "output", providerRef("node:stream", "Writable"), nativeNullableReference(nativeNamedType("Tsonic.CSharp.Node.Writable")), true, false],
    ["ReadLineOptions", optionsOwner, "terminal", booleanProviderType, nativeNullable(nativeBool), true, false],
    ["ReadLineOptions", optionsOwner, "prompt", stringProviderType, nativeNullableReference(nativeString), true, false],
    ["ReadLineOptions", optionsOwner, "historySize", int32ProviderType, nativeNullable(nativeInt), true, false],
    ["ReadLineOptions", optionsOwner, "removeHistoryDuplicates", booleanProviderType, nativeNullable(nativeBool), true, false],
  ];
  return rows.map(([exportName, owner, memberName, providerType, physical, optional, readonly]) => ({
    exportName, memberName, memberId: `node:readline.${exportName}.${memberName}`, memberKind: "property" as const, providerType,
    ...optional ? { optional: true as const } : {}, ...readonly ? { readonly: true as const } : {},
    native: nativeSlot(owner, memberName, physical),
  }));
}

export function nodeReadlineExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeReadlineModuleSpecifier, moduleCalls: nodeReadlineCallTargetMembers(),
    classCalls: nodeReadlineClassCallTargetMembers(), classProperties: nodeReadlineClassPropertyTargetMembers(), classes: ["Interface", "ReadLineOptions"] });
}
function method(
  memberName: string, providerParameters: readonly ProviderParameterDeclaration[], providerReturnType: ProviderTypeExpression,
  parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator,
): NodejsClassCallTargetMetadata {
  return { exportName: "Interface", memberName, memberId: `node:readline.Interface.${memberName}`,
    signatureId: `node:readline.Interface.${memberName}(${providerParameters.map(parameter => parameter.type.kind).join(",")})`,
    memberKind: "method", providerParameters, providerReturnType,
    native: nativeCall({ owner: interfaceOwner, kind: "method", targetName: memberName, static: false, parameters, returnType, receiver: { kind: "instance" } }) };
}

