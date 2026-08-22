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
}

export const nodejsProviderTargetTypeRows: readonly NodejsProviderTargetTypeRow[] = Object.freeze([
  { moduleSpecifier: "node:buffer", exportName: "Buffer", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Buffer" },
  { moduleSpecifier: "node:crypto", exportName: "Hash", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Hash" },
  { moduleSpecifier: "node:crypto", exportName: "Hmac", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Hmac" },
  { moduleSpecifier: "node:fs", exportName: "Stats", kind: "class", namespace: "Tsonic.CSharp.Node", targetName: "Stats" },
  { moduleSpecifier: "node:http", exportName: "IncomingMessage", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "IncomingMessage" },
  { moduleSpecifier: "node:http", exportName: "ServerResponse", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "ServerResponse" },
  { moduleSpecifier: "node:http", exportName: "Server", kind: "class", namespace: "Tsonic.CSharp.Node.Http", targetName: "Server" },
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
