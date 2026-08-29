import type {
  ProviderExportDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpNullableTargetType,
  csharpNullableValueTargetType,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpTsValueTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  arrayProviderType,
  booleanProviderType,
  callbackProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  providerRef,
  stringProviderType,
  unionProviderType,
  voidProviderType,
} from "./capability-module.js";
import {
  nodeBufferProviderType,
} from "./buffer/provider-types.js";
import {
  nodeHttpModuleSpecifier,
} from "./http.js";
import {
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";

export const nodeHttpsModuleSpecifier = "node:https";

const targetTypes = {
  Server: nodejsTargetNamedType("Tsonic.CSharp.Node.Http", "Server"),
  ClientRequest: nodejsTargetNamedType("Tsonic.CSharp.Node.Http", "ClientRequest"),
  IncomingMessage: nodejsTargetNamedType("Tsonic.CSharp.Node.Http", "IncomingMessage"),
  ServerResponse: nodejsTargetNamedType("Tsonic.CSharp.Node.Http", "ServerResponse"),
  ServerOptions: nodejsTargetNamedType("Tsonic.CSharp.Node.Https", "HttpsServerOptions"),
  RequestOptions: nodejsTargetNamedType("Tsonic.CSharp.Node.Https", "HttpsRequestOptions"),
};
const httpsTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node.Https", "https");
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const nullableBoolTargetType = csharpNullableValueTargetType(boolTargetType);
const tsValueTargetType = csharpTsValueTargetType();
const serverProviderType = providerRef(nodeHttpsModuleSpecifier, "Server");
const clientRequestProviderType = providerRef(nodeHttpsModuleSpecifier, "ClientRequest");
const serverOptionsProviderType = providerRef(nodeHttpsModuleSpecifier, "ServerOptions");
const requestOptionsProviderType = providerRef(nodeHttpsModuleSpecifier, "RequestOptions");
const incomingMessageProviderType = providerRef(nodeHttpModuleSpecifier, "IncomingMessage");
const serverResponseProviderType = providerRef(nodeHttpModuleSpecifier, "ServerResponse");
const requestListenerProviderType = callbackProviderType("node:https.request-listener", [
  { name: "request", type: incomingMessageProviderType },
  { name: "response", type: serverResponseProviderType },
]);
const responseListenerProviderType = callbackProviderType("node:https.response-listener", [
  { name: "response", type: incomingMessageProviderType },
]);
const requestListenerTargetType = csharpDelegateTargetType(
  "System.Action",
  [targetTypes.IncomingMessage, targetTypes.ServerResponse],
);
const responseListenerTargetType = csharpDelegateTargetType(
  "System.Action",
  [targetTypes.IncomingMessage],
);
const keyMaterialProviderType = unionProviderType(
  stringProviderType,
  nodeBufferProviderType,
);
const caProviderType = unionProviderType(
  stringProviderType,
  nodeBufferProviderType,
  arrayProviderType(stringProviderType),
);

export function nodeHttpsExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeHttpsModuleSpecifier,
    moduleCalls: nodeHttpsCallTargetMembers(),
    classProperties: nodeHttpsClassPropertyTargetMembers(),
    classes: ["Server", "ClientRequest"],
    classHeritage: {
      Server: [providerRef(nodeHttpModuleSpecifier, "Server")],
      ClientRequest: [providerRef(nodeHttpModuleSpecifier, "ClientRequest")],
    },
    additionalExports: [
      optionDeclaration("ServerOptions", providerRef("node:tls", "TlsOptions")),
      optionDeclaration("RequestOptions", providerRef(nodeHttpModuleSpecifier, "RequestOptions")),
    ],
  });
}

