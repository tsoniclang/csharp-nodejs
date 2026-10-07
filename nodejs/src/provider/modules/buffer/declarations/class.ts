import type {
  ProviderExportDeclaration,
} from "@tsonic/tsts";
import {
  nodeBufferExportName,
  nodeBufferModuleSpecifier,
} from "../identities.js";
import {
  nodeBufferInstanceMemberDeclarations,
} from "./instance-members.js";
import {
  nodeBufferStaticMemberDeclarations,
} from "./static-members.js";
import {
  nodeBufferUnsupportedClassMemberDeclarations,
} from "../unsupported.js";

export function nodeBufferClassExport(includeJsSurfaceMembers = true): ProviderExportDeclaration {
  return {
    id: "node:buffer.Buffer",
    name: nodeBufferExportName,
    kind: "class",
    members: [
      ...nodeBufferStaticMemberDeclarations(includeJsSurfaceMembers),
      ...nodeBufferInstanceMemberDeclarations(),
      ...nodeBufferUnsupportedClassMemberDeclarations(),
    ],
  };
}
