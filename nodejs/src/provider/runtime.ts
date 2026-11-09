import { fileURLToPath } from "node:url";
import type { TargetRuntimeContributions } from "@tsonic/target-api/artifacts";
import {
  csharpCoreRuntimeSource,
  csharpJsRuntimeSource,
  csharpRuntimeSourceContributions,
} from "@tsonic/target-csharp/provider";
import type { CsharpRuntimeProjectSource } from "@tsonic/target-csharp/provider";

export const nodejsRuntimeSource: CsharpRuntimeProjectSource = Object.freeze({
  projectPath: fileURLToPath(import.meta.resolve("@tsonic/csharp-nodejs/runtime.csproj")),
  propertiesPath: fileURLToPath(import.meta.resolve("@tsonic/csharp-nodejs/runtime.props")),
  dependencies: Object.freeze({
    TsonicCsharpRuntimeProject: csharpCoreRuntimeSource,
    TsonicCsharpJsProject: csharpJsRuntimeSource,
  }),
});

export function nodejsRuntimeContributions(): TargetRuntimeContributions {
  return csharpRuntimeSourceContributions(nodejsRuntimeSource);
}
