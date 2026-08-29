import type {
  ProviderExportDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpNullableTargetType,
  csharpNullableValueTargetType,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpVoidTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  booleanProviderType,
  callbackProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  numberProviderType,
  providerRef,
  stringProviderType,
  voidProviderType,
} from "../capability-module.js";
import {
  nodejsClassCallTargetMetadata,
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";
import {
  nodeFsModuleSpecifier,
} from "./identities.js";
import {
  statsProviderType,
  statsTargetType,
} from "./types.js";

const optionExportNames = [
  "WatchOptions",
  "ReadStreamOptions",
  "WriteStreamOptions",
] as const;
const runtimeClassNames = [
  "FsWatcher",
  "StatWatcher",
  "ReadStream",
  "WriteStream",
] as const;
export const nodeFsStreamClassNames = [
  ...optionExportNames,
  ...runtimeClassNames,
] as const;

const targetTypes: Record<(typeof nodeFsStreamClassNames)[number], TargetTypeRef> = {
  WatchOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "WatchOptions"),
  ReadStreamOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "ReadStreamOptions"),
  WriteStreamOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "WriteStreamOptions"),
  FsWatcher: nodejsTargetNamedType("Tsonic.CSharp.Node", "FsWatcher"),
  StatWatcher: nodejsTargetNamedType("Tsonic.CSharp.Node", "StatWatcher"),
  ReadStream: nodejsTargetNamedType("Tsonic.CSharp.Node", "ReadStream"),
  WriteStream: nodejsTargetNamedType("Tsonic.CSharp.Node", "WriteStream"),
};
const fsTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "fs");
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const longTargetType = csharpSourcePrimitiveTargetType("int64");
const nullableLongTargetType = csharpNullableValueTargetType(longTargetType);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const nullableBoolTargetType = csharpNullableValueTargetType(boolTargetType);
const voidTargetType = csharpVoidTargetType();
const watchListenerProviderType = callbackProviderType("node:fs.watch.listener", [
  { name: "eventType", type: stringProviderType },
  { name: "filename", type: stringProviderType, optional: true },
]);
const watchListenerTargetType = csharpDelegateTargetType("System.Action", [
  stringTargetType,
  nullableStringTargetType,
]);
const statListenerProviderType = callbackProviderType("node:fs.watchFile.listener", [
  { name: "current", type: statsProviderType },
  { name: "previous", type: statsProviderType },
]);
const statListenerTargetType = csharpDelegateTargetType("System.Action", [
  statsTargetType,
  statsTargetType,
]);
const readStreamFlagProviderType = literalStringUnion("r", "r+", "rs+");
const writeStreamFlagProviderType = literalStringUnion(
  "w",
  "wx",
  "w+",
  "wx+",
  "a",
  "ax",
  "a+",
  "ax+",
  "as",
  "as+",
);

export function nodeFsStreamExportDeclarations(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeFsModuleSpecifier,
    moduleCalls: nodeFsStreamCallTargetMembers(),
    classCalls: nodeFsStreamClassCallTargetMembers(),
    classProperties: nodeFsStreamClassPropertyTargetMembers(),
    classes: runtimeClassNames,
    classHeritage: {
      FsWatcher: [providerRef("node:events", "EventEmitter")],
      StatWatcher: [providerRef("node:events", "EventEmitter")],
      ReadStream: [providerRef("node:stream", "Readable")],
      WriteStream: [providerRef("node:stream", "Writable")],
    },
    additionalExports: optionExportNames.map(optionDeclaration),
    includeDefaultExports: false,
  });
}

export function nodeFsStreamCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return Object.freeze([
    moduleCall("createReadStream", [
      stringParameter("path"),
      optionalProviderParameter("options", providerClass("ReadStreamOptions")),
    ], providerClass("ReadStream"), [
      targetParameter("path", stringTargetType),
      targetParameter("options", targetTypes.ReadStreamOptions, { optional: true }),
    ], targetTypes.ReadStream),
    moduleCall("createWriteStream", [
      stringParameter("path"),
      optionalProviderParameter("options", providerClass("WriteStreamOptions")),
    ], providerClass("WriteStream"), [
      targetParameter("path", stringTargetType),
      targetParameter("options", targetTypes.WriteStreamOptions, { optional: true }),
    ], targetTypes.WriteStream),
    moduleCall("watch", [
      stringParameter("path"),
      optionalProviderParameter("listener", watchListenerProviderType),
    ], providerClass("FsWatcher"), [
      targetParameter("path", stringTargetType),
      targetParameter("listener", watchListenerTargetType, { optional: true }),
    ], targetTypes.FsWatcher),
    moduleCall("watch", [
      stringParameter("path"),
      { name: "options", type: providerClass("WatchOptions") },
      optionalProviderParameter("listener", watchListenerProviderType),
    ], providerClass("FsWatcher"), [
      targetParameter("path", stringTargetType),
      targetParameter("options", targetTypes.WatchOptions),
      targetParameter("listener", watchListenerTargetType, { optional: true }),
    ], targetTypes.FsWatcher),
    moduleCall("watchFile", [
      stringParameter("path"),
      optionalProviderParameter("listener", statListenerProviderType),
    ], providerClass("StatWatcher"), [
      targetParameter("path", stringTargetType),
      targetParameter("listener", statListenerTargetType, { optional: true }),
    ], targetTypes.StatWatcher),
    moduleCall("unwatchFile", [
      stringParameter("path"),
      optionalProviderParameter("listener", statListenerProviderType),
    ], voidProviderType, [
      targetParameter("path", stringTargetType),
      targetParameter("listener", statListenerTargetType, { optional: true }),
    ], voidTargetType),
  ]);
}

