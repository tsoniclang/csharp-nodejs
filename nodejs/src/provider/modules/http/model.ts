import { csharpNullableTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../../model/target-members.js";
import {
  stringTargetType,
  serverTargetType,
  clientRequestTargetType,
  requestOptionsTargetType,
  serverProviderType,
  clientRequestProviderType,
  requestOptionsProviderType,
  responseListenerProviderType,
  responseListenerTargetType,
  requestListenerProviderType,
  requestListenerTargetType,
  stringParameter,
} from "./types.js";
import { nodeHttpModuleCall } from "./members.js";
import { httpHeaderCallTargetMembers, headerValuesPropertyTargetMembers } from "./headers.js";
import {
  incomingMessageReadCallTargetMembers,
  incomingMessageLifecycleCallTargetMembers,
  incomingMessageMethodPropertyTargetMember,
  incomingMessageUrlPropertyTargetMember,
  incomingMessagePropertyTargetMembers,
  clientRequestCallTargetMembers,
  requestOptionsPropertyTargetMembers,
  clientRequestPropertyTargetMembers,
} from "./requests.js";
import { serverResponseCallTargetMembers, serverResponseStatusCodePropertyTargetMember, serverResponsePropertyTargetMembers } from "./responses.js";
import {
  serverCallTargetMembers,
  serverPropertyTargetMembers,
  addressInfoPropertyTargetMembers,
  serverAddressPropertyTargetMembers,
} from "./servers.js";

export function nodeHttpCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    nodeHttpModuleCall({
      exportName: "createServer",
      signatureId: "node:http.createServer(Function)",
      targetMemberId: "Tsonic.CSharp.Node.Http.http.createServer(System.Action`2)",
      sourceName: "createServer",
      targetName: "createServer",
      providerParameters: [{ name: "requestListener", type: requestListenerProviderType, optional: true }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter("requestListener", csharpNullableTargetType(requestListenerTargetType), { optional: true })],
      targetReturnType: serverTargetType,
    }),
    ...(["request", "get"] as const).flatMap((exportName) => [
      nodeHttpModuleCall({
        exportName,
        signatureId: `node:http.${exportName}(System.String,System.Action\`1)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.http.${exportName}(System.String,System.Action\`1)`,
        sourceName: exportName,
        targetName: exportName,
        providerParameters: [stringParameter("url"), {
          name: "callback",
          type: responseListenerProviderType(`node:http.${exportName}(System.String,System.Action\`1).callback`),
          optional: true,
        }],
        providerReturnType: clientRequestProviderType,
        targetParameters: [targetParameter("url", stringTargetType), targetParameter("callback", csharpNullableTargetType(responseListenerTargetType), { optional: true })],
        targetReturnType: clientRequestTargetType,
      }),
      nodeHttpModuleCall({
        exportName,
        signatureId: `node:http.${exportName}(RequestOptions,System.Action\`1)`,
        targetMemberId: `Tsonic.CSharp.Node.Http.http.${exportName}(Tsonic.CSharp.Node.Http.RequestOptions,System.Action\`1)`,
        sourceName: exportName,
        targetName: exportName,
        providerParameters: [{ name: "options", type: requestOptionsProviderType }, {
          name: "callback",
          type: responseListenerProviderType(`node:http.${exportName}(RequestOptions,System.Action\`1).callback`),
          optional: true,
        }],
        providerReturnType: clientRequestProviderType,
        targetParameters: [targetParameter("options", requestOptionsTargetType), targetParameter("callback", csharpNullableTargetType(responseListenerTargetType), { optional: true })],
        targetReturnType: clientRequestTargetType,
      }),
    ]),
  ];
}

export function nodeHttpClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    ...incomingMessageReadCallTargetMembers(),
    ...httpHeaderCallTargetMembers(),
    ...incomingMessageLifecycleCallTargetMembers(),
    ...clientRequestCallTargetMembers(),
    ...serverResponseCallTargetMembers(),
    ...serverCallTargetMembers(),
  ];
}

export function nodeHttpClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    incomingMessageMethodPropertyTargetMember(),
    ...requestOptionsPropertyTargetMembers(),
    ...clientRequestPropertyTargetMembers(),
    incomingMessageUrlPropertyTargetMember(),
    serverResponseStatusCodePropertyTargetMember(),
    ...incomingMessagePropertyTargetMembers(),
    ...headerValuesPropertyTargetMembers(),
    ...serverResponsePropertyTargetMembers(),
    ...serverPropertyTargetMembers(),
    ...addressInfoPropertyTargetMembers(),
    ...serverAddressPropertyTargetMembers(),
  ];
}
