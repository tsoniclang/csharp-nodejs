import type { ProviderExportDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeOwner, nativeParameter } from "../native/authoring.js";
import { nativeNamedType, nativeNullableReference } from "../native/locators.js";
import { int32ProviderType } from "../model/source-types.js";
import { nodejsDefaultModuleObjectExports } from "../declarations/defaults.js";

export const nodeTimersModuleSpecifier = "node:timers";
export const nodeTimersTimeoutExportName = "Timeout";
const voidProviderType: ProviderTypeExpression = { kind: "void" };
const timeoutProviderType: ProviderTypeExpression = { kind: "provider-ref", moduleSpecifier: nodeTimersModuleSpecifier, exportName: nodeTimersTimeoutExportName };
const callbackProviderType: ProviderTypeExpression = { kind: "function", id: "node:timers.callback", parameters: [], returnType: voidProviderType };
const owner = nativeOwner("Tsonic.CSharp.Node.timers");
const timeout = nativeNamedType("Tsonic.CSharp.Node.Timeout");
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };

export function nodeTimersCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    ...(["setTimeout", "setInterval"] as const).map(exportName => ({
      exportName, signatureId: `node:timers.${exportName}(Function,System.Int32)`,
      providerParameters: [{ name: "callback", type: callbackProviderType }, { name: "delay", type: int32ProviderType, optional: true }],
      providerReturnType: timeoutProviderType,
      native: nativeCall({ owner, kind: "method", targetName: exportName, static: true,
        parameters: [nativeParameter(0, nativeNamedType("System.Action")), nativeParameter(1, nativeInt)],
        returnType: timeout, receiver: { kind: "none" } }),
    })),
    ...(["clearTimeout", "clearInterval"] as const).map(exportName => ({
      exportName, signatureId: `node:timers.${exportName}(Timeout)`,
      providerParameters: [{ name: "timeout", type: timeoutProviderType }], providerReturnType: voidProviderType,
      native: nativeCall({ owner, kind: "method", targetName: exportName, static: true,
        parameters: [nativeParameter(0, nativeNullableReference(timeout))], returnType: { kind: "void" }, receiver: { kind: "none" } }),
    })),
  ];
}

export function nodeTimersExports(): readonly ProviderExportDeclaration[] {
  const exports: readonly ProviderExportDeclaration[] = [
    { id: "node:timers.Timeout", name: nodeTimersTimeoutExportName, kind: "class", members: [] },
    ...nodeTimersCallTargetMembers().map(entry => ({ id: `node:timers.${entry.exportName}`, name: entry.exportName, kind: "function" as const,
      signatures: [{ id: entry.signatureId, parameters: entry.providerParameters, returnType: entry.providerReturnType }] })),
  ];
  return [...exports, ...nodejsDefaultModuleObjectExports(nodeTimersModuleSpecifier, exports)];
}

