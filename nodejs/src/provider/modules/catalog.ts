import type { ProviderExportDeclaration } from "@tsonic/tsts";
import type { CsharpProviderModuleDefinition } from "@tsonic/target-csharp/provider";
import {
  nodeAssertExports,
  nodeAssertModuleSpecifier,
} from "./assert.js";
import {
  nodeBufferExports,
  nodeBufferModuleSpecifier,
} from "./buffer/index.js";
import {
  nodeCryptoExports,
  nodeCryptoModuleSpecifier,
} from "./crypto.js";
import {
  nodeChildProcessExports,
  nodeChildProcessModuleSpecifier,
} from "./child-process.js";
import {
  nodeFsExports,
  nodeFsModuleSpecifier,
  nodeFsPromisesExports,
  nodeFsPromisesModuleSpecifier,
} from "./filesystem/index.js";
import {
  nodeHttpExports,
  nodeHttpModuleSpecifier,
} from "./http/index.js";
import {
  nodeHttpsExports,
  nodeHttpsModuleSpecifier,
} from "./https.js";
import {
  nodeEventsExports,
  nodeEventsModuleSpecifier,
} from "./events.js";
import {
  nodeStreamExports,
  nodeStreamModuleSpecifier,
} from "./stream.js";
import {
  nodeZlibExports,
  nodeZlibModuleSpecifier,
} from "./zlib.js";
import {
  nodeDnsExports,
  nodeDnsModuleSpecifier,
  nodeDnsPromisesExports,
  nodeDnsPromisesModuleSpecifier,
} from "./dns.js";
import {
  nodeNetExports,
  nodeNetModuleSpecifier,
} from "./net.js";
import {
  nodeTlsExports,
  nodeTlsModuleSpecifier,
} from "./tls.js";
import {
  nodeReadlineExports,
  nodeReadlineModuleSpecifier,
} from "./readline.js";
import {
  nodeWorkerThreadsExports,
  nodeWorkerThreadsModuleSpecifier,
} from "./worker-threads.js";
import {
  nodeOsExports,
  nodeOsModuleSpecifier,
} from "./os.js";
import {
  nodePathExports,
  nodePathModuleSpecifier,
} from "./path/index.js";
import {
  nodeProcessExports,
  nodeProcessModuleSpecifier,
} from "./process.js";
import {
  nodeTimersExports,
  nodeTimersModuleSpecifier,
} from "./timers.js";
import {
  nodeUtilExports,
  nodeUtilModuleSpecifier,
} from "./util/declarations.js";
import {
  nodeUrlExports,
  nodeUrlModuleSpecifier,
} from "./url/index.js";

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
  [nodeProcessModuleSpecifier, nodeProcessExports()],
  [nodeTimersModuleSpecifier, nodeTimersExports()],
  [nodeUtilModuleSpecifier, nodeUtilExports()],
  [nodeUrlModuleSpecifier, nodeUrlExports()],
]);

export function nodejsCanonicalProviderExports(
  moduleSpecifier: string,
): readonly ProviderExportDeclaration[] | undefined {
  return canonicalModules.get(moduleSpecifier);
}

export const nodejsProviderModules: readonly CsharpProviderModuleDefinition[] =
  [...canonicalModules].map(([moduleSpecifier, exports]): CsharpProviderModuleDefinition => ({
    moduleSpecifier,
    providerModuleId: moduleSpecifier,
    getExports(selectedSurfaceIds) {
      return moduleSpecifier === nodeFsModuleSpecifier
        ? nodeFsExports({ includeJsSurfaceMembers: selectedSurfaceIds.includes("js") })
        : exports;
    },
  }));
