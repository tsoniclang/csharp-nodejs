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
  nodeFsOptionExportDeclarations,
} from "./options.js";
import {
  nodeFsPromisesExportDeclarations,
} from "./promises.js";
import {
  nodeFsModuleSpecifier,
  nodeFsPromisesModuleSpecifier,
} from "./identities.js";
import {
  nodejsDefaultModuleObjectExports,
} from "../defaults.js";
import {
  nodeFsStreamExportDeclarations,
} from "./streams.js";

export function nodeFsExports(options: {
  readonly includeJsSurfaceMembers: boolean;
}): readonly ProviderExportDeclaration[] {
  const exports = [
    ...nodeFsOptionExportDeclarations(),
    nodeFsStatsExportDeclaration(options.includeJsSurfaceMembers),
    ...nodeFsStreamExportDeclarations(),
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
