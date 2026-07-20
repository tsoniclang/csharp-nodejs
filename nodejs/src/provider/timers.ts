import type {
  ProviderExportDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpQualifiedTypeRenderShape,
  csharpSourcePrimitiveTargetType,
  csharpTargetNamedType,
  targetParameter,
} from "@tsonic/target-csharp";
import {
  nodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsModuleCallTargetMetadata,
  NodejsModuleCallTargetMetadataRow,
} from "./members/target-member-metadata.js";
import {
  nodejsDefaultModuleObjectExports,
} from "./module-defaults.js";
import {
  nodejsProviderTargetIdentity,
} from "./target-bindings.js";

export const nodeTimersModuleSpecifier = "node:timers";
export const nodeTimersTimeoutExportName = "Timeout";

const numberProviderType = { kind: "number" } satisfies ProviderTypeExpression;
const voidProviderType = { kind: "void" } satisfies ProviderTypeExpression;
const timeoutProviderType = {
  kind: "provider-ref",
  moduleSpecifier: nodeTimersModuleSpecifier,
  exportName: nodeTimersTimeoutExportName,
} satisfies ProviderTypeExpression;
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const callbackTargetType = csharpDelegateTargetType("System.Action", []);
const timeoutTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.Timeout",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "Timeout"),
);
const timersTargetType = csharpTargetNamedType(
  "Tsonic.CSharp.Node.timers",
  undefined,
  csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "timers"),
);
const callbackProviderType = {
  kind: "function",
  id: "node:timers.callback",
  parameters: [],
  returnType: voidProviderType,
} satisfies ProviderTypeExpression;

type NodeTimersModuleCallTargetMetadataRow = Omit<NodejsModuleCallTargetMetadataRow, "declaringType">;

export function nodeTimersExports(): readonly ProviderExportDeclaration[] {
  const exports = [
    {
      id: `${nodeTimersModuleSpecifier}.${nodeTimersTimeoutExportName}`,
      name: nodeTimersTimeoutExportName,
      kind: "class" as const,
      targetIdentity: nodejsProviderTargetIdentity(nodeTimersModuleSpecifier, nodeTimersTimeoutExportName),
      members: [],
    },
    ...nodeTimersCallTargetMembers().map((member) => ({
      id: `${nodeTimersModuleSpecifier}.${member.exportName}`,
      name: member.exportName,
      kind: "function" as const,
      signatures: [{
        id: member.signatureId,
        parameters: member.providerParameters,
        returnType: member.providerReturnType,
      }],
    })),
  ];
  return [
    ...exports,
    ...nodejsDefaultModuleObjectExports(nodeTimersModuleSpecifier, exports),
  ];
}

export function nodeTimersCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    nodeTimersCall("setTimeout", "node:timers.setTimeout(Function,System.Int32)"),
    nodeTimersCall("setInterval", "node:timers.setInterval(Function,System.Int32)"),
  ];
}

function nodeTimersCall(
  exportName: "setTimeout" | "setInterval",
  signatureId: string,
): NodejsModuleCallTargetMetadata {
  return nodejsModuleCallTargetMetadata({
    exportName,
    signatureId,
    targetMemberId: `Tsonic.CSharp.Node.timers.${exportName}(System.Action,System.Int32)`,
    sourceName: exportName,
    targetName: exportName,
    providerParameters: [
      { name: "callback", type: callbackProviderType },
      { name: "delay", type: numberProviderType, optional: true },
    ],
    providerReturnType: timeoutProviderType,
    targetParameters: [
      targetParameter("callback", callbackTargetType),
      targetParameter("delay", intTargetType, { optional: true }),
    ],
    targetReturnType: timeoutTargetType,
    declaringType: timersTargetType,
  } satisfies NodeTimersModuleCallTargetMetadataRow & { readonly declaringType: typeof timersTargetType });
}
