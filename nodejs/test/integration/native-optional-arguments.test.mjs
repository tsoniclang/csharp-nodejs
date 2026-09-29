import test from "node:test";
import { fileURLToPath } from "node:url";
import { compileCsharpSource } from "../../../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import { executeCsharpConstruction } from "../../../../tsonic-csharp/test/helpers/native-construction.mjs";
import { createTsonicPlugin } from "../../../dist/index.js";

test("Node optional arguments preserve omitted and explicit absence at native calls", { timeout: 300_000 }, () => {
  const compiled = compileCsharpSource({ surface: "js", capabilities: [createTsonicPlugin()], sourceText: `
    import { createServer } from "node:http";
    import { Readable } from "node:stream";
    import { Buffer } from "node:buffer";
    export function run(): boolean {
      const server = createServer();
      const explicit = createServer(undefined);
      const source = Readable.from([Buffer.from("data")]);
      const chunk = source.read();
      source.destroy();
      source.destroy(undefined);
      return !server.listening && !explicit.listening && source.destroyed && chunk !== undefined && chunk.length === 4;
    }
  ` });
  executeCsharpConstruction(compiled, "native-optional-arguments", false, false, [
    fileURLToPath(new URL("../../../csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj", import.meta.url)),
  ]);
});
