import type {
  ProviderTypeExpression,
} from "@tsonic/tsts";
import type { TargetTypeRef } from "@tsonic/target-csharp/provider";
import type {
  NodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";
import {
  csharpVoidTargetType,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpQualifiedTypeRenderShape,
  csharpTargetNamedType,
} from "@tsonic/target-csharp/provider";
import {
  csharpTaskTargetType,
} from "@tsonic/target-csharp/provider";
import {
  csharpJsDateTargetType,
} from "@tsonic/target-csharp/provider";
import {
  nodeBufferExportName,
  nodeBufferModuleSpecifier,
  nodeBufferTargetType,
} from "../buffer/identities.js";

export const stringProviderType = { kind: "string" } satisfies ProviderTypeExpression;
export const boolProviderType = { kind: "boolean" } satisfies ProviderTypeExpression;
export const numberProviderType = { kind: "number" } satisfies ProviderTypeExpression;
export const voidProviderType = { kind: "void" } satisfies ProviderTypeExpression;
export const dateProviderType = {
  kind: "source-global",
  name: "Date",
} satisfies ProviderTypeExpression;
export const bufferProviderType = {
  kind: "provider-ref",
  moduleSpecifier: nodeBufferModuleSpecifier,
  exportName: nodeBufferExportName,
} satisfies ProviderTypeExpression;
export const stringTargetType = csharpStringTargetType();
export const boolTargetType = csharpSourcePrimitiveTargetType("bool");
export const intTargetType = csharpSourcePrimitiveTargetType("int32");
export const longTargetType = csharpSourcePrimitiveTargetType("int64");
export const doubleTargetType = csharpSourcePrimitiveTargetType("float64");
export const voidTargetType = csharpVoidTargetType();
export const dateTargetType = csharpJsDateTargetType();
export const bufferTargetType = nodeBufferTargetType;
export const fsTargetType = csharpTargetNamedType("Tsonic.CSharp.Node.fs", undefined, csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "fs"));
export const fsPromisesTargetType = csharpTargetNamedType("Tsonic.CSharp.Node.fs_promises", undefined, csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "fs_promises"));
export const statsProviderType = { kind: "provider-ref", moduleSpecifier: "node:fs", exportName: "Stats" } satisfies ProviderTypeExpression;
export const statsTargetType = csharpTargetNamedType("Tsonic.CSharp.Node.Stats", undefined, csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "Stats"));
export const makeDirectoryOptionsProviderType = { kind: "provider-ref", moduleSpecifier: "node:fs", exportName: "MakeDirectoryOptions" } satisfies ProviderTypeExpression;
export const rmOptionsProviderType = { kind: "provider-ref", moduleSpecifier: "node:fs", exportName: "RmOptions" } satisfies ProviderTypeExpression;
export const makeDirectoryOptionsTargetType = csharpTargetNamedType("Tsonic.CSharp.Node.MakeDirectoryOptions", undefined, csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "MakeDirectoryOptions"));
export const rmOptionsTargetType = csharpTargetNamedType("Tsonic.CSharp.Node.RmOptions", undefined, csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "RmOptions"));

export function promiseProviderType(resultType: ProviderTypeExpression): ProviderTypeExpression {
  return {
    kind: "source-global",
    name: "Promise",
    typeArguments: [resultType],
  };
}

export function taskTargetType(resultType: TargetTypeRef): TargetTypeRef {
  return csharpTaskTargetType(resultType);
}

export interface NodeFsProviderParameter {
  readonly name: string;
  readonly type: ProviderTypeExpression;
  readonly optional?: boolean;
}

export type NodeFsCallTargetMember = NodejsModuleCallTargetMetadata;

export type {
  ProviderTypeExpression,
  TargetTypeRef,
};
