import type {
  ProviderExportDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpJsArrayTargetType,
  csharpNullableTargetType,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpTaskTargetType,
  csharpTsValueTargetType,
  csharpVoidTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type { TargetTypeRef } from "@tsonic/target-csharp/provider";
import {
  arrayProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  numberProviderType,
  providerRef,
  stringProviderType,
  undefinedProviderType,
  unionProviderType,
  unknownProviderType,
  voidProviderType,
} from "./capability-module.js";
import {
  nodejsClassCallTargetMetadata,
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
  nodejsModulePropertyTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
  NodejsModulePropertyTargetMetadata,
} from "./members/target-member-metadata.js";

export const nodeWorkerThreadsModuleSpecifier = "node:worker_threads";

const workerType = providerRef(nodeWorkerThreadsModuleSpecifier, "Worker");
const workerOptionsType = providerRef(
  nodeWorkerThreadsModuleSpecifier,
  "WorkerOptions",
);
const messagePortType = providerRef(
  nodeWorkerThreadsModuleSpecifier,
  "MessagePort",
);
const workerTargetType = nodejsTargetNamedType(
  "Tsonic.CSharp.Node",
  "Worker",
);
const workerOptionsTargetType = nodejsTargetNamedType(
  "Tsonic.CSharp.Node",
  "WorkerOptions",
);
const messagePortTargetType = nodejsTargetNamedType(
  "Tsonic.CSharp.Node",
  "MessagePort",
);
const messageChannelTargetType = nodejsTargetNamedType(
  "Tsonic.CSharp.Node",
  "MessageChannel",
);
const stringTargetType = csharpStringTargetType();
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const voidTargetType = csharpVoidTargetType();
const valueTargetType = csharpTsValueTargetType();
const stringArrayTargetType = csharpJsArrayTargetType(stringTargetType);
const optionalStringTargetType = csharpNullableTargetType(stringTargetType);
const optionalMessagePortTargetType = csharpNullableTargetType(
  messagePortTargetType,
);

export function nodeWorkerThreadsExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeWorkerThreadsModuleSpecifier,
    moduleCalls: nodeWorkerThreadsModuleCallTargetMembers(),
    moduleProperties: nodeWorkerThreadsModulePropertyTargetMembers(),
    classCalls: nodeWorkerThreadsClassCallTargetMembers(),
    classProperties: nodeWorkerThreadsClassPropertyTargetMembers(),
    classes: [
      "Worker",
      "WorkerOptions",
      "MessagePort",
      "MessageChannel",
    ],
    classHeritage: {
      Worker: [providerRef("node:events", "EventEmitter")],
      MessagePort: [providerRef("node:events", "EventEmitter")],
    },
  });
}

export function nodeWorkerThreadsModuleCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return Object.freeze([
    moduleCall(
      "receiveMessageOnPort",
      [{ name: "port", type: messagePortType }],
      unionProviderType(unknownProviderType, undefinedProviderType),
      [targetParameter("port", messagePortTargetType)],
      valueTargetType,
    ),
    moduleCall(
      "getEnvironmentData",
      [{ name: "key", type: stringProviderType }],
      unionProviderType(unknownProviderType, undefinedProviderType),
      [targetParameter("key", stringTargetType)],
      valueTargetType,
    ),
    moduleCall(
      "setEnvironmentData",
      [
        { name: "key", type: stringProviderType },
        { name: "value", type: unknownProviderType },
      ],
      voidProviderType,
      [
        targetParameter("key", stringTargetType),
        targetParameter("value", valueTargetType),
      ],
      voidTargetType,
    ),
    moduleCall(
      "markAsUntransferable",
      [{ name: "value", type: unknownProviderType }],
      voidProviderType,
      [targetParameter("value", valueTargetType)],
      voidTargetType,
    ),
    moduleCall(
      "isMarkedAsUntransferable",
      [{ name: "value", type: unknownProviderType }],
      { kind: "boolean" },
      [targetParameter("value", valueTargetType)],
      boolTargetType,
    ),
  ]);
}

export function nodeWorkerThreadsModulePropertyTargetMembers(): readonly NodejsModulePropertyTargetMetadata[] {
  return Object.freeze([
    moduleProperty("isMainThread", { kind: "boolean" }, boolTargetType),
    moduleProperty("threadId", numberProviderType, intTargetType),
    moduleProperty("workerData", unknownProviderType, valueTargetType),
    moduleProperty(
      "parentPort",
      unionProviderType(messagePortType, undefinedProviderType),
      optionalMessagePortTargetType,
    ),
  ]);
}

