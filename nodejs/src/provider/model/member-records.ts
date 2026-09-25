import type { CsharpTargetMember, CsharpProviderArgumentAdapter } from "@tsonic/target-csharp/provider";
import type { NodejsProviderDeclarationIdentity } from "../identity.js";
import type { NodejsUnsupportedTargetIdentity } from "./members.js";

export interface NodejsTargetMemberMetadataRecord {
  readonly argumentAdapters?: readonly (CsharpProviderArgumentAdapter | undefined)[];
  readonly declarationIdentities: readonly NodejsProviderDeclarationIdentity[];
  readonly member: CsharpTargetMember;
}

export interface NodejsUnsupportedTargetMetadataRecord {
  readonly declarationIdentities: readonly NodejsProviderDeclarationIdentity[];
  readonly identity: NodejsUnsupportedTargetIdentity;
}