export function nodeHttpsCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  const requestRows = (["request", "get"] as const).flatMap((exportName) => [
    nodejsModuleCallTargetMetadata({
      exportName,
      signatureId: `node:https.${exportName}(System.String,System.Action\`1)`,
      targetMemberId: `Tsonic.CSharp.Node.Https.https.${exportName}(System.String,System.Action\`1)`,
      sourceName: exportName,
      targetName: exportName,
      providerParameters: [
        { name: "url", type: stringProviderType },
        { name: "callback", type: responseListenerProviderType, optional: true },
      ],
      providerReturnType: clientRequestProviderType,
      targetParameters: [
        targetParameter("url", stringTargetType),
        targetParameter("callback", responseListenerTargetType, { optional: true }),
      ],
      targetReturnType: targetTypes.ClientRequest,
      declaringType: httpsTargetType,
    }),
    nodejsModuleCallTargetMetadata({
      exportName,
      signatureId: `node:https.${exportName}(HttpsRequestOptions,System.Action\`1)`,
      targetMemberId: `Tsonic.CSharp.Node.Https.https.${exportName}(Tsonic.CSharp.Node.Https.HttpsRequestOptions,System.Action\`1)`,
      sourceName: exportName,
      targetName: exportName,
      providerParameters: [
        { name: "options", type: requestOptionsProviderType },
        { name: "callback", type: responseListenerProviderType, optional: true },
      ],
      providerReturnType: clientRequestProviderType,
      targetParameters: [
        targetParameter("options", targetTypes.RequestOptions),
        targetParameter("callback", responseListenerTargetType, { optional: true }),
      ],
      targetReturnType: targetTypes.ClientRequest,
      declaringType: httpsTargetType,
    }),
  ]);
  return [
    nodejsModuleCallTargetMetadata({
      exportName: "createServer",
      signatureId: "node:https.createServer(HttpsServerOptions,System.Action`2)",
      targetMemberId: "Tsonic.CSharp.Node.Https.https.createServer(Tsonic.CSharp.Node.Https.HttpsServerOptions,System.Action`2)",
      sourceName: "createServer",
      targetName: "createServer",
      providerParameters: [
        { name: "options", type: serverOptionsProviderType },
        { name: "requestListener", type: requestListenerProviderType, optional: true },
      ],
      providerReturnType: serverProviderType,
      targetParameters: [
        targetParameter("options", targetTypes.ServerOptions),
        targetParameter("requestListener", requestListenerTargetType, { optional: true }),
      ],
      targetReturnType: targetTypes.Server,
      declaringType: httpsTargetType,
    }),
    ...requestRows,
  ];
}

export function nodeHttpsClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const rows: readonly [string, string, ProviderTypeExpression, TargetTypeRef, boolean][] = [
    ["ServerOptions", "ca", caProviderType, tsValueTargetType, true],
    ["ServerOptions", "cert", keyMaterialProviderType, tsValueTargetType, true],
    ["ServerOptions", "key", keyMaterialProviderType, tsValueTargetType, true],
    ["ServerOptions", "pfx", nodeBufferProviderType, tsValueTargetType, true],
    ["ServerOptions", "passphrase", stringProviderType, nullableStringTargetType, true],
    ["ServerOptions", "minVersion", stringProviderType, nullableStringTargetType, true],
    ["ServerOptions", "maxVersion", stringProviderType, nullableStringTargetType, true],
    ["ServerOptions", "requestCert", booleanProviderType, nullableBoolTargetType, true],
    ["ServerOptions", "rejectUnauthorized", booleanProviderType, nullableBoolTargetType, true],
    ["RequestOptions", "ca", caProviderType, tsValueTargetType, true],
    ["RequestOptions", "cert", keyMaterialProviderType, tsValueTargetType, true],
    ["RequestOptions", "key", keyMaterialProviderType, tsValueTargetType, true],
    ["RequestOptions", "pfx", nodeBufferProviderType, tsValueTargetType, true],
    ["RequestOptions", "passphrase", stringProviderType, nullableStringTargetType, true],
    ["RequestOptions", "minVersion", stringProviderType, nullableStringTargetType, true],
    ["RequestOptions", "maxVersion", stringProviderType, nullableStringTargetType, true],
    ["RequestOptions", "rejectUnauthorized", booleanProviderType, nullableBoolTargetType, true],
  ];
  return rows.map(([exportName, memberName, providerType, targetType, optional]) =>
    nodejsClassPropertyTargetMetadata({
      exportName,
      memberName,
      memberId: `node:https.${exportName}.${memberName}`,
      targetMemberId: `Tsonic.CSharp.Node.Https.Https${exportName}.${memberName}`,
      sourceName: memberName,
      targetName: memberName,
      memberKind: "property",
      providerType,
      targetParameters: [],
      targetReturnType: targetType,
      declaringType: targetTypes[exportName as "ServerOptions" | "RequestOptions"],
      ...(optional ? { optional: true } : {}),
    }));
}

function optionDeclaration(
  exportName: "ServerOptions" | "RequestOptions",
  heritage: ProviderTypeExpression,
): ProviderExportDeclaration {
  return {
    id: `${nodeHttpsModuleSpecifier}.${exportName}`,
    name: exportName,
    kind: "interface",
    heritage: [{ kind: "extends", type: heritage }],
    members: nodeHttpsClassPropertyTargetMembers()
      .filter((member) => member.exportName === exportName)
      .map((member) => ({
        id: member.memberId,
        name: member.memberName,
        kind: "property" as const,
        optional: true,
        type: member.providerType,
      })),
  };
}
