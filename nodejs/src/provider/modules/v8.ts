import type { ProviderExportDeclaration } from "@tsonic/tsts";
import { csharpQualifiedTypeRenderShape, csharpSourcePrimitiveTargetType, csharpStringTargetType, csharpTargetNamedType, csharpVoidTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import { nodejsClassPropertyTargetMetadata, nodejsModuleCallTargetMetadata } from "../members/target-member-metadata.js";
import type { NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../members/target-member-metadata.js";
import { nodejsCapabilityModuleExports, numberProviderType, providerRef, stringProviderType, voidProviderType } from "./declarations.js";

export const nodeV8ModuleSpecifier = "node:v8";
const v8Type = csharpTargetNamedType("Tsonic.CSharp.Node.v8", undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "v8"));
const heapType = csharpTargetNamedType("Tsonic.CSharp.Node.HeapInfo", undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "HeapInfo"));
const heapFields = [
  "total_heap_size", "total_heap_size_executable", "total_physical_size",
  "total_available_size", "used_heap_size", "heap_size_limit", "malloced_memory",
  "peak_malloced_memory", "number_of_native_contexts", "number_of_detached_contexts",
  "total_global_handles_size", "used_global_handles_size", "external_memory", "total_allocated_bytes",
];

export function nodeV8CallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [nodejsModuleCallTargetMetadata({
    exportName: "setFlagsFromString",
    signatureId: "node:v8.setFlagsFromString(System.String)",
    targetMemberId: "Tsonic.CSharp.Node.v8.setFlagsFromString(System.String)",
    sourceName: "setFlagsFromString",
    targetName: "setFlagsFromString",
    providerParameters: [{ name: "flags", type: stringProviderType }],
    providerReturnType: voidProviderType,
    targetParameters: [targetParameter("flags", csharpStringTargetType())],
    targetReturnType: csharpVoidTargetType(),
    declaringType: v8Type,
  }), nodejsModuleCallTargetMetadata({
    exportName: "getHeapStatistics", signatureId: "node:v8.getHeapStatistics()",
    targetMemberId: "Tsonic.CSharp.Node.v8.getHeapStatistics()",
    sourceName: "getHeapStatistics", targetName: "getHeapStatistics",
    providerParameters: [], providerReturnType: providerRef(nodeV8ModuleSpecifier, "HeapInfo"),
    targetParameters: [], targetReturnType: heapType, declaringType: v8Type,
  })];
}

export function nodeV8PropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [...heapFields.map(name => ({ name, type: numberProviderType })), {
    name: "does_zap_garbage", type: { kind: "union" as const,
      types: [{ kind: "literal" as const, value: 0 }, { kind: "literal" as const, value: 1 }] },
  }].map(({ name, type }) => nodejsClassPropertyTargetMetadata({
    exportName: "HeapInfo", memberName: name, memberId: `Tsonic.CSharp.Node.HeapInfo.${name}`,
    targetMemberId: `Tsonic.CSharp.Node.HeapInfo.${name}`, sourceName: name, targetName: name,
    memberKind: "property", providerType: type, targetParameters: [],
    targetReturnType: csharpSourcePrimitiveTargetType("float64"), declaringType: heapType,
  }));
}

export function nodeV8Exports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeV8ModuleSpecifier,
    moduleCalls: nodeV8CallTargetMembers(), additionalExports: [{
      id: "node:v8.HeapInfo", name: "HeapInfo", kind: "interface",
      members: nodeV8PropertyTargetMembers().map(member => ({
        id: member.memberId, name: member.memberName, kind: "property", type: member.providerType,
      })),
    }] });
}
