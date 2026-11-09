import type { NodejsModuleCallTargetMetadata } from "../../model/target-members.js";
import { nativeCall } from "../../native/authoring.js";
import { pathOwner } from "./types.js";
import { pathOperations } from "./operations.js";

export function nodePathCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return pathOperations.map(operation => ({
    exportName: operation.name, signatureId: `node:path.${operation.name}${operation.signature}`,
    providerParameters: operation.sourceParameters, providerReturnType: operation.sourceReturn,
    native: nativeCall({ owner: pathOwner, kind: "method", targetName: operation.name, static: true,
      parameters: operation.parameters, returnType: operation.returnType, receiver: { kind: "none" } }),
  }));
}

