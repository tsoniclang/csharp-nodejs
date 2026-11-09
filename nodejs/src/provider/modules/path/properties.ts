import type { NodejsModulePropertyTargetMetadata } from "../../model/target-members.js";
import { nativeSlot } from "../../native/authoring.js";
import { nativePathModule, pathModuleProviderType, pathOwner, stringProviderType } from "./types.js";

export function nodePathPropertyTargetMembers(): readonly NodejsModulePropertyTargetMetadata[] {
  return [
    ...["sep", "delimiter"].map(exportName => ({ exportName, providerType: stringProviderType,
      native: nativeSlot(pathOwner, exportName, { kind: "string" }, true, "field") })),
    ...["posix", "win32"].map(exportName => ({ exportName, providerType: pathModuleProviderType,
      native: nativeSlot(pathOwner, exportName, nativePathModule, true) })),
  ];
}

