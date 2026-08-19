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
} from "../../module-defaults.js";

export function nodeBufferExports(): readonly ProviderExportDeclaration[] {
  const declarations = [
    nodeBufferClassExport(),
    ...nodeBufferFunctionExports(),
  ];
  return [
    ...declarations,
    ...nodejsDefaultModuleObjectExports(nodeBufferModuleSpecifier, declarations),
  ];
}
