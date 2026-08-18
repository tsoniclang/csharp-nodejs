import type {
  ProviderExportDeclaration,
} from "@tsonic/tsts";
import {
  nodeFsCallExportDeclarations,
} from "./calls.js";
import {
  nodeFsStatsExportDeclaration,
} from "./stats.js";
import {
  nodeFsPromisesExportDeclarations,
} from "./promises.js";
import {
  nodeFsModuleSpecifier,
  nodeFsPromisesModuleSpecifier,
} from "./identities.js";
import {
  nodejsDefaultModuleObjectExports,
} from "../module-defaults.js";

export function nodeFsExports(): readonly ProviderExportDeclaration[] {
  const exports = [
    nodeFsStatsExportDeclaration(),
    ...nodeFsCallExportDeclarations(),
  ];
  return [
    ...exports,
    ...nodejsDefaultModuleObjectExports(nodeFsModuleSpecifier, exports),
  ];
}

export function nodeFsPromisesExports(): readonly ProviderExportDeclaration[] {
  const exports = nodeFsPromisesExportDeclarations();
  return [
    ...exports,
    ...nodejsDefaultModuleObjectExports(nodeFsPromisesModuleSpecifier, exports),
  ];
}
