import type { ProviderExportDeclaration } from "@tsonic/tsts";
import { csharpQualifiedTypeRenderShape, csharpStringTargetType, csharpTargetNamedType, csharpVoidTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import { nodejsModuleCallTargetMetadata } from "../members/target-member-metadata.js";
import type { NodejsModuleCallTargetMetadata } from "../members/target-member-metadata.js";
import { nodejsCapabilityModuleExports, stringProviderType, voidProviderType } from "./declarations.js";

export const nodeV8ModuleSpecifier = "node:v8";

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
    declaringType: csharpTargetNamedType("Tsonic.CSharp.Node.v8", undefined,
      csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "v8")),
  })];
}

export function nodeV8Exports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeV8ModuleSpecifier,
    moduleCalls: nodeV8CallTargetMembers() });
}
