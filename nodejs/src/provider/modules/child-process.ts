import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import {
  csharpJsArrayTargetType, csharpJsTypedArrayTargetType, csharpNullableTargetType,
  csharpNullableValueTargetType, csharpQualifiedTypeRenderShape, csharpRuntimeUnionTargetType,
  csharpRuntimeNullTargetType, csharpRuntimeUndefinedTargetType,
  csharpSourcePrimitiveTargetType, csharpStringTargetType, csharpTargetNamedType, targetParameter,
} from "@tsonic/target-csharp/provider";
import type { CsharpTargetNamedTypeRef, TargetTypeRef } from "@tsonic/target-csharp/provider";
import { nodeBufferTargetType } from "./buffer/identities.js";
import { nodejsClassPropertyTargetMetadata, nodejsModuleCallTargetMetadata } from "../members/target-member-metadata.js";
import type { NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../members/target-member-metadata.js";
import { nodejsDefaultModuleObjectExports } from "./defaults.js";

export const nodeChildProcessModuleSpecifier = "node:child_process";
export const nodeChildProcessSpawnSyncReturnsExportName = "SpawnSyncReturns";
const optionsExportName = "SpawnSyncOptionsWithBufferEncoding";
const errorExportName = "SpawnSyncError";
const stringType: ProviderTypeExpression = { kind: "string" };
const numberType: ProviderTypeExpression = { kind: "number" };
const nullType: ProviderTypeExpression = { kind: "literal", value: null };
const undefinedType: ProviderTypeExpression = { kind: "undefined" };
const providerRef = (exportName: string, moduleSpecifier = nodeChildProcessModuleSpecifier): ProviderTypeExpression =>
  ({ kind: "provider-ref", moduleSpecifier, exportName });
const union = (...types: ProviderTypeExpression[]): ProviderTypeExpression => ({ kind: "union", types });
const bufferType = providerRef("Buffer", "node:buffer");
const stringArrayType: ProviderTypeExpression = { kind: "array", elementType: stringType };
const resultType: ProviderTypeExpression = {
  kind: "provider-ref", moduleSpecifier: nodeChildProcessModuleSpecifier,
  exportName: nodeChildProcessSpawnSyncReturnsExportName, typeArguments: [bufferType],
};
const nativeType = (name: string) => csharpTargetNamedType(
  `Tsonic.CSharp.Node.${name}`, undefined, csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", name),
);
const stringTarget = csharpStringTargetType();
const intTarget = csharpSourcePrimitiveTargetType("int32");
const numberTarget = csharpSourcePrimitiveTargetType("float64");
const resultTarget = nativeType("SpawnSyncResult");
const errorTarget = nativeType("SpawnSyncError");
const moduleTarget = nativeType("child_process");

function requiredUnion(arms: readonly TargetTypeRef[]): TargetTypeRef {
  const result = csharpRuntimeUnionTargetType(arms);
  if (result === undefined) throw new Error("Invalid closed child-process union.");
  return result;
}

export function nodeChildProcessOptionsTarget(includeJsSurfaceMembers: boolean) {
  const input = includeJsSurfaceMembers
    ? requiredUnion([csharpJsTypedArrayTargetType("Uint8Array"), nodeBufferTargetType, csharpRuntimeUndefinedTargetType()])
    : nodeBufferTargetType;
  const descriptor = requiredUnion([numberTarget, stringTarget, csharpRuntimeNullTargetType(), csharpRuntimeUndefinedTargetType()]);
  const stdio: TargetTypeRef = includeJsSurfaceMembers
    ? csharpJsArrayTargetType(descriptor) : { kind: "array", element: descriptor };
  return {
    input, stdio,
    type: csharpTargetNamedType("Tsonic.CSharp.Node.SpawnSyncOptions`2", [input, stdio],
      csharpQualifiedTypeRenderShape("Tsonic.CSharp.Node", "SpawnSyncOptions")),
  };
}

function parameters(options: boolean): readonly ProviderParameterDeclaration[] {
  return [
    { name: "command", type: stringType }, { name: "args", type: stringArrayType },
    ...(options ? [{ name: "options", type: providerRef(optionsExportName) }] : []),
  ];
}

function signatureId(options: boolean): string {
  return `node:child_process.spawnSync(System.String,System.String[]${options ? ",SpawnSyncOptionsWithBufferEncoding" : ""})`;
}

function property(
  exportName: string, owner: CsharpTargetNamedTypeRef, name: string, type: ProviderTypeExpression,
  target: TargetTypeRef, optional = false, readonly = false,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName, memberName: name, memberId: `node:child_process.${exportName}.${name}`,
    targetMemberId: `${owner.id}.${name}`,
    sourceName: name, targetName: name, memberKind: "property", providerType: type,
    targetParameters: [], targetReturnType: target, declaringType: owner,
    ...(optional ? { optional: true } : {}), ...(readonly ? { readonly: true } : {}),
  });
}

