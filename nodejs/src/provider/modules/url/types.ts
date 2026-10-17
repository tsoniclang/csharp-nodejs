import type {
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import type { CsharpTargetMember } from "@tsonic/target-csharp/provider";

export interface NodeUrlCallTargetMember {
  readonly exportName: string;
  readonly signatureId: string;
  readonly targetMemberId: string;
  readonly targetName: string;
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType: ProviderTypeExpression;
  readonly member: CsharpTargetMember;
}

export interface NodeUrlClassCallTargetMember {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly signatureId: string;
  readonly targetMemberId: string;
  readonly targetName: string;
  readonly memberKind: "constructor" | "method";
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType?: ProviderTypeExpression;
  readonly static?: boolean;
  readonly member: CsharpTargetMember;
}

export interface NodeUrlClassPropertyTargetMember {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly targetMemberId: string;
  readonly targetName: string;
  readonly providerType: ProviderTypeExpression;
  readonly readonly?: true;
  readonly member: CsharpTargetMember;
}

export interface NodeUrlUnsupportedTargetIdentity {
  readonly exportName: string;
  readonly memberName?: string;
  readonly memberId?: string;
  readonly signatureId?: string;
  readonly targetIdentityId: string;
  readonly displayName: string;
}
