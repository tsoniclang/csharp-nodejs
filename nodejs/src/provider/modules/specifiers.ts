import type { CsharpProviderModuleSpecifier } from "@tsonic/target-csharp/provider";
import {
  nodeAssertModuleSpecifier,
} from "./assert.js";
import {
  nodeBufferModuleSpecifier,
} from "./buffer/index.js";
import {
  nodeCryptoModuleSpecifier,
} from "./crypto.js";
import {
  nodeChildProcessModuleSpecifier,
} from "./child-process.js";
import {
  nodeFsModuleSpecifier,
  nodeFsPromisesModuleSpecifier,
} from "./filesystem/index.js";
import {
  nodeOsModuleSpecifier,
} from "./os.js";
import {
  nodeHttpModuleSpecifier,
} from "./http/index.js";
import {
  nodeHttpsModuleSpecifier,
} from "./https.js";
import {
  nodePathModuleSpecifier,
} from "./path/index.js";
import {
  nodeProcessModuleSpecifier,
} from "./process.js";
import {
  nodeTimersModuleSpecifier,
} from "./timers.js";
import {
  nodeUtilModuleSpecifier,
} from "./util/declarations.js";
import {
  nodeUrlModuleSpecifier,
} from "./url/index.js";
import {
  nodeEventsModuleSpecifier,
} from "./events.js";
import {
  nodeStreamModuleSpecifier,
} from "./stream.js";
import {
  nodeZlibModuleSpecifier,
} from "./zlib.js";
import {
  nodeDnsModuleSpecifier,
  nodeDnsPromisesModuleSpecifier,
} from "./dns.js";
import {
  nodeNetModuleSpecifier,
} from "./net.js";
import {
  nodeTlsModuleSpecifier,
} from "./tls.js";
import {
  nodeReadlineModuleSpecifier,
} from "./readline.js";
import {
  nodeWorkerThreadsModuleSpecifier,
} from "./worker-threads.js";

const canonicalBySpecifier = new Map<string, string>([
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
