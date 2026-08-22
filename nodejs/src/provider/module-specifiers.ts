import {
  nodeAssertModuleSpecifier,
} from "./assert.js";
import {
  nodeBufferModuleSpecifier,
} from "./buffer.js";
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
} from "./http.js";
import {
  nodePathModuleSpecifier,
} from "./path.js";
import {
  nodeProcessModuleSpecifier,
} from "./process.js";
import {
  nodeTimersModuleSpecifier,
} from "./timers.js";
import {
  nodeUtilModuleSpecifier,
} from "./util.js";
import {
  nodeUrlModuleSpecifier,
} from "./url.js";

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
