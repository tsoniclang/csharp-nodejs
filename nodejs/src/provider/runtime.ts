import { fileURLToPath } from "node:url";
import type { TargetRuntimeContributions } from "@tsonic/target-api/artifacts";

const csharpJsPackageUrl = import.meta.resolve("@tsonic/csharp-js/package.json");

export function nodejsRuntimeContributions(): TargetRuntimeContributions {
  return {
    references: [
      {
        kind: "assembly",
        include: "Tsonic.CSharp.Node",
        attributes: {
          HintPath: new URL("../../runtimes/net10.0/Tsonic.CSharp.Node.dll", import.meta.url).pathname,
        },
      },
      {
        kind: "assembly",
        include: "Tsonic.CSharp.Js",
        attributes: {
          HintPath: fileURLToPath(new URL("runtimes/net10.0/Tsonic.CSharp.Js.dll", csharpJsPackageUrl)),
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
}
