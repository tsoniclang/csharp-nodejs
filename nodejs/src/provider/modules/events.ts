import type {
  ProviderExportDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpJsArrayTargetType,
  csharpSourcePrimitiveTargetType,
  csharpTsValueTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import {
  callbackProviderType,
  arrayProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  stringProviderType,
  unionProviderType,
  unknownProviderType,
} from "./declarations.js";
import {
  nodejsClassCallTargetMetadata,
} from "../members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
} from "../members/target-member-metadata.js";

export const nodeEventsModuleSpecifier = "node:events";
export const nodeEventEmitterExportName = "EventEmitter";

const eventEmitterTargetType = nodejsTargetNamedType(
  "Tsonic.CSharp.Node",
  "EventEmitter",
);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const tsValueTargetType = csharpTsValueTargetType();
const eventNameProviderType = unionProviderType(
  stringProviderType,
  { kind: "source-global", name: "Symbol" },
);
const eventNameArrayProviderType = arrayProviderType(eventNameProviderType);
const eventNameArrayTargetType = csharpJsArrayTargetType(tsValueTargetType);

const listenerShapes = Object.freeze([
  {
    suffix: "0",
    providerType: callbackProviderType("node:events.listener.0", []),
    targetType: csharpDelegateTargetType("System.Action", []),
  },
  {
    suffix: "1",
    providerType: callbackProviderType("node:events.listener.1", [
      { name: "value", type: unknownProviderType },
    ]),
    targetType: csharpDelegateTargetType("System.Action", [tsValueTargetType]),
  },
  {
    suffix: "2",
    providerType: callbackProviderType("node:events.listener.2", [
      { name: "first", type: unknownProviderType },
      { name: "second", type: unknownProviderType },
    ]),
    targetType: csharpDelegateTargetType("System.Action", [
      tsValueTargetType,
      tsValueTargetType,
    ]),
  },
  {
    suffix: "3",
    providerType: callbackProviderType("node:events.listener.3", [
      { name: "first", type: unknownProviderType },
      { name: "second", type: unknownProviderType },
      { name: "third", type: unknownProviderType },
    ]),
    targetType: csharpDelegateTargetType("System.Action", [
      tsValueTargetType,
      tsValueTargetType,
      tsValueTargetType,
    ]),
  },
]);

export function nodeEventsExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeEventsModuleSpecifier,
    classCalls: nodeEventsClassCallTargetMembers(),
    classes: [nodeEventEmitterExportName],
  });
}

