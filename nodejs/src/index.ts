import { createRequire } from "node:module";
import { dirname, resolve } from "node:path";
import type {
  TargetCapabilityContext,
  TsonicTargetCapabilityPlugin,
} from "@tsonic/target-api/provider";
import type { TargetRuntimeContributions } from "@tsonic/target-api/artifacts";
import {
  createCsharpNodejsProviderPackageExtension,
  createCsharpNodejsTargetContributions,
  nodejsProviderPackageModuleOwnership,
} from "./provider/index.js";

const require = createRequire(import.meta.url);
const csharpJsPackageRoot = dirname(require.resolve("@tsonic/csharp-js/package.json"));

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
    moduleOwnership: nodejsProviderPackageModuleOwnership,
    sourceCompilerContributions(context: TargetCapabilityContext) {
      return {
        extensions: [createCsharpNodejsProviderPackageExtension(context)],
      };
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
            kind: "assembly",
            include: "Tsonic.CSharp.Js",
            attributes: {
              HintPath: resolve(csharpJsPackageRoot, "runtimes/net10.0/Tsonic.CSharp.Js.dll"),
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
