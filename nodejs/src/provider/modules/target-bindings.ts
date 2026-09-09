import {
  csharpQualifiedTypeRenderShape,
  csharpTargetNamedType,
} from "@tsonic/target-csharp/provider";
import type {
  CsharpTargetBindingFact,
} from "@tsonic/target-csharp/provider";

export interface NodejsProviderTargetTypeRow {
  readonly moduleSpecifier: string;
  readonly exportName: string;
  readonly kind: CsharpTargetBindingFact["kind"];
  readonly namespace: string;
  readonly targetName: string;
  readonly objectLiteralConstruction?: "object-initializer";
}

export const nodejsProviderTargetTypeRows: readonly NodejsProviderTargetTypeRow[] = Object.freeze([
  { moduleSpecifier: "node:events", exportName: "EventEmitter", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "EventEmitter" },
  { moduleSpecifier: "node:stream", exportName: "Stream", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Stream" },
  { moduleSpecifier: "node:stream", exportName: "Readable", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Readable" },
  { moduleSpecifier: "node:stream", exportName: "Writable", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Writable" },
  { moduleSpecifier: "node:stream", exportName: "Duplex", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Duplex" },
  { moduleSpecifier: "node:stream", exportName: "Transform", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Transform" },
  { moduleSpecifier: "node:zlib", exportName: "ZlibOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ZlibOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:zlib", exportName: "BrotliOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "BrotliOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:zlib", exportName: "ZlibTransform", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ZlibTransform" },
  { moduleSpecifier: "node:dns", exportName: "LookupOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "LookupOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:dns", exportName: "LookupAllOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "LookupOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:dns", exportName: "LookupAddress", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "LookupAddress" },
  { moduleSpecifier: "node:dns", exportName: "DnsPromises", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "DnsPromises" },
  { moduleSpecifier: "node:net", exportName: "Socket", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Socket" },
  { moduleSpecifier: "node:net", exportName: "Server", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Server" },
  { moduleSpecifier: "node:net", exportName: "ConnectOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ConnectOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:net", exportName: "ListenOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ListenOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:net", exportName: "ServerOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ServerOpts", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:tls", exportName: "TLSSocket", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "TLSSocket" },
  { moduleSpecifier: "node:tls", exportName: "Server", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "TLSServer" },
  { moduleSpecifier: "node:tls", exportName: "SecureContext", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "SecureContext" },
  { moduleSpecifier: "node:tls", exportName: "ConnectionOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ConnectionOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:tls", exportName: "TlsOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "TlsOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:tls", exportName: "SecureContextOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "SecureContextOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:readline", exportName: "Interface", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Interface" },
  { moduleSpecifier: "node:readline", exportName: "ReadLineOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "InterfaceOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:worker_threads", exportName: "Worker", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Worker" },
  { moduleSpecifier: "node:worker_threads", exportName: "WorkerOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "WorkerOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:worker_threads", exportName: "MessagePort", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "MessagePort" },
  { moduleSpecifier: "node:worker_threads", exportName: "MessageChannel", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "MessageChannel" },
  { moduleSpecifier: "node:buffer", exportName: "Buffer", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Buffer" },
  { moduleSpecifier: "node:crypto", exportName: "Hash", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Hash" },
  { moduleSpecifier: "node:crypto", exportName: "Hmac", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Hmac" },
  { moduleSpecifier: "node:fs", exportName: "Stats", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Stats" },
  { moduleSpecifier: "node:fs", exportName: "MakeDirectoryOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "MakeDirectoryOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:fs", exportName: "RmOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "RmOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:fs", exportName: "WatchOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "WatchOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:fs", exportName: "ReadStreamOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ReadStreamOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:fs", exportName: "WriteStreamOptions", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "WriteStreamOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:fs", exportName: "FsWatcher", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "FsWatcher" },
  { moduleSpecifier: "node:fs", exportName: "StatWatcher", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "StatWatcher" },
  { moduleSpecifier: "node:fs", exportName: "ReadStream", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "ReadStream" },
  { moduleSpecifier: "node:fs", exportName: "WriteStream", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "WriteStream" },
  { moduleSpecifier: "node:http", exportName: "IncomingMessage", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "IncomingMessage" },
  { moduleSpecifier: "node:http", exportName: "ServerResponse", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "ServerResponse" },
  { moduleSpecifier: "node:http", exportName: "Server", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "Server" },
  { moduleSpecifier: "node:http", exportName: "ClientRequest", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "ClientRequest" },
  { moduleSpecifier: "node:http", exportName: "RequestOptions", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "RequestOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:https", exportName: "Server", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "Server" },
  { moduleSpecifier: "node:https", exportName: "ClientRequest", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "ClientRequest" },
  { moduleSpecifier: "node:https", exportName: "ServerOptions", kind: "class", namespace: "Tsonic.CSharp.Node.Https", targetName: "HttpsServerOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:https", exportName: "RequestOptions", kind: "class", namespace: "Tsonic.CSharp.Node.Https", targetName: "HttpsRequestOptions", objectLiteralConstruction: "object-initializer" },
  { moduleSpecifier: "node:path", exportName: "ParsedPath", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "ParsedPath" },
  { moduleSpecifier: "node:path", exportName: "PathModule", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "PathModule" },
  { moduleSpecifier: "node:process", exportName: "ProcessEnv", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "ProcessEnv" },
  { moduleSpecifier: "node:process", exportName: "MemoryUsage", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "MemoryUsage" },
  { moduleSpecifier: "node:process", exportName: "ProcessVersions", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "ProcessVersions" },
  { moduleSpecifier: "node:timers", exportName: "Timeout", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Timeout" },
  { moduleSpecifier: "node:util", exportName: "TextDecoder", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "TextDecoder" },
  { moduleSpecifier: "node:url", exportName: "URL", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "URL" },
  { moduleSpecifier: "node:url", exportName: "URLSearchParams", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "URLSearchParams" },
  { moduleSpecifier: "node:url", exportName: "UrlObject", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "LegacyUrlObject" },
  { moduleSpecifier: "node:url", exportName: "Url", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "LegacyUrlObject" },
  { moduleSpecifier: "node:url", exportName: "UrlWithStringQuery", kind: "interface", namespace: "Tsonic.CSharp.Node", targetName: "LegacyUrlObject" },
]);

const bindingByProviderExport = new Map(
  nodejsProviderTargetTypeRows.map((row) => {
    const targetId = `${row.namespace}.${row.targetName}`;
    const binding = Object.freeze({
      id: targetId,
      sourceName: row.targetName,
      targetName: targetId,
      target: "csharp" as const,
      kind: row.kind,
      csharpType: csharpTargetNamedType(
        targetId,
        undefined,
        csharpQualifiedTypeRenderShape(row.namespace, row.targetName),
      ),
    }) satisfies CsharpTargetBindingFact;
    return [providerExportKey(row.moduleSpecifier, row.exportName), binding] as const;
  }),
);

export function nodejsProviderTargetBinding(
  moduleSpecifier: string,
  exportName: string,
): CsharpTargetBindingFact | undefined {
  return bindingByProviderExport.get(
    providerExportKey(moduleSpecifier, exportName),
  );
}

function providerExportKey(moduleSpecifier: string, exportName: string): string {
  return `${moduleSpecifier}\u0000${exportName}`;
}
