import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsModuleCallTargetMetadata, NodejsModulePropertyTargetMetadata } from "../model/target-members.js";
import type { NodejsUnsupportedTargetIdentity } from "../model/members.js";
import type { NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeOwner, nativeSlot } from "../native/authoring.js";
import { nativeArray } from "../native/locators.js";
import { int32ProviderType, int64ProviderType } from "../model/source-types.js";
import { nodejsDefaultModuleObjectExports } from "../declarations/defaults.js";

export const nodeOsModuleSpecifier = "node:os";
export const nodeOsHomedirExportName = "homedir";
export const nodeOsHomedirSignatureId = "node:os.homedir()";
export const nodeOsPlatformExportName = "platform";
export const nodeOsPlatformSignatureId = "node:os.platform()";
const stringProviderType: ProviderTypeExpression = { kind: "string" };
const numberProviderType: ProviderTypeExpression = { kind: "number" };
const voidProviderType: ProviderTypeExpression = { kind: "void" };
const unknownProviderType: ProviderTypeExpression = { kind: "unknown" };
const owner = nativeOwner("Tsonic.CSharp.Node.os");
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const nativeLong: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int64" };

export function nodeOsCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  const signatures: readonly (readonly [string, ProviderTypeExpression, NodejsNativeTypeLocator])[] = [
    ["arch", stringProviderType, nativeString], ["availableParallelism", int32ProviderType, nativeInt],
    ["endianness", stringProviderType, nativeString], ["freemem", int64ProviderType, nativeLong],
    ["homedir", stringProviderType, nativeString], ["hostname", stringProviderType, nativeString],
    ["loadavg", { kind: "array", elementType: numberProviderType }, nativeArray({ kind: "source-primitive", name: "float64" })],
    ["machine", stringProviderType, nativeString], ["platform", stringProviderType, nativeString],
    ["release", stringProviderType, nativeString], ["tmpdir", stringProviderType, nativeString],
    ["totalmem", int64ProviderType, nativeLong], ["type", stringProviderType, nativeString],
    ["uptime", int64ProviderType, nativeLong], ["version", stringProviderType, nativeString],
  ];
  return signatures.map(([exportName, providerReturnType, returnType]) => ({
    exportName, signatureId: `node:os.${exportName}()`, providerParameters: [], providerReturnType,
    native: nativeCall({ owner, kind: "method", targetName: exportName, static: true, parameters: [], returnType, receiver: { kind: "none" } }),
  }));
}

export function nodeOsPropertyTargetMembers(): readonly NodejsModulePropertyTargetMetadata[] {
  return ["EOL", "devNull"].map(exportName => ({
    exportName, providerType: stringProviderType, native: nativeSlot(owner, exportName, nativeString, true, "field"),
  }));
}

export function nodeOsExports(): readonly ProviderExportDeclaration[] {
  const exports: readonly ProviderExportDeclaration[] = [
    ...nodeOsCallTargetMembers().map(entry => ({ id: `node:os.${entry.exportName}`, name: entry.exportName, kind: "function" as const,
      signatures: [{ id: entry.signatureId, parameters: [], returnType: entry.providerReturnType }] })),
    ...nodeOsPropertyTargetMembers().map(entry => ({ id: `node:os.${entry.exportName}`, name: entry.exportName, kind: "value" as const, type: entry.providerType })),
    ...nodeOsUnsupportedExports.map((entry): ProviderExportDeclaration => entry.signatureId === undefined
      ? { id: `node:os.${entry.exportName}`, name: entry.exportName, kind: "value", type: entry.providerType }
      : { id: `node:os.${entry.exportName}`, name: entry.exportName, kind: "function",
          signatures: [{ id: entry.signatureId, parameters: entry.providerParameters ?? [], returnType: entry.providerType }] }),
  ];
  return [...exports, ...nodejsDefaultModuleObjectExports(nodeOsModuleSpecifier, exports)];
}

export function nodeOsUnsupportedTargetIdentities(): readonly NodejsUnsupportedTargetIdentity[] {
  return nodeOsUnsupportedExports.map(({ exportName, signatureId, targetIdentityId, displayName }) =>
    ({ exportName, ...(signatureId === undefined ? {} : { signatureId }), targetIdentityId, displayName }));
}

const nodeOsUnsupportedExports = [
  {
    exportName: "constants",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.os.constants",
    displayName: "unsupported NodeJS os.constants",
    providerType: unknownProviderType,
  },
  {
    exportName: "cpus",
    signatureId: "node:os.cpus()",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.os.cpus()",
    displayName: "unsupported NodeJS os.cpus",
    providerType: { kind: "array", elementType: unknownProviderType },
  },
  {
    exportName: "networkInterfaces",
    signatureId: "node:os.networkInterfaces()",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.os.networkInterfaces()",
    displayName: "unsupported NodeJS os.networkInterfaces",
    providerType: unknownProviderType,
  },
  {
    exportName: "userInfo",
    signatureId: "node:os.userInfo(System.Object)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.os.userInfo(System.Object)",
    displayName: "unsupported NodeJS os.userInfo",
    providerParameters: [{ name: "options", type: unknownProviderType, optional: true }],
    providerType: unknownProviderType,
  },
  {
    exportName: "getPriority",
    signatureId: "node:os.getPriority(System.Int32)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.os.getPriority(System.Int32)",
    displayName: "unsupported NodeJS os.getPriority",
    providerParameters: [{ name: "pid", type: int32ProviderType, optional: true }],
    providerType: int32ProviderType,
  },
  {
    exportName: "setPriority",
    signatureId: "node:os.setPriority(System.Int32,System.Int32)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.os.setPriority(System.Int32,System.Int32)",
    displayName: "unsupported NodeJS os.setPriority",
    providerParameters: [
      { name: "pid", type: int32ProviderType },
      { name: "priority", type: int32ProviderType },
    ],
    providerType: voidProviderType,
  },
] satisfies readonly {
  readonly exportName: string;
  readonly signatureId?: string;
  readonly targetIdentityId: string;
  readonly displayName: string;
  readonly providerParameters?: readonly ProviderParameterDeclaration[];
  readonly providerType: ProviderTypeExpression;
}[];

