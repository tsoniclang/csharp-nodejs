import type {
  ProviderExportDeclaration,
} from "@tsonic/tsts";
import {
  nodeBufferClassExport,
} from "./class.js";
import {
  nodeBufferFunctionExports,
} from "./functions.js";
import {
  nodeBufferModuleSpecifier,
} from "../identities.js";
import {
  nodejsDefaultModuleObjectExports,
} from "../../../declarations/defaults.js";

export function nodeBufferExports(includeJsSurfaceMembers = true): readonly ProviderExportDeclaration[] {
  const declarations = [
    nodeBufferClassExport(includeJsSurfaceMembers),
    ...nodeBufferFunctionExports(),
  ];
  return [
    ...declarations,
    ...nodejsDefaultModuleObjectExports(nodeBufferModuleSpecifier, declarations),
  ];
}
