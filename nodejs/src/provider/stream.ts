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
  csharpTsValueTargetType,
  csharpVoidTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import {
  booleanProviderType,
  callbackProviderType,
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
} from "./members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";

export const nodeStreamModuleSpecifier = "node:stream";

const classNames = ["Stream", "Readable", "Writable", "Duplex", "Transform"] as const;
const targetTypes = Object.fromEntries(classNames.map((name) => [
  name,
  nodejsTargetNamedType("Tsonic.CSharp.Node", name),
])) as Record<(typeof classNames)[number], ReturnType<typeof nodejsTargetNamedType>>;
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const voidTargetType = csharpVoidTargetType();
const tsValueTargetType = csharpTsValueTargetType();
const actionTargetType = csharpDelegateTargetType("System.Action", []);
const callbackType = callbackProviderType("node:stream.callback", []);

export function nodeStreamExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeStreamModuleSpecifier,
    classCalls: nodeStreamClassCallTargetMembers(),
    classProperties: nodeStreamClassPropertyTargetMembers(),
    classes: classNames,
    classHeritage: {
      Stream: [providerRef("node:events", "EventEmitter")],
      Readable: [providerRef(nodeStreamModuleSpecifier, "Stream")],
      Writable: [providerRef(nodeStreamModuleSpecifier, "Stream")],
      Duplex: [providerRef(nodeStreamModuleSpecifier, "Readable")],
      Transform: [providerRef(nodeStreamModuleSpecifier, "Duplex")],
    },
  });
}

export function nodeStreamClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const calls: NodejsClassCallTargetMetadata[] = classNames.map((exportName) =>
    classCall(exportName, "constructor", "constructor", [], undefined, [], targetTypes[exportName], {
      targetName: exportName,
      memberKind: "constructor",
    })
  );

  calls.push(
    classCall("Readable", "read", "read", [optionalNumber("size")], unknownOrUndefined(), [
      targetParameter("size", nullableIntTargetType, { optional: true }),
    ], tsValueTargetType, { targetName: "readValue" }),
    classCall("Readable", "pause", "pause", [], providerClass("Readable"), [], targetTypes.Readable),
    classCall("Readable", "resume", "resume", [], providerClass("Readable"), [], targetTypes.Readable),
    classCall("Readable", "setEncoding", "setEncoding", [requiredString("encoding")], providerClass("Readable"), [
      targetParameter("encoding", stringTargetType),
    ], targetTypes.Readable),
    classCall("Readable", "pipe", "pipe", [
      { name: "destination", type: providerClass("Writable") },
    ], providerClass("Writable"), [
      targetParameter("destination", targetTypes.Writable),
    ], targetTypes.Writable, { targetName: "pipeTo" }),
  );

  for (const exportName of ["Writable", "Duplex"] as const) {
    calls.push(
      classCall(exportName, "write", "write", [
        { name: "chunk", type: unknownProviderType },
        optionalString("encoding"),
        optionalCallback("callback"),
      ], booleanProviderType, [
        targetParameter("chunk", tsValueTargetType),
        targetParameter("encoding", nullableStringTargetType, { optional: true }),
        targetParameter("callback", actionTargetType, { optional: true }),
      ], boolTargetType),
      classCall(exportName, "end", "end", [
        { name: "chunk", type: unknownProviderType, optional: true },
        optionalString("encoding"),
        optionalCallback("callback"),
      ], voidProviderType, [
        targetParameter("chunk", tsValueTargetType, { optional: true }),
        targetParameter("encoding", nullableStringTargetType, { optional: true }),
        targetParameter("callback", actionTargetType, { optional: true }),
      ], voidTargetType),
      classCall(exportName, "cork", "cork", [], voidProviderType, [], voidTargetType),
      classCall(exportName, "uncork", "uncork", [], voidProviderType, [], voidTargetType),
    );
  }

  return Object.freeze(calls);
}

export function nodeStreamClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return Object.freeze([
    classProperty("Readable", "readable", booleanProviderType, boolTargetType, true),
    classProperty("Readable", "readableEnded", booleanProviderType, boolTargetType, true),
    classProperty("Writable", "writable", booleanProviderType, boolTargetType, true),
    classProperty("Writable", "writableEnded", booleanProviderType, boolTargetType, true),
    classProperty("Duplex", "writable", booleanProviderType, boolTargetType, true),
    classProperty("Duplex", "writableEnded", booleanProviderType, boolTargetType, true),
  ]);
}

function classCall(
  exportName: (typeof classNames)[number],
  memberName: string,
  idSuffix: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression | undefined,
  targetParameters: Parameters<typeof nodejsClassCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
  options: {
    readonly targetName?: string;
    readonly memberKind?: "constructor" | "method";
  } = {},
): NodejsClassCallTargetMetadata {
  const parameterShape = providerParameters.map((parameter) => parameter.name).join(",");
  return nodejsClassCallTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeStreamModuleSpecifier}.${exportName}.${memberName}`,
    signatureId: `${nodeStreamModuleSpecifier}.${exportName}.${idSuffix}(${parameterShape})`,
    targetMemberId: `Tsonic.CSharp.Node.${exportName}.${options.targetName ?? memberName}(${parameterShape})`,
    sourceName: memberName,
    targetName: options.targetName ?? memberName,
    memberKind: options.memberKind ?? "method",
    providerParameters,
    ...(providerReturnType === undefined ? {} : { providerReturnType }),
    targetParameters,
    targetReturnType,
    declaringType: targetTypes[exportName],
  });
}

function classProperty(
  exportName: "Readable" | "Writable" | "Duplex",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: typeof boolTargetType,
  readonly: boolean,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeStreamModuleSpecifier}.${exportName}.${memberName}`,
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

function providerClass(exportName: (typeof classNames)[number]): ProviderTypeExpression {
  return providerRef(nodeStreamModuleSpecifier, exportName);
}

function requiredString(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType };
}

function optionalString(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType, optional: true };
}

function optionalNumber(name: string): ProviderParameterDeclaration {
  return { name, type: numberProviderType, optional: true };
}

function optionalCallback(name: string): ProviderParameterDeclaration {
  return { name, type: callbackType, optional: true };
}

function unknownOrUndefined(): ProviderTypeExpression {
  return unionProviderType(unknownProviderType, undefinedProviderType);
}
