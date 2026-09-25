import type {
  ProviderExportDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import type {
  CsharpTargetNamedTypeRef,
  CsharpTargetMember,
} from "@tsonic/target-csharp/provider";
import {
  csharpNullableValueTargetType,
  csharpSourcePrimitiveTargetType,
} from "@tsonic/target-csharp/provider";
import type { NodejsClassPropertyTargetMember } from "../../model/members.js";
import {
  boolProviderType,
  boolTargetType,
  intTargetType,
  numberProviderType,
} from "./types.js";
import {
  makeDirectoryOptionsTargetType,
  rmOptionsTargetType,
} from "./types.js";

export const nodeFsMakeDirectoryOptionsExportName = "MakeDirectoryOptions";
export const nodeFsRmOptionsExportName = "RmOptions";

const makeDirectoryOptionsId =
  `node:fs.${nodeFsMakeDirectoryOptionsExportName}`;
const rmOptionsId = `node:fs.${nodeFsRmOptionsExportName}`;

interface NodeFsOptionPropertyRow {
  readonly exportName: string;
  readonly exportId: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly providerType: ProviderTypeExpression;
  readonly member: CsharpTargetMember;
}

const optionPropertyRows = [
  optionProperty(
    nodeFsMakeDirectoryOptionsExportName,
    makeDirectoryOptionsId,
    makeDirectoryOptionsTargetType,
    "recursive",
    boolProviderType,
    csharpNullableValueTargetType(boolTargetType),
  ),
  optionProperty(
    nodeFsMakeDirectoryOptionsExportName,
    makeDirectoryOptionsId,
    makeDirectoryOptionsTargetType,
    "mode",
    numberProviderType,
    csharpNullableValueTargetType(intTargetType),
  ),
  optionProperty(
    nodeFsRmOptionsExportName,
    rmOptionsId,
    rmOptionsTargetType,
    "recursive",
    boolProviderType,
    csharpNullableValueTargetType(boolTargetType),
  ),
  optionProperty(
    nodeFsRmOptionsExportName,
    rmOptionsId,
    rmOptionsTargetType,
    "force",
    boolProviderType,
    csharpNullableValueTargetType(boolTargetType),
  ),
  optionProperty(
    nodeFsRmOptionsExportName,
    rmOptionsId,
    rmOptionsTargetType,
    "maxRetries",
    numberProviderType,
    csharpNullableValueTargetType(csharpSourcePrimitiveTargetType("uint32")),
  ),
  optionProperty(
    nodeFsRmOptionsExportName,
    rmOptionsId,
    rmOptionsTargetType,
    "retryDelay",
    numberProviderType,
    csharpNullableValueTargetType(intTargetType),
  ),
] satisfies readonly NodeFsOptionPropertyRow[];

export function nodeFsOptionExportDeclarations(): readonly ProviderExportDeclaration[] {
  return [
    optionExport(
      nodeFsMakeDirectoryOptionsExportName,
      makeDirectoryOptionsId,
    ),
    optionExport(nodeFsRmOptionsExportName, rmOptionsId),
  ];
}

export function nodeFsOptionClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMember[] {
  return optionPropertyRows.map((row) => ({
    exportName: row.exportName,
    memberName: row.memberName,
    memberId: row.memberId,
    member: row.member,
  }));
}

function optionExport(
  exportName: string,
  exportId: string,
): ProviderExportDeclaration {
  return {
    id: exportId,
    name: exportName,
    kind: "interface",
    members: optionPropertyRows
      .filter((row) => row.exportId === exportId)
      .map((row) => ({
        id: row.memberId,
        name: row.memberName,
        kind: "property" as const,
        optional: true,
        type: row.providerType,
      })),
  };
}

function optionProperty(
  exportName: string,
  exportId: string,
  declaringType: CsharpTargetNamedTypeRef,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetType: NonNullable<CsharpTargetMember["returnType"]>,
): NodeFsOptionPropertyRow {
  const memberId = `${exportId}.${memberName}`;
  return {
    exportName,
    exportId,
    memberName,
    memberId,
    providerType,
    member: {
      id: `${declaringType.id}.${memberName}`,
      sourceName: memberName,
      targetName: memberName,
      kind: "property",
      parameters: [],
      returnType: targetType,
      declaringType,
    },
  };
}
