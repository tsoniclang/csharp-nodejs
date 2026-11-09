import type { NodejsNativeMemberMapping } from "../native/locators.js";
import type { NodejsProviderDeclarationIdentity } from "./identity.js";
import type { NodejsUnsupportedTargetIdentity } from "./members.js";

export interface NodejsTargetMemberMetadataRecord {
  readonly declarationIdentities: readonly NodejsProviderDeclarationIdentity[];
  readonly native: NodejsNativeMemberMapping;
}

export interface NodejsUnsupportedTargetMetadataRecord {
  readonly declarationIdentities: readonly NodejsProviderDeclarationIdentity[];
  readonly identity: NodejsUnsupportedTargetIdentity;
}
