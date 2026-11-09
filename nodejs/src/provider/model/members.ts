import type { NodejsNativeCallMapping, NodejsNativeMemberMapping } from "../native/locators.js";

export interface NodejsModuleCallTargetMember {
  readonly exportName: string;
  readonly signatureId: string;
  readonly native: NodejsNativeCallMapping;
}

export interface NodejsModulePropertyTargetMember {
  readonly exportName: string;
  readonly native: NodejsNativeMemberMapping;
}

export interface NodejsClassCallTargetMember {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly signatureId: string;
  readonly static?: boolean;
  readonly native: NodejsNativeCallMapping;
}

export interface NodejsClassPropertyTargetMember {
  readonly exportName: string;
  readonly memberName: string;
  readonly memberId: string;
  readonly signatureId?: string;
  readonly static?: boolean;
  readonly native: NodejsNativeMemberMapping;
}

export interface NodejsUnsupportedTargetIdentity {
  readonly exportName: string;
  readonly memberName?: string;
  readonly memberId?: string;
  readonly signatureId?: string;
  readonly targetIdentityId: string;
  readonly displayName: string;
}
