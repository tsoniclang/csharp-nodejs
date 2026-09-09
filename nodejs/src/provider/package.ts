import type { ExtensionDiagnostic } from "@tsonic/tsts";
import { createCsharpProviderPackage } from "@tsonic/target-csharp/provider";
import {
  csharpNodejsProviderPackageProviderIdentity,
  csharpNodejsVirtualDeclarationFileName,
} from "./identity.js";
import { nodejsProviderModules } from "./modules/catalog.js";
import { nodejsProviderModuleSpecifiers } from "./modules/specifiers.js";
import { nodejsRuntimeContributions } from "./runtime.js";
import { createCsharpNodejsProviderPolicyContribution } from "./target-relations.js";

export function createCsharpNodejsProviderPackage() {
  return createCsharpProviderPackage({
    id: "@tsonic/csharp-nodejs",
    displayName: "C# NodeJS capability package",
    providerIdentity: csharpNodejsProviderPackageProviderIdentity,
    modules: nodejsProviderModules,
    moduleSpecifiers: nodejsProviderModuleSpecifiers(),
    virtualDeclarationFileName: csharpNodejsVirtualDeclarationFileName,
    moduleDiagnostic: nodejsProviderModuleDiagnostic,
    resolutionEvidence: [{ message: "C# NodeJS provider package supplied virtual module." }],
    declarationEvidence: [{ message: "C# NodeJS provider package virtual declaration model." }],
    policy: createCsharpNodejsProviderPolicyContribution(),
    runtime: nodejsRuntimeContributions(),
  });
}

function nodejsProviderModuleDiagnostic(
  kind: "unowned" | "missing",
  moduleSpecifier: string,
): ExtensionDiagnostic {
  return {
    extensionId: csharpNodejsProviderPackageProviderIdentity.id,
    extensionCode: kind === "unowned"
      ? "NODEJS_PROVIDER_PACKAGE_MODULE_UNOWNED"
      : "NODEJS_PROVIDER_PACKAGE_MODULE_MISSING",
    numericCode: kind === "unowned" ? 9300001 : 9300002,
    category: "error",
    message: kind === "unowned"
      ? `C# NodeJS provider package does not own '${moduleSpecifier}'.`
      : `C# NodeJS provider package has no declaration model for '${moduleSpecifier}'.`,
  };
}
