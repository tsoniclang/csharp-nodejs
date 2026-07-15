import type {
  NodejsProviderDeclarationIdentity,
} from "../identity.js";
import {
  canonicalNodejsModuleSpecifier,
  isSupportedNodejsModuleSpecifier,
} from "../module-specifiers.js";

export function isNodejsProviderModule(moduleSpecifier: string | undefined): boolean {
  return isSupportedNodejsModuleSpecifier(moduleSpecifier);
}

export function canonicalNodejsDeclarationIdentity(declaration: NodejsProviderDeclarationIdentity): NodejsProviderDeclarationIdentity {
  const canonicalSpecifier = canonicalNodejsModuleSpecifier(declaration.moduleSpecifier);
  return canonicalSpecifier === undefined
    ? declaration
    : {
        ...declaration,
        providerModuleId: canonicalSpecifier,
        moduleSpecifier: canonicalSpecifier,
      };
}
