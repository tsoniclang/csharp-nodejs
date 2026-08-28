import assert from "node:assert/strict";
import test from "node:test";

import {
  assertCsharpCheckingSucceeded,
  assertCsharpCompilationSucceeded,
  compileCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import {
  createTsonicPlugin,
} from "../dist/index.js";
import {
  nodejsProviderPackageOwnedModuleSpecifiers,
} from "../dist/provider/module-specifiers.js";
import {
  nodejsCanonicalProviderExports,
} from "../dist/provider/provider.js";
import {
  nodejsProviderTargetRejections,
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";

test("every canonical Node source operation has exact target policy", () => {
  const sourceSignatures = new Set();
  const sourceProperties = new Set();
  const canonicalModules = nodejsProviderPackageOwnedModuleSpecifiers()
    .filter((specifier) => specifier.startsWith("node:"));

  for (const moduleSpecifier of canonicalModules) {
    for (
      const declaration of
        nodejsCanonicalProviderExports(moduleSpecifier) ?? []
    ) {
      for (const signature of declaration.signatures ?? []) {
        sourceSignatures.add(signature.id);
      }
      if (declaration.kind === "value") {
        sourceProperties.add(declaration.id);
      }
      for (const member of declaration.members ?? []) {
        for (const signature of member.signatures ?? []) {
          sourceSignatures.add(signature.id);
        }
        if (member.kind === "property") {
          sourceProperties.add(member.id);
        }
      }
    }
  }

  const policySignatures = new Set();
  const policyProperties = new Set();
  for (
    const policy of [
      ...nodejsProviderTargetRelations(),
      ...nodejsProviderTargetRejections(),
    ]
  ) {
    const source = policy.source;
    if (source.kind === "signature") {
      policySignatures.add(source.signatureId);
    } else if (source.kind === "member") {
      policyProperties.add(source.memberId);
    } else if (source.kind === "value") {
      policyProperties.add(source.exportId);
    }
  }

  assert.equal(sourceSignatures.size, 271);
  assert.equal(sourceProperties.size, 123);
  assert.deepEqual([...policySignatures].sort(), [...sourceSignatures].sort());
  assert.deepEqual([...policyProperties].sort(), [...sourceProperties].sort());
});

test("Node Promise declarations and targets preserve separate source and target shapes", () => {
  const declarations = nodejsCanonicalProviderExports("node:http") ?? [];
  const incoming = declarations.find((declaration) =>
    declaration.name === "IncomingMessage"
  );
  const readAll = incoming?.members
    ?.find((member) => member.name === "readAll")
    ?.signatures?.find((signature) =>
      signature.id === "node:http.IncomingMessage.readAll()"
    );
  assert.deepEqual(readAll?.returnType, {
    kind: "source-global",
    name: "Promise",
    typeArguments: [{ kind: "string" }],
  });

  const target = nodejsProviderTargetRelations().filter((relation) =>
    relation.kind === "signature" &&
    relation.source.moduleSpecifier === "node:http" &&
    relation.source.signatureId ===
      "node:http.IncomingMessage.readAll()"
  );
  assert.equal(target.length, 1);
  assert.equal(
    target[0].targetMember.returnType.id,
    "System.Threading.Tasks.Task`1",
  );
  assert.equal(
    target[0].targetMember.returnType.typeArguments[0].id,
    "System.String",
  );
});

test("Node process declarations expose exact undefined source semantics", () => {
  const declarations = nodejsCanonicalProviderExports("node:process") ?? [];
  const processEnv = declarations.find((declaration) =>
    declaration.name === "ProcessEnv"
  );
  const indexer = processEnv?.members?.find((member) =>
    member.kind === "indexer"
  );
  assert.deepEqual(indexer?.signatures?.[0]?.returnType, {
    kind: "union",
    types: [{ kind: "string" }, { kind: "undefined" }],
  });
});

test("Node provider families compile together through selected source evidence", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import fsPromises from "node:fs/promises";
      import { mkdtempSync } from "node:fs";
      import { Buffer } from "node:buffer";
      import process from "node:process";
      import { URLSearchParams } from "node:url";

      export async function run(
        path: string,
        key: string,
      ): Promise<string> {
        const bytes = Buffer.from("x", "utf8");
        const tempDir = mkdtempSync(path + "-");
        await fsPromises.writeFile(path, bytes);
        const text = await fsPromises.readFile(path, "utf8");
        const params = new URLSearchParams("a=1");
        params.append("b", "2");
        return (process.env[key] ?? text)
          + ":" + params.size + ":" + bytes.length + ":" + tempDir;
      }
    `,
  });

  assertCsharpCompilationSucceeded(compiled);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(source, /Tsonic\.CSharp\.Node\.Buffer\.from/u);
  assert.match(
    source,
    /await Tsonic\.CSharp\.Node\.fs_promises\.readFile/u,
  );
  assert.match(
    source,
    /Tsonic\.CSharp\.Node\.process\.env\[key\] \?\? text/u,
  );
  assert.match(source, /Tsonic\.CSharp\.Node\.fs\.mkdtempSync/u);
  assert.match(source, /new Tsonic\.CSharp\.Node\.URLSearchParams/u);
});

test("required Node capability families compile together through exact provider evidence", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import { Buffer } from "node:buffer";
      import { lookup } from "node:dns";
      import type { EventEmitter } from "node:events";
      import { createServer as createHttpsServer, request, type ServerOptions as HttpsServerOptions } from "node:https";
      import { createConnection, type Socket } from "node:net";
      import { createInterface, type ReadLineOptions } from "node:readline";
      import type { Readable, Writable } from "node:stream";
      import { connect, createServer as createTlsServer, type ConnectionOptions, type TlsOptions } from "node:tls";
      import {
        isMarkedAsUntransferable,
        markAsUntransferable,
        MessageChannel,
        Worker,
      } from "node:worker_threads";
      import { gzipSync } from "node:zlib";

      export function portableFamilies(
        emitter: EventEmitter,
        readable: Readable,
        writable: Writable,
        bytes: Buffer,
        lines: ReadLineOptions,
        host: string,
        port: number,
      ): Socket {
        const listener = (value: any): void => {
          JSON.stringify(value);
        };
        emitter.on("data", listener);
        emitter.emit("data", bytes);
        emitter.off("data", listener);
        readable.pause().resume().pipe(writable);
        writable.write(bytes);
        writable.end("done");
        lookup(host, (_error: any, address: string, family: number): void => {
          JSON.stringify({ address, family });
        });
        gzipSync(bytes);
        const input = createInterface(lines);
        input.setPrompt("> ");
        input.question("value? ", (answer: string): void => {
          JSON.stringify(answer);
        });
        input.pause().resume();
        input.close();
        return createConnection(port, host);
      }

      export function secureFamilies(
        connection: ConnectionOptions,
        tlsOptions: TlsOptions,
        httpsOptions: HttpsServerOptions,
        port: number,
        host: string,
      ): void {
        const socket = connect(connection, (): void => {});
        socket.write("ping");
        socket.end();
        const tlsServer = createTlsServer(tlsOptions, (peer): void => {
          peer.write("ready");
          peer.end();
        });
        tlsServer.listen(port, host, (): void => {});
        tlsServer.unref();
        tlsServer.close();
        const httpsServer = createHttpsServer(httpsOptions, (_incoming, response): void => {
          response.end("ok");
        });
        httpsServer.listen(port, host, (): void => {});
        httpsServer.unref();
        httpsServer.close();
        const outgoing = request("https://example.test/", (response): void => {
          response.readAll();
        });
        outgoing.write("body");
        outgoing.end();
      }

      export function workerFamilies(): void {
        const worker = new Worker("./worker.js", { name: "proof" });
        worker.on("message", (value: any): void => {
          JSON.stringify(value);
        });
        worker.postMessage({ ready: true });
        worker.ref().unref();
        worker.terminate();
        const channel = new MessageChannel();
        channel.port1.start();
        channel.port1.postMessage("message");
        channel.port1.unref();
        channel.port1.ref();
        channel.port1.close();
        const exact = { value: 1 };
        markAsUntransferable(exact);
        isMarkedAsUntransferable(exact);
      }
    `,
    files: {
      "worker.ts": `
        import { parentPort, workerData } from "node:worker_threads";
        if (parentPort !== undefined) {
          parentPort.postMessage(workerData);
        }
      `,
    },
  });

  assertCsharpCompilationSucceeded(compiled);
  const source = [...compiled.artifacts.values()].join("\n");
  assert.match(source, /Tsonic\.CSharp\.Node\.dns\.lookup/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.zlib\.gzipSync/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.tls\.connect/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.Https\.https\.createServer/u);
  assert.match(source, /new Tsonic\.CSharp\.Node\.Worker/u);
  assert.match(source, /InitializeWorkerProcess/u);
});

test("unsupported selected Node operations fail closed without artifacts", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import { format } from "node:util";
      export function invalid(): string {
        return format({ value: 1 });
      }
    `,
  });

  assertCsharpCheckingSucceeded(compiled);
  assert.deepEqual(
    compiled.result.diagnostics.map((diagnostic) => diagnostic.code),
    ["TS9100203"],
  );
  assert.match(
    compiled.result.diagnostics[0].message,
    /hard-rejected selected call 'node:util' export 'format'/u,
  );
  assert.equal(compiled.artifacts.size, 0);
});