export function nodeFsStreamClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const calls: NodejsClassCallTargetMetadata[] = [];
  for (const exportName of ["FsWatcher", "StatWatcher"] as const) {
    for (const memberName of ["close", "ref", "unref"] as const) {
      calls.push(classCall(exportName, memberName, [], providerClass(exportName), [], targetTypes[exportName]));
    }
  }
  calls.push(classCall("ReadStream", "pipe", [
    { name: "destination", type: providerClass("WriteStream") },
  ], providerClass("WriteStream"), [
    targetParameter("destination", targetTypes.WriteStream),
  ], targetTypes.WriteStream, "pipeTo"));
  return Object.freeze(calls);
}

export function nodeFsStreamClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return Object.freeze([
    optionProperty("WatchOptions", "persistent", booleanProviderType, nullableBoolTargetType),
    optionProperty("WatchOptions", "recursive", booleanProviderType, nullableBoolTargetType),
    optionProperty("ReadStreamOptions", "flags", readStreamFlagProviderType, nullableStringTargetType),
    optionProperty("ReadStreamOptions", "encoding", stringProviderType, nullableStringTargetType),
    optionProperty("ReadStreamOptions", "start", numberProviderType, nullableLongTargetType),
    optionProperty("ReadStreamOptions", "end", numberProviderType, nullableLongTargetType),
    optionProperty("ReadStreamOptions", "highWaterMark", numberProviderType, nullableIntTargetType),
    optionProperty("WriteStreamOptions", "flags", writeStreamFlagProviderType, nullableStringTargetType),
    optionProperty("WriteStreamOptions", "encoding", stringProviderType, nullableStringTargetType),
    optionProperty("WriteStreamOptions", "start", numberProviderType, nullableLongTargetType),
    optionProperty("WriteStreamOptions", "highWaterMark", numberProviderType, nullableIntTargetType),
    optionProperty("WriteStreamOptions", "flush", booleanProviderType, nullableBoolTargetType),
    readonlyProperty("FsWatcher", "closed", booleanProviderType, boolTargetType),
    readonlyProperty("StatWatcher", "closed", booleanProviderType, boolTargetType),
    readonlyProperty("ReadStream", "path", stringProviderType, stringTargetType),
    readonlyProperty("ReadStream", "bytesRead", numberProviderType, longTargetType),
    readonlyProperty("WriteStream", "path", stringProviderType, stringTargetType),
    readonlyProperty("WriteStream", "bytesWritten", numberProviderType, longTargetType),
  ]);
}

function moduleCall(
  exportName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsModuleCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
): NodejsModuleCallTargetMetadata {
  const shape = providerParameters.map((parameter) => parameter.name).join(",");
  return nodejsModuleCallTargetMetadata({
    exportName,
    signatureId: `${nodeFsModuleSpecifier}.${exportName}(${shape})`,
    targetMemberId: `Tsonic.CSharp.Node.fs.${exportName}(${shape})`,
    sourceName: exportName,
    targetName: exportName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: fsTargetType,
  });
}

function classCall(
  exportName: "FsWatcher" | "StatWatcher" | "ReadStream",
  memberName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
  targetName = memberName,
): NodejsClassCallTargetMetadata {
  const shape = providerParameters.map((parameter) => parameter.name).join(",");
  return nodejsClassCallTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeFsModuleSpecifier}.${exportName}.${memberName}`,
    signatureId: `${nodeFsModuleSpecifier}.${exportName}.${memberName}(${shape})`,
    targetMemberId: `Tsonic.CSharp.Node.${exportName}.${targetName}(${shape})`,
    sourceName: memberName,
    targetName,
    memberKind: "method",
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: targetTypes[exportName],
  });
}

function optionProperty(
  exportName: "WatchOptions" | "ReadStreamOptions" | "WriteStreamOptions",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return {
    ...property(exportName, memberName, providerType, targetReturnType, false),
    optional: true,
  };
}

function optionDeclaration(
  exportName: (typeof optionExportNames)[number],
): ProviderExportDeclaration {
  return {
    id: `${nodeFsModuleSpecifier}.${exportName}`,
    name: exportName,
    kind: "interface",
    members: nodeFsStreamClassPropertyTargetMembers()
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

function readonlyProperty(
  exportName: "FsWatcher" | "StatWatcher" | "ReadStream" | "WriteStream",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return property(exportName, memberName, providerType, targetReturnType, true);
}

function property(
  exportName: (typeof nodeFsStreamClassNames)[number],
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  readonly: boolean,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeFsModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.${exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType: targetTypes[exportName],
    ...(readonly ? { readonly: true } : {}),
  });
}

function providerClass(exportName: (typeof nodeFsStreamClassNames)[number]): ProviderTypeExpression {
  return providerRef(nodeFsModuleSpecifier, exportName);
}

function stringParameter(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType };
}

function optionalProviderParameter(
  name: string,
  type: ProviderTypeExpression,
): ProviderParameterDeclaration {
  return { name, type, optional: true };
}

function literalStringUnion(...values: readonly string[]): ProviderTypeExpression {
  return {
    kind: "union",
    types: values.map((value) => ({ kind: "literal", value })),
  };
}
