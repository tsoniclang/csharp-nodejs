import type { ProviderExportDeclaration } from "@tsonic/tsts";
import { nodeV8Exports, nodeV8ModuleSpecifier } from "../modules/v8.js";
import type { CsharpProviderModuleDefinition } from "@tsonic/target-csharp/provider";
import {
  nodeAssertExports,
  nodeAssertModuleSpecifier,
} from "../modules/assert.js";
import {
  nodeBufferExports,
  nodeBufferModuleSpecifier,
} from "../modules/buffer/index.js";
import {
  nodeCryptoExports,
  nodeCryptoModuleSpecifier,
} from "../modules/crypto.js";
import {
  nodeChildProcessExports,
  nodeChildProcessModuleSpecifier,
} from "../modules/child-process.js";
import {
  nodeFsExports,
  nodeFsModuleSpecifier,
  nodeFsPromisesExports,
  nodeFsPromisesModuleSpecifier,
} from "../modules/filesystem/index.js";
import {
  nodeHttpExports,
  nodeHttpModuleSpecifier,
} from "../modules/http/index.js";
import {
  nodeHttpsExports,
  nodeHttpsModuleSpecifier,
} from "../modules/https.js";
import {
  nodeEventsExports,
  nodeEventsModuleSpecifier,
} from "../modules/events.js";
import {
  nodeStreamExports,
  nodeStreamModuleSpecifier,
} from "../modules/stream.js";
import {
  nodeZlibExports,
  nodeZlibModuleSpecifier,
} from "../modules/zlib.js";
import {
  nodeDnsExports,
  nodeDnsModuleSpecifier,
  nodeDnsPromisesExports,
  nodeDnsPromisesModuleSpecifier,
} from "../modules/dns.js";
import {
  nodeNetExports,
  nodeNetModuleSpecifier,
} from "../modules/net.js";
import {
  nodeTlsExports,
  nodeTlsModuleSpecifier,
} from "../modules/tls.js";
import {
  nodeReadlineExports,
  nodeReadlineModuleSpecifier,
} from "../modules/readline.js";
import {
  nodeWorkerThreadsExports,
  nodeWorkerThreadsModuleSpecifier,
} from "../modules/worker-threads.js";
import {
  nodeOsExports,
  nodeOsModuleSpecifier,
} from "../modules/os.js";
import {
  nodePathExports,
  nodePathModuleSpecifier,
} from "../modules/path/index.js";
import {
  nodeProcessExports,
  nodeProcessModuleSpecifier,
} from "../modules/process/declarations.js";
import {
  nodeTimersExports,
  nodeTimersModuleSpecifier,
} from "../modules/timers.js";
import {
  nodeUtilExports,
  nodeUtilModuleSpecifier,
} from "../modules/util/declarations.js";
import {
  nodeUrlExports,
  nodeUrlModuleSpecifier,
} from "../modules/url/index.js";

const canonicalModules = new Map<string, readonly ProviderExportDeclaration[]>([
  [nodeAssertModuleSpecifier, nodeAssertExports()],
  [nodeBufferModuleSpecifier, nodeBufferExports()],
  [nodeChildProcessModuleSpecifier, nodeChildProcessExports()],
  [nodePathModuleSpecifier, nodePathExports()],
  [nodeFsModuleSpecifier, nodeFsExports({ includeJsSurfaceMembers: true })],
  [nodeFsPromisesModuleSpecifier, nodeFsPromisesExports()],
  [nodeHttpModuleSpecifier, nodeHttpExports()],
  [nodeHttpsModuleSpecifier, nodeHttpsExports()],
  [nodeEventsModuleSpecifier, nodeEventsExports()],
  [nodeStreamModuleSpecifier, nodeStreamExports()],
  [nodeZlibModuleSpecifier, nodeZlibExports()],
  [nodeDnsModuleSpecifier, nodeDnsExports()],
  [nodeDnsPromisesModuleSpecifier, nodeDnsPromisesExports()],
  [nodeNetModuleSpecifier, nodeNetExports()],
  [nodeTlsModuleSpecifier, nodeTlsExports()],
  [nodeReadlineModuleSpecifier, nodeReadlineExports()],
  [nodeWorkerThreadsModuleSpecifier, nodeWorkerThreadsExports()],
  [nodeCryptoModuleSpecifier, nodeCryptoExports()],
  [nodeOsModuleSpecifier, nodeOsExports()],
  [nodeV8ModuleSpecifier, nodeV8Exports()],
  [nodeProcessModuleSpecifier, nodeProcessExports()],
  [nodeTimersModuleSpecifier, nodeTimersExports()],
  [nodeUtilModuleSpecifier, nodeUtilExports()],
  [nodeUrlModuleSpecifier, nodeUrlExports()],
]);

export function nodejsCanonicalProviderExports(
  moduleSpecifier: string,
  includeJsSurfaceMembers = true,
): readonly ProviderExportDeclaration[] | undefined {
  if (moduleSpecifier === nodeFsModuleSpecifier) return nodeFsExports({ includeJsSurfaceMembers });
  if (moduleSpecifier === nodeChildProcessModuleSpecifier) return nodeChildProcessExports(includeJsSurfaceMembers);
  return canonicalModules.get(moduleSpecifier);
}

export const nodejsProviderModules: readonly CsharpProviderModuleDefinition[] =
  [...canonicalModules.keys()].map((moduleSpecifier): CsharpProviderModuleDefinition => ({
    moduleSpecifier,
    providerModuleId: moduleSpecifier,
    getExports(selectedSurfaceIds) {
      return nodejsCanonicalProviderExports(moduleSpecifier, selectedSurfaceIds.includes("js"))!;
    },
  }));
