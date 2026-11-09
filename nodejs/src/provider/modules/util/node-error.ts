import type { ProviderExportDeclaration } from "@tsonic/tsts";
import { nativeOwner, nativeSlot } from "../../native/authoring.js";
import { providerRef, unionProviderType } from "../../declarations/exports.js";
import { stringProviderType, undefinedProviderType } from "../../model/source-types.js";

export const nodeErrorProviderType = providerRef("node:util", "NodeError");
export const optionalNodeErrorProviderType = unionProviderType(nodeErrorProviderType, undefinedProviderType);

export function nodeErrorExportDeclaration(): ProviderExportDeclaration {
  return {
    id: "node:util.NodeError",
    name: "NodeError",
    kind: "interface",
    heritage: [{ kind: "extends", type: { kind: "source-global", name: "Readonly", typeArguments: [
      { kind: "source-global", name: "Error" },
    ] } }],
    members: [{
      id: "node:util.NodeError.message",
      name: "message",
      kind: "property",
      readonly: true,
      type: stringProviderType,
    }],
  };
}

export function nodeErrorClassPropertyTargetMembers() {
  return [{
    exportName: "NodeError",
    memberName: "message",
    memberId: "node:util.NodeError.message",
    memberKind: "property",
    providerType: stringProviderType,
    native: nativeSlot(nativeOwner("System.Exception"), "Message", { kind: "string" }),
    readonly: true,
  }];
}
