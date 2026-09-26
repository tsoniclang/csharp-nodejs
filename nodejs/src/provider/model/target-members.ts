import type { ProviderParameterDeclaration, ProviderTypeExpression, ProviderTypeParameterDeclaration } from "@tsonic/tsts";
import type { CsharpTargetMember, TargetParameter, TargetTypeRef } from "@tsonic/target-csharp/provider";
import type { NodejsClassCallTargetMember, NodejsCallArgumentAdapters, NodejsClassPropertyTargetMember, NodejsModuleCallTargetMember, NodejsModulePropertyTargetMember } from "./members.js";

export type CsharpTargetInvocation = NonNullable<
  CsharpTargetMember["csharpInvocation"]
>;

export interface NodejsModuleCallTargetMetadata extends NodejsModuleCallTargetMember {
  readonly targetMemberId: string;
  readonly targetName: string;
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType: ProviderTypeExpression;
  readonly providerTypeParameters?: readonly ProviderTypeParameterDeclaration[];
}

export interface NodejsModulePropertyTargetMetadata extends NodejsModulePropertyTargetMember {
  readonly targetMemberId: string;
  readonly targetName: string;
  readonly providerType: ProviderTypeExpression;
}

export interface NodejsClassCallTargetMetadata extends NodejsClassCallTargetMember {
  readonly targetMemberId: string;
  readonly targetName: string;
  readonly memberKind: "constructor" | "method";
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType?: ProviderTypeExpression;
  readonly providerTypeParameters?: readonly ProviderTypeParameterDeclaration[];
  readonly static?: boolean;
}

export interface NodejsClassPropertyTargetMetadata extends NodejsClassPropertyTargetMember {
  readonly targetMemberId: string;
  readonly targetName: string;
  readonly memberKind: "property" | "indexer";
  readonly providerType: ProviderTypeExpression;
  readonly readonly?: true;
  readonly optional?: true;
}

export interface NodejsModuleCallTargetMetadataRow extends NodejsCallArgumentAdapters {
  readonly exportName: string;
  readonly signatureId: string;
  readonly targetMemberId: string;
  readonly sourceName: string;
  readonly targetName: string;
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType: ProviderTypeExpression;
  readonly providerTypeParameters?: readonly ProviderTypeParameterDeclaration[];
  readonly targetParameters: readonly TargetParameter[];
  readonly targetReturnType: TargetTypeRef;
  readonly declaringType: TargetTypeRef;
  readonly targetTypeParameters?: CsharpTargetMember["typeParameters"];
}

export interface NodejsModulePropertyTargetMetadataRow {
  readonly exportName: string;
  readonly targetMemberId: string;
  readonly sourceName: string;
  readonly targetName: string;
  readonly providerType: ProviderTypeExpression;
  readonly targetReturnType: TargetTypeRef;
  readonly declaringType: TargetTypeRef;
}

export interface NodejsClassCallTargetMetadataRow extends NodejsCallArgumentAdapters {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly signatureId: string;
  readonly targetMemberId: string;
  readonly sourceName: string;
  readonly targetName: string;
  readonly memberKind: "constructor" | "method";
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType?: ProviderTypeExpression;
  readonly providerTypeParameters?: readonly ProviderTypeParameterDeclaration[];
  readonly targetParameters: readonly TargetParameter[];
  readonly targetReturnType: TargetTypeRef;
  readonly declaringType: TargetTypeRef;
  readonly targetTypeParameters?: CsharpTargetMember["typeParameters"];
  readonly static?: boolean;
  readonly csharpInvocation?: CsharpTargetInvocation;
}

export interface NodejsClassPropertyTargetMetadataRow {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly signatureId?: string;
  readonly targetMemberId: string;
  readonly sourceName: string;
  readonly targetName: string;
  readonly memberKind: "property" | "indexer";
  readonly providerType: ProviderTypeExpression;
  readonly targetParameters: readonly TargetParameter[];
  readonly targetReturnType: TargetTypeRef;
  readonly declaringType: TargetTypeRef;
  readonly readonly?: true;
  readonly optional?: true;
}
