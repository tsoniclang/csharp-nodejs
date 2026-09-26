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
import { callbackProviderType, nodejsCapabilityModuleExports, nodejsTargetNamedType, providerRef, unionProviderType } from "../declarations/exports.js";
import { booleanProviderType, numberProviderType, stringProviderType, undefinedProviderType, voidProviderType } from "../model/source-types.js";
import { nodejsClassCallTargetMetadata, nodejsClassPropertyTargetMetadata } from "../declarations/target-members.js";
import { nodeBufferProviderType } from "./buffer/provider-types.js";
import { nodeBufferTargetType } from "./buffer/identities.js";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../model/target-members.js";
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
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const voidTargetType = csharpVoidTargetType();
const actionTargetType = csharpDelegateTargetType("System.Action", []);
const errorProviderType = { kind: "source-global", name: "Error" } satisfies ProviderTypeExpression;
const exceptionTargetType = nodejsTargetNamedType("System", "Exception");
const bufferArrayProviderType = {
  kind: "array",
  elementType: nodeBufferProviderType,
} satisfies ProviderTypeExpression;
const bufferArrayTargetType = {
  kind: "array",
  element: nodeBufferTargetType,
} satisfies TargetTypeRef;
const destinationType = {
  kind: "type-parameter",
  name: "TDestination",
} satisfies ProviderTypeExpression;
const destinationTargetType = {
  kind: "type-parameter",
  name: "TDestination",
} satisfies TargetTypeRef;

export function nodeStreamExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeStreamModuleSpecifier,
    classCalls: nodeStreamClassCallTargetMembers(),
    classProperties: nodeStreamClassPropertyTargetMembers(),
    classes: classNames,
    classHeritage: {
      Readable: [providerRef(nodeStreamModuleSpecifier, "Stream")],
      Writable: [providerRef(nodeStreamModuleSpecifier, "Stream")],
      Duplex: [providerRef(nodeStreamModuleSpecifier, "Readable")],
      Transform: [providerRef(nodeStreamModuleSpecifier, "Duplex")],
    },
  });
}

export function nodeStreamClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const calls: NodejsClassCallTargetMetadata[] = [];

  calls.push(
    classCall("Readable", "from", "from", [{ name: "chunks", type: bufferArrayProviderType }], providerClass("Readable"), [
      targetParameter("chunks", bufferArrayTargetType),
    ], targetTypes.Readable, { targetName: "from", static: true }),
    classCall("Readable", "read", "read", [optionalNumber("size")], unionProviderType(nodeBufferProviderType, undefinedProviderType), [
      targetParameter("size", nullableIntTargetType, { optional: true }),
    ], csharpNullableTargetType(nodeBufferTargetType), { targetName: "readBuffer" }),
    classCall("Readable", "pause", "pause", [], providerClass("Readable"), [], targetTypes.Readable),
    classCall("Readable", "resume", "resume", [], providerClass("Readable"), [], targetTypes.Readable),
    classCall("Readable", "pipe", "pipe", [
      { name: "destination", type: destinationType },
    ], destinationType, [
      targetParameter("destination", destinationTargetType),
    ], destinationTargetType, {
      targetName: "pipeTo",
      providerTypeParameters: [{
        name: "TDestination",
        constraints: [providerClass("Writable")],
      }],
      targetTypeParameters: [{
        name: "TDestination",
        constraints: [{ kind: "implements", contract: "Tsonic.CSharp.Node.Writable" }],
      }],
    }),
    classCall("Readable", "isPaused", "isPaused", [], booleanProviderType, [], boolTargetType),
  );

  for (const exportName of ["Readable", "Writable", "Duplex"] as const) {
    calls.push(classCall(exportName, "destroy", "destroy", [{
      name: "error",
      type: errorProviderType,
      optional: true,
    }], providerClass(exportName), [
      targetParameter("error", csharpNullableTargetType(exceptionTargetType), { optional: true }),
    ], targetTypes[exportName], { targetName: "destroyChain" }));
  }

  for (const exportName of ["Writable", "Duplex"] as const) {
    calls.push(
      classCall(exportName, "write", "write", [
        { name: "chunk", type: stringProviderType },
      ], booleanProviderType, [
        targetParameter("chunk", stringTargetType),
      ], boolTargetType),
      classCall(exportName, "write", "writeBuffer", [
        { name: "chunk", type: nodeBufferProviderType },
      ], booleanProviderType, [
        targetParameter("chunk", nodeBufferTargetType),
      ], boolTargetType),
      classCall(exportName, "end", "end", [
      ], providerClass(exportName), [
      ], targetTypes[exportName]),
      classCall(exportName, "end", "endString", [
        { name: "chunk", type: stringProviderType },
      ], providerClass(exportName), [
        targetParameter("chunk", stringTargetType),
      ], targetTypes[exportName]),
      classCall(exportName, "end", "endBuffer", [
        { name: "chunk", type: nodeBufferProviderType },
      ], providerClass(exportName), [
        targetParameter("chunk", nodeBufferTargetType),
      ], targetTypes[exportName]),
      classCall(exportName, "cork", "cork", [], voidProviderType, [], voidTargetType),
      classCall(exportName, "uncork", "uncork", [], voidProviderType, [], voidTargetType),
    );
  }

  calls.push(...streamEventRows(
    "Readable",
    [
      ["data", nodeBufferProviderType, nodeBufferTargetType],
      ["end", undefined, undefined],
      ["error", errorProviderType, exceptionTargetType],
      ["close", undefined, undefined],
    ],
  ));
  for (const exportName of ["Writable", "Duplex"] as const) {
    calls.push(...streamEventRows(
      exportName,
      [
        ...(exportName === "Duplex" ? [
          ["data", nodeBufferProviderType, nodeBufferTargetType],
          ["end", undefined, undefined],
        ] as const : []),
        ["drain", undefined, undefined],
        ["finish", undefined, undefined],
        ["error", errorProviderType, exceptionTargetType],
        ["close", undefined, undefined],
      ],
    ));
  }

  return Object.freeze(calls);
}

