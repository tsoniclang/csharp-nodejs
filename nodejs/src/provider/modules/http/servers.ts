import { csharpDelegateTargetType, csharpNullableTargetType, targetParameter } from "@tsonic/target-csharp/provider";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../../model/target-members.js";
import {
  nodeHttpServerExportName,
  nodeHttpAddressInfoExportName,
  nodeHttpServerAddressExportName,
  stringProviderType,
  numberProviderType,
  voidProviderType,
  boolProviderType,
  undefinedProviderType,
  stringOrUndefinedProviderType,
  stringTargetType,
  nullableStringTargetType,
  intTargetType,
  integerInputAdapter,
  boolTargetType,
  nullableIntTargetType,
  serverTargetType,
  addressInfoTargetType,
  serverAddressTargetType,
  exceptionTargetType,
  serverProviderType,
  addressInfoProviderType,
  serverAddressProviderType,
  errorProviderType,
  voidCallbackProviderType,
  voidListenHostnameCallbackProviderType,
  voidCallbackTargetType,
  callbackProviderType,
  stringParameter,
  numberParameter,
} from "./types.js";
import { typedEventRows, readonlyProperty, nodeHttpClassCall } from "./members.js";

export function serverCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Int32,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Int32,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [numberParameter("port"), { name: "callback", type: voidCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      argumentAdapters: [integerInputAdapter, undefined],
      targetParameters: [targetParameter("port", intTargetType), targetParameter("callback", csharpNullableTargetType(voidCallbackTargetType), { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Int32,System.String,System.Int32,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Int32,System.String,System.Int32,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [
        numberParameter("port"),
        stringParameter("hostname"),
        numberParameter("backlog"),
        { name: "callback", type: voidListenHostnameCallbackProviderType, optional: true },
      ],
      providerReturnType: serverProviderType,
      argumentAdapters: [integerInputAdapter, undefined, integerInputAdapter, undefined],
      targetParameters: [
        targetParameter("port", intTargetType),
        targetParameter("hostname", stringTargetType),
        targetParameter("backlog", nullableIntTargetType),
        targetParameter("callback", csharpNullableTargetType(voidCallbackTargetType), { optional: true }),
      ],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.Int32,System.String,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.Int32,System.String,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [numberParameter("port"), stringParameter("hostname"), { name: "callback", type: voidListenHostnameCallbackProviderType, optional: true }],
      providerReturnType: serverProviderType,
      argumentAdapters: [integerInputAdapter, undefined, undefined],
      targetParameters: [targetParameter("port", intTargetType), targetParameter("hostname", stringTargetType), targetParameter("callback", csharpNullableTargetType(voidCallbackTargetType), { optional: true })],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "listen",
      memberId: "node:http.Server.listen",
      signatureId: "node:http.Server.listen(System.String,System.Action)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.listen(System.String,System.Action)",
      sourceName: "listen",
      targetName: "listen",
      memberKind: "method",
      providerParameters: [
        stringParameter("path"),
        { name: "callback", type: voidCallbackProviderType, optional: true },
      ],
      providerReturnType: serverProviderType,
      targetParameters: [
        targetParameter("path", stringTargetType),
        targetParameter("callback", csharpNullableTargetType(voidCallbackTargetType), { optional: true }),
      ],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "close",
      memberId: "node:http.Server.close",
      signatureId: "node:http.Server.close(System.Action<System.Exception>)",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.close(System.Action<System.Exception>)",
      sourceName: "close",
      targetName: "close",
      memberKind: "method",
      providerParameters: [{
        name: "callback",
        type: callbackProviderType("node:http.Server.close.callback", [
          { name: "error", type: errorProviderType, optional: true },
        ], voidProviderType),
        optional: true,
      }],
      providerReturnType: serverProviderType,
      targetParameters: [targetParameter(
        "callback",
        csharpNullableTargetType(csharpDelegateTargetType("System.Action", [csharpNullableTargetType(exceptionTargetType)])),
        { optional: true },
      )],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    }),
    ...(["ref", "unref"] as const).map((memberName) => nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName,
      memberId: `node:http.Server.${memberName}`,
      signatureId: `node:http.Server.${memberName}()`,
      targetMemberId: `Tsonic.CSharp.Node.Http.Server.${memberName}()`,
      sourceName: memberName,
      targetName: memberName,
      memberKind: "method",
      providerParameters: [],
      providerReturnType: serverProviderType,
      targetParameters: [],
      targetReturnType: serverTargetType,
      declaringType: serverTargetType,
    })),
    ...serverLifecycleCallTargetMembers(),
  ];
}

function serverLifecycleCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return [
    nodeHttpClassCall({
      exportName: nodeHttpServerExportName,
      memberName: "address",
      memberId: "node:http.Server.address",
      signatureId: "node:http.Server.address()",
      targetMemberId: "Tsonic.CSharp.Node.Http.Server.address()",
      sourceName: "address",
      targetName: "address",
      memberKind: "method",
      providerParameters: [],
      providerReturnType: { kind: "union", types: [serverAddressProviderType, undefinedProviderType] },
      targetParameters: [],
      targetReturnType: csharpNullableTargetType(serverAddressTargetType),
      declaringType: serverTargetType,
    }),
    ...typedEventRows(
      nodeHttpServerExportName,
      serverProviderType,
      serverTargetType,
      [
        ["error", [{ name: "error", provider: errorProviderType, target: exceptionTargetType }]],
        ["listening", []],
        ["close", []],
      ],
    ),
  ];
}

export function serverPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [readonlyProperty(nodeHttpServerExportName, "listening", boolProviderType, boolTargetType, serverTargetType)];
}

export function addressInfoPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    readonlyProperty(nodeHttpAddressInfoExportName, "address", stringProviderType, stringTargetType, addressInfoTargetType),
    readonlyProperty(nodeHttpAddressInfoExportName, "family", stringProviderType, stringTargetType, addressInfoTargetType),
    readonlyProperty(nodeHttpAddressInfoExportName, "port", numberProviderType, intTargetType, addressInfoTargetType),
  ];
}

export function serverAddressPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return [
    readonlyProperty(nodeHttpServerAddressExportName, "address", { kind: "union", types: [addressInfoProviderType, undefinedProviderType] }, csharpNullableTargetType(addressInfoTargetType), serverAddressTargetType),
    readonlyProperty(nodeHttpServerAddressExportName, "path", stringOrUndefinedProviderType, nullableStringTargetType, serverAddressTargetType),
    readonlyProperty(nodeHttpServerAddressExportName, "port", { kind: "union", types: [numberProviderType, undefinedProviderType] }, nullableIntTargetType, serverAddressTargetType),
  ];
}
