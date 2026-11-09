import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import { int32ProviderType } from "../model/source-types.js";
import type { NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeTypeDeclarationLocator, NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";
import { nativeSlot } from "../native/authoring.js";
import { nodejsDefaultModuleObjectExports } from "../declarations/defaults.js";
import { spawnErrorDeclaration, spawnOptionsDeclaration, spawnReturnsDeclaration, spawnSyncNativeMapping } from "./child-process-native.js";

export const nodeChildProcessModuleSpecifier = "node:child_process";
export const nodeChildProcessSpawnSyncReturnsExportName = "SpawnSyncReturns";
const optionsExportName = "SpawnSyncOptionsWithBufferEncoding";
const errorExportName = "SpawnSyncError";
const stringType: ProviderTypeExpression = { kind: "string" };
const numberType: ProviderTypeExpression = { kind: "number" };
const nullType: ProviderTypeExpression = { kind: "literal", value: null };
const undefinedType: ProviderTypeExpression = { kind: "undefined" };
const providerRef = (exportName: string, moduleSpecifier = nodeChildProcessModuleSpecifier): ProviderTypeExpression =>
  ({ kind: "provider-ref", moduleSpecifier, exportName });
const union = (...types: ProviderTypeExpression[]): ProviderTypeExpression => ({ kind: "union", types });
const bufferType = providerRef("Buffer", "node:buffer");
const stringArrayType: ProviderTypeExpression = { kind: "array", elementType: stringType };
const resultType: ProviderTypeExpression = {
  kind: "provider-ref", moduleSpecifier: nodeChildProcessModuleSpecifier,
  exportName: nodeChildProcessSpawnSyncReturnsExportName, typeArguments: [bufferType],
};
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };

function parameters(options: boolean): readonly ProviderParameterDeclaration[] {
  return [
    { name: "command", type: stringType }, { name: "args", type: stringArrayType },
    ...(options ? [{ name: "options", type: providerRef(optionsExportName) }] : []),
  ];
}

function signatureId(options: boolean): string {
  return `node:child_process.spawnSync(System.String,System.String[]${options ? ",SpawnSyncOptionsWithBufferEncoding" : ""})`;
}

function property(
  exportName: string, owner: NodejsNativeTypeDeclarationLocator, name: string, type: ProviderTypeExpression,
  physicalType: NodejsNativeTypeLocator, optional = false, readonly = false,
): NodejsClassPropertyTargetMetadata {
  return Object.freeze({
    exportName, memberName: name, memberId: `node:child_process.${exportName}.${name}`,
    memberKind: "property", providerType: type, native: nativeSlot(owner, name, physicalType),
    ...(optional ? { optional: true } : {}), ...(readonly ? { readonly: true } : {}),
  });
}

export function nodeChildProcessClassPropertyTargetMembers(_includeJsSurfaceMembers = true): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    ...["stdout", "stderr"].map(name => property(nodeChildProcessSpawnSyncReturnsExportName, spawnReturnsDeclaration, name,
      union({ kind: "type-parameter", name: "T" }, nullType), nativeNullableReference({ kind: "class-parameter", index: 0 }))),
    property(nodeChildProcessSpawnSyncReturnsExportName, spawnReturnsDeclaration, "status", union(int32ProviderType, nullType), nativeNullable(nativeInt)),
    property(nodeChildProcessSpawnSyncReturnsExportName, spawnReturnsDeclaration, "pid", int32ProviderType, nativeNullable(nativeInt), true),
    property(nodeChildProcessSpawnSyncReturnsExportName, spawnReturnsDeclaration, "signal", union(providerRef("Signals", "node:process"), nullType), nativeNullableReference(nativeString)),
    property(nodeChildProcessSpawnSyncReturnsExportName, spawnReturnsDeclaration, "error", providerRef(errorExportName), nativeNullableReference(nativeNamedType(spawnErrorDeclaration.metadataName)), true),
    ...["message", "code"].map(name => property(errorExportName, spawnErrorDeclaration, name, stringType, nativeString, false, true)),
    property(optionsExportName, spawnOptionsDeclaration, "encoding", { kind: "literal", value: "buffer" }, nativeNullableReference(nativeString), true),
    property(optionsExportName, spawnOptionsDeclaration, "cwd", stringType, nativeNullableReference(nativeString), true),
    property(optionsExportName, spawnOptionsDeclaration, "env", providerRef("ProcessEnv", "node:process"), nativeNullableReference(nativeNamedType("Tsonic.CSharp.Node.ProcessEnv")), true),
    property(optionsExportName, spawnOptionsDeclaration, "maxBuffer", int32ProviderType, nativeNullable(nativeInt), true),
    ...["uid", "gid", "timeout"].map(name => property(optionsExportName, spawnOptionsDeclaration, name, int32ProviderType, nativeNullable(nativeInt), true)),
    property(optionsExportName, spawnOptionsDeclaration, "killSignal", providerRef("Signals", "node:process"), nativeNullableReference(nativeString), true),
    property(optionsExportName, spawnOptionsDeclaration, "input", union(bufferType, { kind: "source-global", name: "Uint8Array" }),
      nativeNullableReference({ kind: "class-parameter", index: 0 }), true),
    property(optionsExportName, spawnOptionsDeclaration, "stdio", {
      kind: "array", elementType: union(numberType, nullType, undefinedType,
        ...["pipe", "ignore", "inherit"].map(value => ({ kind: "literal" as const, value }))),
    }, nativeNullableReference({ kind: "class-parameter", index: 1 }), true),
  ];
}

export function nodeChildProcessExports(includeJsSurfaceMembers = true): readonly ProviderExportDeclaration[] {
  const properties = nodeChildProcessClassPropertyTargetMembers();
  const exports: readonly ProviderExportDeclaration[] = [
    ...[nodeChildProcessSpawnSyncReturnsExportName, optionsExportName, errorExportName].map(name => ({
      id: `node:child_process.${name}`, name, kind: "interface" as const,
      ...(name === nodeChildProcessSpawnSyncReturnsExportName ? { typeParameters: [{ name: "T" }] } : {}),
      members: properties.filter(member => member.exportName === name).map(member => ({
        id: member.memberId, name: member.memberName, kind: "property" as const,
        type: name === optionsExportName && member.memberName === "input" && !includeJsSurfaceMembers ? bufferType : member.providerType,
        ...(member.optional === true ? { optional: true } : {}),
        ...(member.readonly === true ? { readonly: true } : {}),
      })),
    })),
    { id: "node:child_process.spawnSync", name: "spawnSync", kind: "function",
      signatures: [false, true].map(options => ({ id: signatureId(options), parameters: parameters(options), returnType: resultType })) },
  ];
  return [...exports, ...nodejsDefaultModuleObjectExports(nodeChildProcessModuleSpecifier, exports)];
}

export function nodeChildProcessCallTargetMembers(includeJsSurfaceMembers = true): readonly NodejsModuleCallTargetMetadata[] {
  return [false, true].map(options => Object.freeze({
    exportName: "spawnSync", signatureId: signatureId(options),
    providerParameters: parameters(options), providerReturnType: resultType,
    native: spawnSyncNativeMapping(includeJsSurfaceMembers, options),
  }));
}
