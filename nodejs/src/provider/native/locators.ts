import type {
  CsharpProviderTypeParameterRelation,
  CsharpTargetMember,
  CsharpTargetReceiverRelation,
} from "@tsonic/target-csharp/provider";
import type { DotnetTypeRef } from "@tsonic/target-csharp/provider/dotnet";

export type NodejsNativeTypeLocator =
  | { readonly kind: "void" | "string" | "object" }
  | { readonly kind: "source-primitive"; readonly name: Extract<DotnetTypeRef, { readonly kind: "source-primitive" }>["name"] }
  | {
      readonly kind: "named";
      readonly metadataName: string;
      readonly typeArguments: readonly NodejsNativeTypeLocator[];
    }
  | { readonly kind: "array"; readonly elementType: NodejsNativeTypeLocator; readonly rank?: number }
  | { readonly kind: "nullable" | "nullable-reference"; readonly elementType: NodejsNativeTypeLocator }
  | { readonly kind: "class-parameter" | "method-parameter"; readonly index: number };

export interface NodejsNativeTypeDeclarationLocator {
  readonly metadataName: string;
  readonly typeParameterCount: number;
}

export interface NodejsNativeCallableLocator {
  readonly owner: NodejsNativeTypeDeclarationLocator;
  readonly kind: "method" | "constructor" | "indexer";
  readonly targetName: string;
  readonly static: boolean;
  readonly typeParameterCount: number;
  readonly parameters: readonly {
    readonly type: NodejsNativeTypeLocator;
    readonly passingMode: CsharpTargetMember["parameters"][number]["passingMode"];
  }[];
  readonly returnType: NodejsNativeTypeLocator;
  readonly returnPassing: "by-value" | "byref-readwrite" | "byref-readonly";
}

export interface NodejsNativeSlotLocator {
  readonly owner: NodejsNativeTypeDeclarationLocator;
  readonly kind: "property" | "field";
  readonly targetName: string;
  readonly static: boolean;
  readonly type: NodejsNativeTypeLocator;
}

export type NodejsNativeMemberLocator = NodejsNativeCallableLocator | NodejsNativeSlotLocator;

export type NodejsNativeClassApplicationRecipe =
  | {
      readonly kind: "source-class";
      readonly original: NodejsNativeTypeDeclarationLocator;
      readonly parameters: readonly CsharpProviderTypeParameterRelation[];
    }
  | {
      readonly kind: "native-parameter-application";
      readonly original: NodejsNativeTypeDeclarationLocator;
      readonly selected: NodejsNativeCallableLocator;
      readonly targetParameterIndex: number;
      readonly referenceAnnotation: "required";
    };

export type NodejsNativeParameterRole =
  | { readonly kind: "source"; readonly sourceParameterIndex: number; readonly targetParameterIndex: number }
  | { readonly kind: "native-default"; readonly targetParameterIndex: number };

export interface NodejsNativeCallMapping {
  readonly selected: NodejsNativeCallableLocator;
  readonly receiver: CsharpTargetReceiverRelation;
  readonly parameters: readonly NodejsNativeParameterRole[];
  readonly methodTypeParameters: readonly CsharpProviderTypeParameterRelation[];
  readonly invocationTypeParameters: readonly CsharpProviderTypeParameterRelation[];
}

export type NodejsNativeMemberMapping =
  | NodejsNativeCallMapping
  | { readonly selected: NodejsNativeSlotLocator; readonly receiver: CsharpTargetReceiverRelation };

export function nativeNamedType(
  metadataName: string,
  ...typeArguments: readonly NodejsNativeTypeLocator[]
): NodejsNativeTypeLocator {
  return Object.freeze({ kind: "named", metadataName, typeArguments: Object.freeze(typeArguments) });
}

export function nativeArray(elementType: NodejsNativeTypeLocator): NodejsNativeTypeLocator {
  return Object.freeze({ kind: "array", elementType });
}

export function nativeNullable(elementType: NodejsNativeTypeLocator): NodejsNativeTypeLocator {
  return Object.freeze({ kind: "nullable", elementType });
}

export function nativeNullableReference(elementType: NodejsNativeTypeLocator): NodejsNativeTypeLocator {
  return Object.freeze({ kind: "nullable-reference", elementType });
}
