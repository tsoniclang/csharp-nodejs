import type {
  CsharpTargetMember,
} from "@tsonic/target-csharp/provider";

export interface NodejsModuleCallTargetMember {
  readonly exportName: string;
  readonly signatureId: string;
  readonly member: CsharpTargetMember;
}

export interface NodejsModulePropertyTargetMember {
  readonly exportName: string;
  readonly member: CsharpTargetMember;
}

export interface NodejsClassCallTargetMember {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly signatureId: string;
  readonly static?: boolean;
  readonly member: CsharpTargetMember;
}

export interface NodejsClassPropertyTargetMember {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly signatureId?: string;
  readonly member: CsharpTargetMember;
}

export interface NodejsUnsupportedTargetIdentity {
  readonly exportName: string;
  readonly memberName?: string;
  readonly memberId?: string;
  readonly signatureId?: string;
  readonly targetIdentityId: string;
  readonly displayName: string;
}
