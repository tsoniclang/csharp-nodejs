import type { ProviderTypeExpression } from "@tsonic/tsts";
import { csharpDelegateTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import type { TargetTypeRef } from "@tsonic/target-csharp/provider";
import { nodejsClassCallTargetMetadata, nodejsClassPropertyTargetMetadata, nodejsModuleCallTargetMetadata } from "../../declarations/target-members.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassCallTargetMetadataRow,
  NodejsClassPropertyTargetMetadata,
  NodejsClassPropertyTargetMetadataRow,
  NodejsModuleCallTargetMetadata,
  NodejsModuleCallTargetMetadataRow,
} from "../../model/target-members.js";
import {
  nodeHttpModuleSpecifier,
  voidProviderType,
  stringTargetType,
  httpTargetType,
  callbackProviderType,
} from "./types.js";

export type NodeHttpModuleCallTargetMetadataRow = Omit<NodejsModuleCallTargetMetadataRow, "declaringType">;
export type NodeHttpClassCallTargetMetadataRow = NodejsClassCallTargetMetadataRow;
export type NodeHttpClassPropertyTargetMetadataRow = NodejsClassPropertyTargetMetadataRow;

export function typedEventRows(
  exportName: string,
  providerReturnType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  events: readonly [
    string,
    readonly { readonly name: string; readonly provider: ProviderTypeExpression; readonly target: TargetTypeRef }[],
  ][],
): readonly NodejsClassCallTargetMetadata[] {
  const rows: NodejsClassCallTargetMetadata[] = [];
  for (const methodName of ["on", "once", "off"] as const) {
    for (const [eventName, callbackParameters] of events) {
      const callbackProvider = callbackProviderType(
        `${nodeHttpModuleSpecifier}.${exportName}.${methodName}.${eventName}.listener`,
        callbackParameters.map(parameter => ({ name: parameter.name, type: parameter.provider })),
        voidProviderType,
      );
      const callbackTarget = csharpDelegateTargetType(
        "System.Action",
        callbackParameters.map(parameter => parameter.target),
      );
      rows.push(nodeHttpClassCall({
        exportName,
        memberName: methodName,
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.${methodName}`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.${methodName}(${eventName})`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.${methodName}(System.String,System.Action)`,
        sourceName: methodName,
        targetName: methodName,
        memberKind: "method",
        providerParameters: [
          { name: "event", type: { kind: "literal", value: eventName } },
          { name: "listener", type: callbackProvider },
        ],
        providerReturnType,
        targetParameters: [
          targetParameter("eventName", stringTargetType),
          targetParameter("listener", callbackTarget),
        ],
        targetReturnType,
        declaringType: targetReturnType,
      }));
    }
  }
  return rows;
}

export function readonlyProperty(
  exportName: string,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodeHttpClassProperty({
    exportName,
    memberName,
    memberId: `${nodeHttpModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
    readonly: true,
  });
}

export function writableProperty(
  exportName: string,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodeHttpClassProperty({
    exportName,
    memberName,
    memberId: `${nodeHttpModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
  });
}

export function nodeHttpModuleCall(row: NodeHttpModuleCallTargetMetadataRow): NodejsModuleCallTargetMetadata {
  return nodejsModuleCallTargetMetadata({ ...row, declaringType: httpTargetType });
}

export function nodeHttpClassCall(row: NodeHttpClassCallTargetMetadataRow): NodejsClassCallTargetMetadata {
  return nodejsClassCallTargetMetadata(row);
}

export function nodeHttpClassProperty(row: NodeHttpClassPropertyTargetMetadataRow): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata(row);
}