export function nodeChildProcessClassPropertyTargetMembers(includeJsSurfaceMembers = true): readonly NodejsClassPropertyTargetMetadata[] {
  const options = nodeChildProcessOptionsTarget(includeJsSurfaceMembers);
  const optionsTarget = options.type;
  return [
    ...["stdout", "stderr"].map(name => property(
      nodeChildProcessSpawnSyncReturnsExportName, resultTarget, name,
      union({ kind: "type-parameter", name: "T" }, nullType), csharpNullableTargetType(nodeBufferTargetType),
    )),
    property(nodeChildProcessSpawnSyncReturnsExportName, resultTarget, "status", union(numberType, nullType), csharpNullableValueTargetType(intTarget)),
    property(nodeChildProcessSpawnSyncReturnsExportName, resultTarget, "pid", numberType, csharpNullableValueTargetType(numberTarget), true),
    property(nodeChildProcessSpawnSyncReturnsExportName, resultTarget, "signal", union(providerRef("Signals", "node:process"), nullType), csharpNullableTargetType(stringTarget)),
    property(nodeChildProcessSpawnSyncReturnsExportName, resultTarget, "error", providerRef(errorExportName), csharpNullableTargetType(errorTarget), true),
    ...["message", "code"].map(name => property(errorExportName, errorTarget, name, stringType, stringTarget, false, true)),
    property(optionsExportName, optionsTarget, "encoding", { kind: "literal", value: "buffer" }, csharpNullableTargetType(stringTarget), true),
    property(optionsExportName, optionsTarget, "cwd", stringType, csharpNullableTargetType(stringTarget), true),
    property(optionsExportName, optionsTarget, "env", providerRef("ProcessEnv", "node:process"), csharpNullableTargetType(nativeType("ProcessEnv")), true),
    ...["maxBuffer", "uid", "gid", "timeout"].map(name =>
      property(optionsExportName, optionsTarget, name, numberType, csharpNullableValueTargetType(numberTarget), true)),
    property(optionsExportName, optionsTarget, "killSignal", providerRef("Signals", "node:process"), csharpNullableTargetType(stringTarget), true),
    property(optionsExportName, optionsTarget, "input", union(bufferType, { kind: "source-global", name: "Uint8Array" }),
      csharpNullableTargetType(options.input), true),
    property(optionsExportName, optionsTarget, "stdio", {
      kind: "array", elementType: union(numberType, nullType, undefinedType,
        ...["pipe", "ignore", "inherit"].map(value => ({ kind: "literal" as const, value }))),
    }, csharpNullableTargetType(options.stdio), true),
  ];
}

export function nodeChildProcessExports(includeJsSurfaceMembers = true): readonly ProviderExportDeclaration[] {
  const properties = nodeChildProcessClassPropertyTargetMembers(includeJsSurfaceMembers);
  const exports: readonly ProviderExportDeclaration[] = [
    ...[nodeChildProcessSpawnSyncReturnsExportName, optionsExportName, errorExportName].map(name => ({
      id: `node:child_process.${name}`, name, kind: "interface" as const,
      ...(name === nodeChildProcessSpawnSyncReturnsExportName ? { typeParameters: [{ name: "T" }] } : {}),
      members: properties.filter(member => member.exportName === name).map(member => ({
        id: member.memberId, name: member.memberName, kind: "property" as const,
        type: name === optionsExportName && member.memberName === "input" && !includeJsSurfaceMembers
          ? bufferType : member.providerType,
        ...(member.optional === true ? { optional: true } : {}),
        ...(member.readonly === true ? { readonly: true } : {}),
      })),
    })),
    { id: "node:child_process.spawnSync", name: "spawnSync", kind: "function",
      signatures: [false, true].map(options => ({
        id: signatureId(options), parameters: parameters(options), returnType: resultType,
      })) },
  ];
  return [...exports, ...nodejsDefaultModuleObjectExports(nodeChildProcessModuleSpecifier, exports)];
}

export function nodeChildProcessCallTargetMembers(includeJsSurfaceMembers = true): readonly NodejsModuleCallTargetMetadata[] {
  const optionsTarget = nodeChildProcessOptionsTarget(includeJsSurfaceMembers).type;
  const argument = includeJsSurfaceMembers
    ? { carrier: csharpJsArrayTargetType(stringTarget), id: "Tsonic.CSharp.Js.JSArray`1" }
    : { carrier: { kind: "array", element: stringTarget } as TargetTypeRef, id: "System.String[]" };
  return [false, true].map(options => nodejsModuleCallTargetMetadata({
    exportName: "spawnSync", signatureId: signatureId(options),
    targetMemberId: `Tsonic.CSharp.Node.child_process.spawnSyncResult(System.String,${argument.id}${options ? ",Tsonic.CSharp.Node.SpawnSyncOptions" : ""})`,
    sourceName: "spawnSync", targetName: "spawnSyncResult",
    providerParameters: parameters(options), providerReturnType: resultType,
    targetParameters: [targetParameter("command", stringTarget), targetParameter("args", argument.carrier),
      ...(options ? [targetParameter("options", optionsTarget)] : [])],
    targetReturnType: resultTarget, declaringType: moduleTarget,
  }));
}