export function nodeWorkerThreadsClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  return Object.freeze([
    nodejsClassCallTargetMetadata({
      exportName: "Worker",
      memberName: "constructor",
      memberId: "node:worker_threads.Worker.constructor",
      signatureId: "node:worker_threads.Worker.constructor(modulePath,options)",
      targetMemberId:
        "Tsonic.CSharp.Node.Worker..ctor(System.String,Tsonic.CSharp.Node.WorkerOptions)",
      sourceName: "constructor",
      targetName: "Worker",
      memberKind: "constructor",
      providerParameters: [
        { name: "modulePath", type: stringProviderType },
        { name: "options", type: workerOptionsType, optional: true },
      ],
      targetParameters: [
        targetParameter("modulePath", stringTargetType),
        targetParameter("options", workerOptionsTargetType, {
          optional: true,
        }),
      ],
      targetReturnType: workerTargetType,
      declaringType: workerTargetType,
      csharpInvocation: {
        kind: "source-module-construction",
        sourceArgumentIndex: 0,
        targetParameterIndex: 0,
        bootstrap: {
          id: "tsonic.csharp.node.worker-threads.process-v1",
          declaringType: nodejsTargetNamedType(
            "Tsonic.CSharp.Node",
            "worker_threads",
          ),
          methodName: "InitializeWorkerProcess",
        },
      },
    }),
    classConstructor("MessageChannel", messageChannelTargetType),
    workerMethod(
      "postMessage",
      [{ name: "value", type: unknownProviderType }],
      voidProviderType,
      [targetParameter("value", valueTargetType)],
      voidTargetType,
    ),
    workerMethod(
      "terminate",
      [],
      { kind: "source-global", name: "Promise", typeArguments: [numberProviderType] },
      [],
      csharpTaskTargetType(intTargetType),
    ),
    workerMethod("ref", [], workerType, [], workerTargetType, "ref"),
    workerMethod("unref", [], workerType, [], workerTargetType),
    portMethod(
      "postMessage",
      [{ name: "value", type: unknownProviderType }],
      voidProviderType,
      [targetParameter("value", valueTargetType)],
      voidTargetType,
    ),
    portMethod("start", [], voidProviderType, [], voidTargetType),
    portMethod("close", [], voidProviderType, [], voidTargetType),
    portMethod("ref", [], messagePortType, [], messagePortTargetType, "ref"),
    portMethod("unref", [], messagePortType, [], messagePortTargetType),
    portMethod("hasRef", [], { kind: "boolean" }, [], boolTargetType),
  ]);
}

export function nodeWorkerThreadsClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return Object.freeze([
    readonlyProperty("Worker", "threadId", numberProviderType, intTargetType, workerTargetType),
    readonlyProperty(
      "MessageChannel",
      "port1",
      messagePortType,
      messagePortTargetType,
      messageChannelTargetType,
    ),
    readonlyProperty(
      "MessageChannel",
      "port2",
      messagePortType,
      messagePortTargetType,
      messageChannelTargetType,
    ),
    optionProperty("WorkerOptions", "name", stringProviderType, optionalStringTargetType, workerOptionsTargetType),
    optionProperty("WorkerOptions", "argv", arrayProviderType(stringProviderType), csharpNullableTargetType(stringArrayTargetType), workerOptionsTargetType),
    optionProperty("WorkerOptions", "env", unknownProviderType, valueTargetType, workerOptionsTargetType),
    optionProperty("WorkerOptions", "workerData", unknownProviderType, valueTargetType, workerOptionsTargetType),
  ]);
}

function moduleCall(
  exportName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsModuleCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
): NodejsModuleCallTargetMetadata {
  return nodejsModuleCallTargetMetadata({
    exportName,
    signatureId: `${nodeWorkerThreadsModuleSpecifier}.${exportName}(${providerParameters.map((parameter) => parameter.name).join(",")})`,
    targetMemberId: `Tsonic.CSharp.Node.worker_threads.${exportName}`,
    sourceName: exportName,
    targetName: exportName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: nodejsTargetNamedType(
      "Tsonic.CSharp.Node",
      "worker_threads",
    ),
  });
}

function moduleProperty(
  exportName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsModulePropertyTargetMetadata {
  return nodejsModulePropertyTargetMetadata({
    exportName,
    targetMemberId: `Tsonic.CSharp.Node.worker_threads.${exportName}`,
    sourceName: exportName,
    targetName: exportName,
    providerType,
    targetReturnType,
    declaringType: nodejsTargetNamedType(
      "Tsonic.CSharp.Node",
      "worker_threads",
    ),
  });
}

function classConstructor(
  exportName: string,
  declaringType: TargetTypeRef,
): NodejsClassCallTargetMetadata {
  return nodejsClassCallTargetMetadata({
    exportName,
    memberName: "constructor",
    memberId: `${nodeWorkerThreadsModuleSpecifier}.${exportName}.constructor`,
    signatureId: `${nodeWorkerThreadsModuleSpecifier}.${exportName}.constructor()`,
    targetMemberId: `${declaringType.kind === "target-named" ? declaringType.id : exportName}..ctor()`,
    sourceName: "constructor",
    targetName: exportName,
    memberKind: "constructor",
    providerParameters: [],
    targetParameters: [],
    targetReturnType: declaringType,
    declaringType,
  });
}

function workerMethod(
  memberName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
  targetName = memberName,
): NodejsClassCallTargetMetadata {
  return classMethod(
    "Worker",
    workerTargetType,
    memberName,
    targetName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
  );
}

function portMethod(
  memberName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
  targetName = memberName,
): NodejsClassCallTargetMetadata {
  return classMethod(
    "MessagePort",
    messagePortTargetType,
    memberName,
    targetName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
  );
}

function classMethod(
  exportName: string,
  declaringType: TargetTypeRef,
  memberName: string,
  targetName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
): NodejsClassCallTargetMetadata {
  const shape = providerParameters.map((parameter) => parameter.name).join(",");
  return nodejsClassCallTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeWorkerThreadsModuleSpecifier}.${exportName}.${memberName}`,
    signatureId: `${nodeWorkerThreadsModuleSpecifier}.${exportName}.${memberName}(${shape})`,
    targetMemberId: `${declaringType.kind === "target-named" ? declaringType.id : exportName}.${targetName}(${shape})`,
    sourceName: memberName,
    targetName,
    memberKind: "method",
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType,
  });
}

function readonlyProperty(
  exportName: string,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeWorkerThreadsModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `${declaringType.kind === "target-named" ? declaringType.id : exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
    readonly: true,
  });
}

function optionProperty(
  exportName: "WorkerOptions",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeWorkerThreadsModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `${declaringType.kind === "target-named" ? declaringType.id : exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
    optional: true,
  });
}
