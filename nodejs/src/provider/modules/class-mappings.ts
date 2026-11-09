import type { NodejsNativeClassApplicationRecipe } from "../native/locators.js";
import { nativeOwner } from "../native/authoring.js";
import { spawnOptionsApplication, spawnReturnsApplication } from "./child-process-native.js";

export interface NodejsNativeClassMapping {
  readonly moduleSpecifier: string;
  readonly exportName: string;
  application(includeJsSurface: boolean): NodejsNativeClassApplicationRecipe;
  readonly objectLiteralConstruction?: "object-initializer";
}

function classes(
  moduleSpecifier: string,
  entries: readonly (readonly [exportName: string, metadataName: string, construction?: "object-initializer"])[],
): readonly NodejsNativeClassMapping[] {
  return entries.map(([exportName, metadataName, objectLiteralConstruction]) => Object.freeze({
    moduleSpecifier, exportName,
    application: () => Object.freeze({ kind: "source-class" as const, original: nativeOwner(metadataName), parameters: Object.freeze([]) }),
    ...(objectLiteralConstruction === undefined ? {} : { objectLiteralConstruction }),
  }));
}

export const nodejsNativeClassMappings: readonly NodejsNativeClassMapping[] = Object.freeze([
  { moduleSpecifier: "node:child_process", exportName: "SpawnSyncReturns", application: () => spawnReturnsApplication },
  { moduleSpecifier: "node:child_process", exportName: "SpawnSyncOptionsWithBufferEncoding", application: spawnOptionsApplication, objectLiteralConstruction: "object-initializer" },
  ...classes("node:child_process", [["SpawnSyncError", "Tsonic.CSharp.Node.SpawnSyncError"]]),
  ...classes("node:events", [["EventEmitter", "Tsonic.CSharp.Node.EventEmitter"]]),
  ...classes("node:stream", [
    ["Stream", "Tsonic.CSharp.Node.Stream"], ["Readable", "Tsonic.CSharp.Node.Readable"],
    ["Writable", "Tsonic.CSharp.Node.Writable"], ["Duplex", "Tsonic.CSharp.Node.Duplex"], ["Transform", "Tsonic.CSharp.Node.Transform"],
  ]),
  ...classes("node:zlib", [["ZlibOptions", "Tsonic.CSharp.Node.ZlibOptions", "object-initializer"],
    ["BrotliOptions", "Tsonic.CSharp.Node.BrotliOptions", "object-initializer"], ["ZlibTransform", "Tsonic.CSharp.Node.ZlibTransform"]]),
  ...classes("node:dns", [["LookupOptions", "Tsonic.CSharp.Node.LookupOptions", "object-initializer"],
    ["LookupAllOptions", "Tsonic.CSharp.Node.LookupOptions", "object-initializer"], ["LookupAddress", "Tsonic.CSharp.Node.LookupAddress"],
    ["DnsPromises", "Tsonic.CSharp.Node.DnsPromises"]]),
  ...classes("node:net", [["Socket", "Tsonic.CSharp.Node.Socket"], ["Server", "Tsonic.CSharp.Node.Server"],
    ["ConnectOptions", "Tsonic.CSharp.Node.ConnectOptions", "object-initializer"], ["ListenOptions", "Tsonic.CSharp.Node.ListenOptions", "object-initializer"],
    ["ServerOptions", "Tsonic.CSharp.Node.ServerOpts", "object-initializer"]]),
  ...classes("node:tls", [["TLSSocket", "Tsonic.CSharp.Node.TLSSocket"], ["Server", "Tsonic.CSharp.Node.TLSServer"],
    ["SecureContext", "Tsonic.CSharp.Node.SecureContext"], ["ConnectionOptions", "Tsonic.CSharp.Node.ConnectionOptions", "object-initializer"],
    ["TlsOptions", "Tsonic.CSharp.Node.TlsOptions", "object-initializer"], ["SecureContextOptions", "Tsonic.CSharp.Node.SecureContextOptions", "object-initializer"]]),
  ...classes("node:readline", [["Interface", "Tsonic.CSharp.Node.Interface"], ["ReadLineOptions", "Tsonic.CSharp.Node.InterfaceOptions", "object-initializer"]]),
  ...classes("node:worker_threads", [["Worker", "Tsonic.CSharp.Node.Worker"], ["WorkerOptions", "Tsonic.CSharp.Node.WorkerOptions", "object-initializer"],
    ["MessagePort", "Tsonic.CSharp.Node.MessagePort"], ["MessageChannel", "Tsonic.CSharp.Node.MessageChannel"]]),
  ...classes("node:buffer", [["Buffer", "Tsonic.CSharp.Node.Buffer"]]),
  ...classes("node:crypto", [["Hash", "Tsonic.CSharp.Node.Hash"], ["Hmac", "Tsonic.CSharp.Node.Hmac"]]),
  ...classes("node:fs", [["Stats", "Tsonic.CSharp.Node.Stats"],
    ["MakeDirectoryOptions", "Tsonic.CSharp.Node.MakeDirectoryOptions", "object-initializer"], ["RmOptions", "Tsonic.CSharp.Node.RmOptions", "object-initializer"],
    ["WatchOptions", "Tsonic.CSharp.Node.WatchOptions", "object-initializer"], ["ReadStreamOptions", "Tsonic.CSharp.Node.ReadStreamOptions", "object-initializer"],
    ["WriteStreamOptions", "Tsonic.CSharp.Node.WriteStreamOptions", "object-initializer"], ["FsWatcher", "Tsonic.CSharp.Node.FsWatcher"],
    ["StatWatcher", "Tsonic.CSharp.Node.StatWatcher"], ["ReadStream", "Tsonic.CSharp.Node.ReadStream"], ["WriteStream", "Tsonic.CSharp.Node.WriteStream"]]),
  ...classes("node:http", [["IncomingMessage", "Tsonic.CSharp.Node.Http.IncomingMessage"], ["IncomingHttpHeaders", "Tsonic.CSharp.Node.Http.IncomingHttpHeaders"],
    ["IncomingHttpHeaderValues", "Tsonic.CSharp.Node.Http.IncomingHttpHeaders"], ["OutgoingHttpHeaders", "Tsonic.CSharp.Node.Http.OutgoingHttpHeaders"],
    ["AddressInfo", "Tsonic.CSharp.Node.Http.AddressInfo"], ["ServerAddress", "Tsonic.CSharp.Node.Http.ServerAddress"],
    ["ServerResponse", "Tsonic.CSharp.Node.Http.ServerResponse"], ["Server", "Tsonic.CSharp.Node.Http.Server"], ["ClientRequest", "Tsonic.CSharp.Node.Http.ClientRequest"],
    ["RequestOptions", "Tsonic.CSharp.Node.Http.RequestOptions", "object-initializer"]]),
  ...classes("node:https", [["Server", "Tsonic.CSharp.Node.Http.Server"], ["ClientRequest", "Tsonic.CSharp.Node.Http.ClientRequest"],
    ["ServerOptions", "Tsonic.CSharp.Node.Https.HttpsServerOptions", "object-initializer"], ["RequestOptions", "Tsonic.CSharp.Node.Https.HttpsRequestOptions", "object-initializer"]]),
  ...classes("node:path", [["ParsedPath", "Tsonic.CSharp.Node.ParsedPath"], ["PathModule", "Tsonic.CSharp.Node.PathModule"]]),
  ...classes("node:process", [["ProcessEnv", "Tsonic.CSharp.Node.ProcessEnv", "object-initializer"], ["MemoryUsage", "Tsonic.CSharp.Node.MemoryUsage"],
    ["CpuUsage", "Tsonic.CSharp.Node.CpuUsage", "object-initializer"], ["ProcessVersions", "Tsonic.CSharp.Node.ProcessVersions"]]),
  ...classes("node:v8", [["HeapInfo", "Tsonic.CSharp.Node.HeapInfo"]]),
  ...classes("node:timers", [["Timeout", "Tsonic.CSharp.Node.Timeout"]]),
  ...classes("node:util", [["TextDecoder", "Tsonic.CSharp.Node.TextDecoder"], ["NodeError", "System.Exception"]]),
  ...classes("node:url", [["URL", "Tsonic.CSharp.Node.URL"], ["URLSearchParams", "Tsonic.CSharp.Node.URLSearchParams"],
    ["UrlObject", "Tsonic.CSharp.Node.LegacyUrlObject"], ["Url", "Tsonic.CSharp.Node.LegacyUrlObject"], ["UrlWithStringQuery", "Tsonic.CSharp.Node.LegacyUrlObject"]]),
]);
