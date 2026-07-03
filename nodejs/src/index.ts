import type {
  TargetCapabilityContext,
  TargetRuntimeContributions,
  TsonicTargetCapabilityPlugin,
} from "@tsonic/target-api";
import {
  createCsharpNodejsProviderPackageExtension,
  createCsharpNodejsProviderPackageOperationsMappers,
  nodejsProviderPackageModuleOwnership,
} from "./provider/index.js";
import type {
  CsharpProviderPackageOperationMapperContributor,
} from "@tsonic/target-csharp";

export {
  createCsharpNodejsProviderPackageBindingProvider,
  createCsharpNodejsProviderPackageExtension,
  createCsharpNodejsProviderPackageOperationsMappers,
  createCsharpNodejsProviderPackageOperationsProvider,
  nodejsProviderPackageModuleOwnership,
} from "./provider/index.js";

export function createTsonicPlugin(): TsonicTargetCapabilityPlugin & CsharpProviderPackageOperationMapperContributor {
  return {
    kind: "target-capability",
    id: "@tsonic/csharp-nodejs",
    targetId: "csharp",
    displayName: "C# NodeJS capability package",
    requiredSurfaces: ["js"],
    moduleOwnership: nodejsProviderPackageModuleOwnership,
    createExtensions(context: TargetCapabilityContext) {
      return [createCsharpNodejsProviderPackageExtension(context)];
    },
    createCsharpOperationsMappers: createCsharpNodejsProviderPackageOperationsMappers,
    runtimeContributions(): TargetRuntimeContributions {
      return {
        references: [
          {
            kind: "assembly",
            include: "Tsonic.CSharp.Node",
            attributes: {
              HintPath: new URL("../runtimes/net10.0/Tsonic.CSharp.Node.dll", import.meta.url).pathname,
            },
          },
          {
            kind: "package",
            include: "BouncyCastle.Cryptography",
            version: "2.4.0",
          },
          {
            kind: "framework",
            include: "Microsoft.AspNetCore.App",
          },
        ],
      };
    },
  };
}
