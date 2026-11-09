import type { ProviderParameterDeclaration, ProviderSignatureDeclaration, ProviderTypeExpression, ProviderTypeParameterDeclaration } from "@tsonic/tsts";
import type { NodejsClassCallTargetMember, NodejsClassPropertyTargetMember, NodejsModuleCallTargetMember, NodejsModulePropertyTargetMember } from "./members.js";

export interface NodejsModuleCallTargetMetadata extends NodejsModuleCallTargetMember {
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType: ProviderTypeExpression;
  readonly providerTypeParameters?: readonly ProviderTypeParameterDeclaration[];
}

export interface NodejsModulePropertyTargetMetadata extends NodejsModulePropertyTargetMember {
  readonly providerType: ProviderTypeExpression;
}

export interface NodejsClassCallTargetMetadata extends NodejsClassCallTargetMember {
  readonly memberKind: "constructor" | "method";
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType?: ProviderTypeExpression;
  readonly providerTypeParameters?: readonly ProviderTypeParameterDeclaration[];
  readonly accessibility?: ProviderSignatureDeclaration["accessibility"];
}

export interface NodejsClassPropertyTargetMetadata extends NodejsClassPropertyTargetMember {
  readonly memberKind: "property" | "indexer";
  readonly providerType: ProviderTypeExpression;
  readonly readonly?: true;
  readonly optional?: true;
}