export function nodeStreamClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return Object.freeze([
    classProperty("Readable", "readable", booleanProviderType, boolTargetType, true),
    classProperty("Readable", "readableEnded", booleanProviderType, boolTargetType, true),
    classProperty("Readable", "destroyed", booleanProviderType, boolTargetType, true),
    classProperty("Writable", "writable", booleanProviderType, boolTargetType, true),
    classProperty("Writable", "writableEnded", booleanProviderType, boolTargetType, true),
    classProperty("Writable", "writableFinished", booleanProviderType, boolTargetType, true),
    classProperty("Writable", "writableNeedDrain", booleanProviderType, boolTargetType, true),
    classProperty("Writable", "destroyed", booleanProviderType, boolTargetType, true),
    classProperty("Duplex", "writable", booleanProviderType, boolTargetType, true),
    classProperty("Duplex", "writableEnded", booleanProviderType, boolTargetType, true),
    classProperty("Duplex", "writableFinished", booleanProviderType, boolTargetType, true),
    classProperty("Duplex", "writableNeedDrain", booleanProviderType, boolTargetType, true),
    classProperty("Duplex", "destroyed", booleanProviderType, boolTargetType, true),
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
    readonly static?: true;
    readonly providerTypeParameters?: NodejsClassCallTargetMetadata["providerTypeParameters"];
    readonly targetTypeParameters?: NodejsClassCallTargetMetadata["member"]["typeParameters"];
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
    ...(options.providerTypeParameters === undefined
      ? {}
      : { providerTypeParameters: options.providerTypeParameters }),
    targetParameters,
    targetReturnType,
    declaringType: targetTypes[exportName],
    ...(options.targetTypeParameters === undefined
      ? {}
      : { targetTypeParameters: options.targetTypeParameters }),
    ...(options.static === true ? { static: true } : {}),
  });
}

function streamEventRows(
  exportName: "Readable" | "Writable" | "Duplex",
  events: readonly (readonly [
    eventName: string,
    providerArgument: ProviderTypeExpression | undefined,
    targetArgument: TargetTypeRef | undefined,
  ])[],
): readonly NodejsClassCallTargetMetadata[] {
  const rows: NodejsClassCallTargetMetadata[] = [];
  for (const methodName of ["on", "once", "off"] as const) {
    for (const [eventName, providerArgument, targetArgument] of events) {
      const providerListener = callbackProviderType(
        `${nodeStreamModuleSpecifier}.${exportName}.${methodName}.${eventName}.listener`,
        providerArgument === undefined ? [] : [{ name: "value", type: providerArgument }],
      );
      const targetListener = targetArgument === undefined
        ? actionTargetType
        : csharpDelegateTargetType("System.Action", [targetArgument]);
      rows.push(classCall(
        exportName,
        methodName,
        `${methodName}.${eventName}`,
        [
          { name: "event", type: { kind: "literal", value: eventName } },
          { name: "listener", type: providerListener },
        ],
        providerClass(exportName),
        [
          targetParameter("eventName", stringTargetType),
          targetParameter("listener", targetListener),
        ],
        targetTypes[exportName],
      ));
    }
  }
  return rows;
}

function classProperty(
  exportName: "Readable" | "Writable" | "Duplex",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
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

function optionalNumber(name: string): ProviderParameterDeclaration {
  return { name, type: numberProviderType, optional: true };
}
