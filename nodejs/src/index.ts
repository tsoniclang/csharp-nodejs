import type {
  TargetCapabilityContext,
  TargetRuntimeContributions,
  TsonicTargetCapabilityPlugin,
} from "@tsonic/target-api";
import {
  createCsharpNodejsProviderPackageExtension,
  createCsharpNodejsTargetContributions,
  nodejsProviderPackageModuleOwnership,
} from "./provider/index.js";

export {
  createCsharpNodejsProviderPackageBindingProvider,
  createCsharpNodejsProviderPackageExtension,
  createCsharpNodejsTargetContributions,
  nodejsProviderPackageModuleOwnership,
} from "./provider/index.js";

export function createTsonicPlugin(): TsonicTargetCapabilityPlugin {
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
    createTargetContributions: createCsharpNodejsTargetContributions,
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
