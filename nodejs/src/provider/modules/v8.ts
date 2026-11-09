import type { ProviderExportDeclaration } from "@tsonic/tsts";
import type { NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeOwner, nativeParameter, nativeSlot } from "../native/authoring.js";
import { nativeNamedType } from "../native/locators.js";
import { nodejsCapabilityModuleExports, providerRef } from "../declarations/exports.js";
import { int64ProviderType, numberProviderType, stringProviderType, voidProviderType } from "../model/source-types.js";

export const nodeV8ModuleSpecifier = "node:v8";
const owner = nativeOwner("Tsonic.CSharp.Node.v8");
const heapOwner = nativeOwner("Tsonic.CSharp.Node.HeapInfo");
const heapByteFields = [
  "total_heap_size", "total_heap_size_executable", "total_physical_size", "total_available_size",
  "used_heap_size", "heap_size_limit", "malloced_memory", "peak_malloced_memory",
  "total_global_handles_size", "used_global_handles_size", "external_memory", "total_allocated_bytes",
];
const nativeLong: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int64" };
const nativeDouble: NodejsNativeTypeLocator = { kind: "source-primitive", name: "float64" };

export function nodeV8CallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [{
    exportName: "setFlagsFromString", signatureId: "node:v8.setFlagsFromString(System.String)",
    providerParameters: [{ name: "flags", type: stringProviderType }], providerReturnType: voidProviderType,
    native: nativeCall({ owner, kind: "method", targetName: "setFlagsFromString", static: true,
      parameters: [nativeParameter(0, { kind: "string" })], returnType: { kind: "void" }, receiver: { kind: "none" } }),
  }, {
    exportName: "getHeapStatistics", signatureId: "node:v8.getHeapStatistics()", providerParameters: [],
    providerReturnType: providerRef(nodeV8ModuleSpecifier, "HeapInfo"),
    native: nativeCall({ owner, kind: "method", targetName: "getHeapStatistics", static: true,
      parameters: [], returnType: nativeNamedType(heapOwner.metadataName), receiver: { kind: "none" } }),
  }];
}

export function nodeV8PropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    ...heapByteFields.map(name => ({ name, type: int64ProviderType, physical: nativeLong })),
    ...["number_of_native_contexts", "number_of_detached_contexts"].map(name => ({ name, type: numberProviderType, physical: nativeDouble })),
    { name: "does_zap_garbage", type: { kind: "union" as const, types: [{ kind: "literal" as const, value: 0 }, { kind: "literal" as const, value: 1 }] }, physical: nativeDouble },
  ].map(({ name, type, physical }) => ({
    exportName: "HeapInfo", memberName: name, memberId: `Tsonic.CSharp.Node.HeapInfo.${name}`,
    memberKind: "property", providerType: type, native: nativeSlot(heapOwner, name, physical),
  }));
}

export function nodeV8Exports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeV8ModuleSpecifier, moduleCalls: nodeV8CallTargetMembers(),
    additionalExports: [{ id: "node:v8.HeapInfo", name: "HeapInfo", kind: "interface",
      members: nodeV8PropertyTargetMembers().map(entry => ({ id: entry.memberId, name: entry.memberName, kind: "property", type: entry.providerType })) }] });
}