export function nodeEventsClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const constructor = nodejsClassCallTargetMetadata({
    exportName: nodeEventEmitterExportName,
    memberName: "constructor",
    memberId: "node:events.EventEmitter.constructor",
    signatureId: "node:events.EventEmitter.constructor()",
    targetMemberId: "Tsonic.CSharp.Node.EventEmitter..ctor()",
    sourceName: "constructor",
    targetName: "EventEmitter",
    memberKind: "constructor",
    providerParameters: [],
    targetParameters: [],
    targetReturnType: eventEmitterTargetType,
    declaringType: eventEmitterTargetType,
  });
  const listenerMembers = [
    "addListener",
    "on",
    "once",
    "off",
    "prependListener",
    "prependOnceListener",
    "removeListener",
  ] as const;
  const listenerCalls = listenerMembers.flatMap((memberName) =>
    listenerShapes.map((listener) => nodejsClassCallTargetMetadata({
      exportName: nodeEventEmitterExportName,
      memberName,
      memberId: `node:events.EventEmitter.${memberName}`,
      signatureId: `node:events.EventEmitter.${memberName}.${listener.suffix}`,
      targetMemberId: `Tsonic.CSharp.Node.EventEmitter.${memberName}.${listener.suffix}`,
      sourceName: memberName,
      targetName: memberName,
      memberKind: "method",
      providerParameters: [
        { name: "eventName", type: eventNameProviderType },
        { name: "listener", type: listener.providerType },
      ],
      providerReturnType: providerEventEmitterType(),
      targetParameters: [
        targetParameter("eventName", tsValueTargetType),
        targetParameter("listener", listener.targetType),
      ],
      targetReturnType: eventEmitterTargetType,
      declaringType: eventEmitterTargetType,
    })),
  );
  const emit = nodejsClassCallTargetMetadata({
    exportName: nodeEventEmitterExportName,
    memberName: "emit",
    memberId: "node:events.EventEmitter.emit",
    signatureId: "node:events.EventEmitter.emit(eventName,...unknown[])",
    targetMemberId: "Tsonic.CSharp.Node.EventEmitter.emit(TsValue,params TsValue[])",
    sourceName: "emit",
    targetName: "emit",
    memberKind: "method",
    providerParameters: [
      { name: "eventName", type: eventNameProviderType },
      { name: "values", type: { kind: "array", elementType: unknownProviderType }, rest: true },
    ],
    providerReturnType: { kind: "boolean" },
    targetParameters: [
      targetParameter("eventName", tsValueTargetType),
      targetParameter("values", tsValueTargetType, { paramsArray: true }),
    ],
    targetReturnType: boolTargetType,
    declaringType: eventEmitterTargetType,
  });
  const removeAllWithoutName = nodejsClassCallTargetMetadata({
    exportName: nodeEventEmitterExportName,
    memberName: "removeAllListeners",
    memberId: "node:events.EventEmitter.removeAllListeners",
    signatureId: "node:events.EventEmitter.removeAllListeners()",
    targetMemberId: "Tsonic.CSharp.Node.EventEmitter.removeAllListeners()",
    sourceName: "removeAllListeners",
    targetName: "removeAllListeners",
    memberKind: "method",
    providerParameters: [],
    providerReturnType: providerEventEmitterType(),
    targetParameters: [],
    targetReturnType: eventEmitterTargetType,
    declaringType: eventEmitterTargetType,
  });
  const removeAllWithName = nodejsClassCallTargetMetadata({
    exportName: nodeEventEmitterExportName,
    memberName: "removeAllListeners",
    memberId: "node:events.EventEmitter.removeAllListeners",
    signatureId: "node:events.EventEmitter.removeAllListeners(eventName)",
    targetMemberId: "Tsonic.CSharp.Node.EventEmitter.removeAllListeners(TsValue)",
    sourceName: "removeAllListeners",
    targetName: "removeAllListeners",
    memberKind: "method",
    providerParameters: [{ name: "eventName", type: eventNameProviderType }],
    providerReturnType: providerEventEmitterType(),
    targetParameters: [targetParameter("eventName", tsValueTargetType)],
    targetReturnType: eventEmitterTargetType,
    declaringType: eventEmitterTargetType,
  });
  const scalarCalls = [
    scalarCall("getMaxListeners", [], numberProviderType(), [], intTargetType),
    scalarCall("listenerCount", [
      { name: "eventName", type: eventNameProviderType },
    ], numberProviderType(), [
      targetParameter("eventName", tsValueTargetType),
    ], intTargetType),
    scalarCall("setMaxListeners", [
      { name: "count", type: numberProviderType() },
    ], providerEventEmitterType(), [
      targetParameter("count", intTargetType),
    ], eventEmitterTargetType),
    scalarCall("eventNames", [], eventNameArrayProviderType, [], eventNameArrayTargetType),
  ];
  return Object.freeze([
    constructor,
    ...listenerCalls,
    emit,
    removeAllWithoutName,
    removeAllWithName,
    ...scalarCalls,
  ]);
}

function scalarCall(
  memberName: string,
  providerParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["providerParameters"],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetReturnType"],
): NodejsClassCallTargetMetadata {
  return nodejsClassCallTargetMetadata({
    exportName: nodeEventEmitterExportName,
    memberName,
    memberId: `node:events.EventEmitter.${memberName}`,
    signatureId: `node:events.EventEmitter.${memberName}(${providerParameters.map((parameter) => parameter.name).join(",")})`,
    targetMemberId: `Tsonic.CSharp.Node.EventEmitter.${memberName}(${targetParameters.map((parameter) => parameter.name).join(",")})`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "method",
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: eventEmitterTargetType,
  });
}

function numberProviderType(): ProviderTypeExpression {
  return { kind: "number" };
}

function providerEventEmitterType(): ProviderTypeExpression {
  return {
    kind: "provider-ref",
    moduleSpecifier: nodeEventsModuleSpecifier,
    exportName: nodeEventEmitterExportName,
  };
}
