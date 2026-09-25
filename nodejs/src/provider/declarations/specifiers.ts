import type { CsharpProviderModuleSpecifier } from "@tsonic/target-csharp/provider";
import { nodeV8ModuleSpecifier } from "../modules/v8.js";
import {
  nodeAssertModuleSpecifier,
} from "../modules/assert.js";
import {
  nodeBufferModuleSpecifier,
} from "../modules/buffer/index.js";
import {
  nodeCryptoModuleSpecifier,
} from "../modules/crypto.js";
import {
  nodeChildProcessModuleSpecifier,
} from "../modules/child-process.js";
import {
  nodeFsModuleSpecifier,
  nodeFsPromisesModuleSpecifier,
} from "../modules/filesystem/index.js";
import {
  nodeOsModuleSpecifier,
} from "../modules/os.js";
import {
  nodeHttpModuleSpecifier,
} from "../modules/http/index.js";
import {
  nodeHttpsModuleSpecifier,
} from "../modules/https.js";
import {
  nodePathModuleSpecifier,
} from "../modules/path/index.js";
import {
  nodeProcessModuleSpecifier,
} from "../modules/process/declarations.js";
import {
  nodeTimersModuleSpecifier,
} from "../modules/timers.js";
import {
  nodeUtilModuleSpecifier,
} from "../modules/util/declarations.js";
import {
  nodeUrlModuleSpecifier,
} from "../modules/url/index.js";
import {
  nodeEventsModuleSpecifier,
} from "../modules/events.js";
import {
  nodeStreamModuleSpecifier,
} from "../modules/stream.js";
import {
  nodeZlibModuleSpecifier,
} from "../modules/zlib.js";
import {
  nodeDnsModuleSpecifier,
  nodeDnsPromisesModuleSpecifier,
} from "../modules/dns.js";
import {
  nodeNetModuleSpecifier,
} from "../modules/net.js";
import {
  nodeTlsModuleSpecifier,
} from "../modules/tls.js";
import {
  nodeReadlineModuleSpecifier,
} from "../modules/readline.js";
import {
  nodeWorkerThreadsModuleSpecifier,
} from "../modules/worker-threads.js";

const canonicalBySpecifier = new Map<string, string>([
  ["v8", nodeV8ModuleSpecifier],
  [nodeV8ModuleSpecifier, nodeV8ModuleSpecifier],
  ["assert", nodeAssertModuleSpecifier],
  [nodeAssertModuleSpecifier, nodeAssertModuleSpecifier],
  ["assert/strict", nodeAssertModuleSpecifier],
  ["node:assert/strict", nodeAssertModuleSpecifier],
  ["buffer", nodeBufferModuleSpecifier],
  [nodeBufferModuleSpecifier, nodeBufferModuleSpecifier],
  ["crypto", nodeCryptoModuleSpecifier],
  [nodeCryptoModuleSpecifier, nodeCryptoModuleSpecifier],
  ["child_process", nodeChildProcessModuleSpecifier],
  [nodeChildProcessModuleSpecifier, nodeChildProcessModuleSpecifier],
  ["fs", nodeFsModuleSpecifier],
  [nodeFsModuleSpecifier, nodeFsModuleSpecifier],
  ["fs/promises", nodeFsPromisesModuleSpecifier],
  [nodeFsPromisesModuleSpecifier, nodeFsPromisesModuleSpecifier],
  ["http", nodeHttpModuleSpecifier],
  [nodeHttpModuleSpecifier, nodeHttpModuleSpecifier],
  ["https", nodeHttpsModuleSpecifier],
  [nodeHttpsModuleSpecifier, nodeHttpsModuleSpecifier],
  ["events", nodeEventsModuleSpecifier],
  [nodeEventsModuleSpecifier, nodeEventsModuleSpecifier],
  ["stream", nodeStreamModuleSpecifier],
  [nodeStreamModuleSpecifier, nodeStreamModuleSpecifier],
  ["zlib", nodeZlibModuleSpecifier],
  [nodeZlibModuleSpecifier, nodeZlibModuleSpecifier],
  ["dns", nodeDnsModuleSpecifier],
  [nodeDnsModuleSpecifier, nodeDnsModuleSpecifier],
  ["dns/promises", nodeDnsPromisesModuleSpecifier],
  [nodeDnsPromisesModuleSpecifier, nodeDnsPromisesModuleSpecifier],
  ["net", nodeNetModuleSpecifier],
  [nodeNetModuleSpecifier, nodeNetModuleSpecifier],
  ["tls", nodeTlsModuleSpecifier],
  [nodeTlsModuleSpecifier, nodeTlsModuleSpecifier],
  ["readline", nodeReadlineModuleSpecifier],
  [nodeReadlineModuleSpecifier, nodeReadlineModuleSpecifier],
  ["worker_threads", nodeWorkerThreadsModuleSpecifier],
  [nodeWorkerThreadsModuleSpecifier, nodeWorkerThreadsModuleSpecifier],
  ["os", nodeOsModuleSpecifier],
  [nodeOsModuleSpecifier, nodeOsModuleSpecifier],
  ["path", nodePathModuleSpecifier],
  [nodePathModuleSpecifier, nodePathModuleSpecifier],
  ["process", nodeProcessModuleSpecifier],
  [nodeProcessModuleSpecifier, nodeProcessModuleSpecifier],
  ["timers", nodeTimersModuleSpecifier],
  [nodeTimersModuleSpecifier, nodeTimersModuleSpecifier],
  ["util", nodeUtilModuleSpecifier],
  [nodeUtilModuleSpecifier, nodeUtilModuleSpecifier],
  ["url", nodeUrlModuleSpecifier],
  [nodeUrlModuleSpecifier, nodeUrlModuleSpecifier],
]);

export function nodejsProviderPackageOwnedModuleSpecifiers(): readonly string[] {
  return Array.from(canonicalBySpecifier.keys());
}

export function nodejsProviderModuleSpecifiers(): readonly CsharpProviderModuleSpecifier[] {
  return [...canonicalBySpecifier].map(([moduleSpecifier, canonicalModuleSpecifier]) => ({
    moduleSpecifier,
    canonicalModuleSpecifier,
    message:
      `target 'csharp' capability '@tsonic/csharp-nodejs' must be installed to import Node.js built-in provider module '${moduleSpecifier}'`,
  }));
}

export function nodejsPublicModuleSpecifiers(
  canonicalModuleSpecifier: string,
): readonly string[] {
  return Object.freeze(
    [...canonicalBySpecifier.entries()]
      .filter(([, canonical]) => canonical === canonicalModuleSpecifier)
      .map(([specifier]) => specifier)
      .sort(),
  );
}

export function canonicalNodejsModuleSpecifier(specifier: string | undefined): string | undefined {
  return specifier === undefined ? undefined : canonicalBySpecifier.get(specifier);
}

export function isSupportedNodejsModuleSpecifier(specifier: string | undefined): boolean {
  return canonicalNodejsModuleSpecifier(specifier) !== undefined;
}
