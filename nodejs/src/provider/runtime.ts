import { fileURLToPath } from "node:url";
import type { TargetRuntimeContributions } from "@tsonic/target-api/artifacts";
import {
  csharpCoreRuntimeSource,
  csharpJsRuntimeSource,
  csharpRuntimeSourceContributions,
} from "@tsonic/target-csharp/provider";

export function nodejsRuntimeContributions(): TargetRuntimeContributions {
  return csharpRuntimeSourceContributions({
    projectPath: fileURLToPath(import.meta.resolve("@tsonic/csharp-nodejs/runtime.csproj")),
    propertiesPath: fileURLToPath(import.meta.resolve("@tsonic/csharp-nodejs/runtime.props")),
    dependencies: {
      TsonicCsharpRuntimeProject: csharpCoreRuntimeSource,
      TsonicCsharpJsProject: csharpJsRuntimeSource,
    },
  });
}
