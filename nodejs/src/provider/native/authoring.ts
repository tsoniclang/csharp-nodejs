import type { CsharpProviderTypeParameterRelation, CsharpTargetMember, CsharpTargetReceiverRelation } from "@tsonic/target-csharp/provider";
import type { NodejsNativeCallMapping, NodejsNativeSlotLocator, NodejsNativeTypeDeclarationLocator, NodejsNativeTypeLocator } from "./locators.js";

export interface NodejsNativeParameterMapping {
  readonly type: NodejsNativeTypeLocator;
  readonly passingMode: CsharpTargetMember["parameters"][number]["passingMode"];
  readonly sourceParameterIndex: number | undefined;
}

export function nativeParameter(
  sourceParameterIndex: number,
  type: NodejsNativeTypeLocator,
  passingMode: NodejsNativeParameterMapping["passingMode"] = "by-value",
): NodejsNativeParameterMapping {
  return Object.freeze({ type, passingMode, sourceParameterIndex });
}

export function nativeDefault(type: NodejsNativeTypeLocator): NodejsNativeParameterMapping {
  return Object.freeze({ type, passingMode: "by-value", sourceParameterIndex: undefined });
}

export function nativeOwner(metadataName: string, typeParameterCount = 0): NodejsNativeTypeDeclarationLocator {
  return Object.freeze({ metadataName, typeParameterCount });
}

export function nativeCall(input: {
  readonly owner: NodejsNativeTypeDeclarationLocator;
  readonly targetName: string;
  readonly kind: "method" | "constructor" | "indexer";
  readonly static: boolean;
  readonly parameters: readonly NodejsNativeParameterMapping[];
  readonly returnType: NodejsNativeTypeLocator;
  readonly returnPassing?: "by-value" | "byref-readwrite" | "byref-readonly";
  readonly typeParameterCount?: number;
  readonly receiver: CsharpTargetReceiverRelation;
  readonly methodTypeParameters?: readonly CsharpProviderTypeParameterRelation[];
  readonly invocationTypeParameters?: readonly CsharpProviderTypeParameterRelation[];
}): NodejsNativeCallMapping {
  return Object.freeze({
    selected: Object.freeze({
      owner: input.owner, kind: input.kind, targetName: input.targetName, static: input.static,
      parameters: Object.freeze(input.parameters.map(({ type, passingMode }) => Object.freeze({ type, passingMode }))),
      returnType: input.returnType, returnPassing: input.returnPassing ?? "by-value",
      typeParameterCount: input.typeParameterCount ?? 0,
    }),
    receiver: input.receiver,
    parameters: Object.freeze(input.parameters.map((parameter, targetParameterIndex) => parameter.sourceParameterIndex === undefined
      ? Object.freeze({ kind: "native-default" as const, targetParameterIndex })
      : Object.freeze({ kind: "source" as const, sourceParameterIndex: parameter.sourceParameterIndex, targetParameterIndex }))),
    methodTypeParameters: Object.freeze(input.methodTypeParameters ?? []),
    invocationTypeParameters: Object.freeze(input.invocationTypeParameters ?? []),
  });
}

export function nativeSlot(
  owner: NodejsNativeTypeDeclarationLocator,
  targetName: string,
  type: NodejsNativeTypeLocator,
  isStatic = false,
  kind: "property" | "field" = "property",
): { readonly selected: NodejsNativeSlotLocator; readonly receiver: CsharpTargetReceiverRelation } {
  return Object.freeze({ selected: Object.freeze({ owner, targetName, type, static: isStatic, kind }),
    receiver: Object.freeze({ kind: isStatic ? "none" : "instance" }) });
}
