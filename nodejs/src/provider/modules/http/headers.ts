import { csharpNullableTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../../model/target-members.js";
import {
  nodeHttpModuleSpecifier,
  nodeHttpIncomingHeadersExportName,
  nodeHttpIncomingHeaderValuesExportName,
  nodeHttpOutgoingHeadersExportName,
  undefinedProviderType,
  stringOrUndefinedProviderType,
  stringArrayProviderType,
  stringTargetType,
  nullableStringTargetType,
  stringArrayTargetType,
  borrowedHeaderValuesProviderType,
  borrowedHeaderValuesTargetType,
  incomingHeadersTargetType,
  outgoingHeadersTargetType,
  stringParameter,
} from "./types.js";
import { nodeHttpClassCall, nodeHttpClassProperty } from "./members.js";

export function httpHeaderCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const rows: NodejsClassCallTargetMetadata[] = [];
  for (const [exportName, targetType] of [
    [nodeHttpIncomingHeadersExportName, incomingHeadersTargetType],
    [nodeHttpOutgoingHeadersExportName, outgoingHeadersTargetType],
  ] as const) {
    rows.push(
      nodeHttpClassCall({
        exportName,
        memberName: "get",
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.get`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.get(System.String)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.get(System.String)`,
        sourceName: "get",
        targetName: "get",
        memberKind: "method",
        providerParameters: [stringParameter("name")],
        providerReturnType: stringOrUndefinedProviderType,
        targetParameters: [targetParameter("name", stringTargetType)],
        targetReturnType: nullableStringTargetType,
        declaringType: targetType,
      }),
      nodeHttpClassCall({
        exportName,
        memberName: "getAll",
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.getAll`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.getAll(System.String)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.getAll(System.String)`,
        sourceName: "getAll",
        targetName: "getAll",
        memberKind: "method",
        providerParameters: [stringParameter("name")],
        providerReturnType: stringArrayProviderType,
        targetParameters: [targetParameter("name", stringTargetType)],
        targetReturnType: stringArrayTargetType,
        declaringType: targetType,
      }),
      nodeHttpClassCall({
        exportName,
        memberName: "names",
        memberId: `${nodeHttpModuleSpecifier}.${exportName}.names`,
        signatureId: `${nodeHttpModuleSpecifier}.${exportName}.names()`,
        targetMemberId: `Tsonic.CSharp.Node.Http.${exportName}.names()`,
        sourceName: "names",
        targetName: "names",
        memberKind: "method",
        providerParameters: [],
        providerReturnType: stringArrayProviderType,
        targetParameters: [],
        targetReturnType: stringArrayTargetType,
        declaringType: targetType,
      }),
    );
  }
  return rows;
}

export function headerValuesPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    nodeHttpClassProperty({
      exportName: nodeHttpIncomingHeaderValuesExportName,
      memberName: "Item",
      memberId: `${nodeHttpModuleSpecifier}.${nodeHttpIncomingHeaderValuesExportName}.Item`,
      signatureId: `${nodeHttpModuleSpecifier}.${nodeHttpIncomingHeaderValuesExportName}.Item(System.String)`,
      targetMemberId: "Tsonic.CSharp.Node.Http.IncomingHttpHeaders.Item(System.String)",
      sourceName: "Item",
      targetName: "Item",
      memberKind: "indexer",
      providerType: { kind: "union", types: [borrowedHeaderValuesProviderType, undefinedProviderType] },
      targetParameters: [targetParameter("name", stringTargetType)],
      targetReturnType: csharpNullableTargetType(borrowedHeaderValuesTargetType),
      declaringType: incomingHeadersTargetType,
      readonly: true,
    }),
  ];
}
