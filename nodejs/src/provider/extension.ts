import type {
  CompilerExtension,
} from "@tsonic/tsts";
import type {
  TargetCapabilityContext,
  TargetProviderModuleOwnership,
} from "@tsonic/target-api/provider";
import {
  csharpProviderVersion,
} from "@tsonic/target-csharp/provider";
import {
  createCsharpNodejsProviderPackageBindingProvider,
} from "./provider.js";
import {
  nodejsProviderPackageOwnedModuleSpecifiers,
} from "./module-specifiers.js";
import {
  createCsharpNodejsProviderPolicyContribution,
} from "./target-relations.js";

export const csharpNodejsProviderPackageExtensionId =
  "tsonic.csharp.provider-package.nodejs";

export const nodejsProviderPackageModuleOwnership:
  readonly TargetProviderModuleOwnership[] =
    nodejsProviderPackageOwnedModuleSpecifiers().map((specifierPrefix) => ({
      specifierPrefix,
      message:
        `target 'csharp' capability '@tsonic/csharp-nodejs' must be installed to import Node.js built-in provider module '${specifierPrefix}'`,
    }));

export function createCsharpNodejsProviderPackageExtension(
  _context: TargetCapabilityContext,
): CompilerExtension {
  return {
    identity: {
      id: csharpNodejsProviderPackageExtensionId,
      version: csharpProviderVersion,
    },
    initialize(extensionContext): void {
      extensionContext.registerSourceDeclarationProvider(
        createCsharpNodejsProviderPackageBindingProvider(),
      );
    },
  };
}

export function createCsharpNodejsTargetContributions(
  _context: TargetCapabilityContext,
) {
  return [createCsharpNodejsProviderPolicyContribution()];
}
