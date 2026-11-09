import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsClassCallTargetMetadata, NodejsClassPropertyTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeParameterMapping } from "../native/authoring.js";
import type { NodejsNativeTypeDeclarationLocator, NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeOwner, nativeParameter, nativeSlot } from "../native/authoring.js";
import { nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";
import { callbackProviderType, nodejsCapabilityModuleExports, providerRef, unionProviderType } from "../declarations/exports.js";
import { booleanProviderType, errorProviderType, int32ProviderType, stringProviderType, undefinedProviderType, voidProviderType } from "../model/source-types.js";
import { nodeBufferProviderType } from "./buffer/provider-types.js";

export const nodeStreamModuleSpecifier = "node:stream";
const classNames = ["Stream", "Readable", "Writable", "Duplex", "Transform"] as const;
type StreamClass = (typeof classNames)[number];
const owners: Readonly<Record<StreamClass, NodejsNativeTypeDeclarationLocator>> = {
  Stream: nativeOwner("Tsonic.CSharp.Node.Stream"), Readable: nativeOwner("Tsonic.CSharp.Node.Readable"),
  Writable: nativeOwner("Tsonic.CSharp.Node.Writable"), Duplex: nativeOwner("Tsonic.CSharp.Node.Duplex"), Transform: nativeOwner("Tsonic.CSharp.Node.Transform"),
};
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const nativeBool: NodejsNativeTypeLocator = { kind: "source-primitive", name: "bool" };
const nativeVoid: NodejsNativeTypeLocator = { kind: "void" };
const buffer = nativeNamedType("Tsonic.CSharp.Node.Buffer");
const exception = nativeNamedType("System.Exception");
const nativeAction = nativeNamedType("System.Action");
const destinationType: ProviderTypeExpression = { kind: "type-parameter", name: "TDestination" };
const nativeDestination: NodejsNativeTypeLocator = { kind: "method-parameter", index: 0 };

export function nodeStreamClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const calls: NodejsClassCallTargetMetadata[] = [
    call("Readable", "from", "from", [{ name: "chunks", type: { kind: "array", elementType: nodeBufferProviderType } }], providerClass("Readable"),
      [nativeParameter(0, nativeNamedType("System.Collections.Generic.IReadOnlyList`1", buffer))], nativeClass("Readable"), "from", true),
    call("Readable", "read", "read", [optionalInt("size")], unionProviderType(nodeBufferProviderType, undefinedProviderType),
      [nativeParameter(0, nativeNullable(nativeInt))], nativeNullableReference(buffer), "readBuffer"),
    call("Readable", "pause", "pause", [], providerClass("Readable"), [], nativeClass("Readable")),
    call("Readable", "resume", "resume", [], providerClass("Readable"), [], nativeClass("Readable")),
    { exportName: "Readable", memberName: "pipe", memberId: "node:stream.Readable.pipe",
      signatureId: "node:stream.Readable.pipe(destination)", memberKind: "method",
      providerParameters: [{ name: "destination", type: destinationType }], providerReturnType: destinationType,
      providerTypeParameters: [{ name: "TDestination", constraints: [providerClass("Writable")] }],
      native: nativeCall({ owner: owners.Readable, kind: "method", targetName: "pipeTo", static: false, typeParameterCount: 1,
        parameters: [nativeParameter(0, nativeDestination)], returnType: nativeDestination, receiver: { kind: "instance" },
        methodTypeParameters: [{ sourceTypeParameterIndex: 0, targetTypeParameterIndex: 0 }] }) },
    call("Readable", "isPaused", "isPaused", [], booleanProviderType, [], nativeBool),
  ];
  for (const exportName of ["Readable", "Writable", "Duplex"] as const) {
    calls.push(call(exportName, "destroy", "destroy", [{ name: "error", type: errorProviderType, optional: true }], providerClass(exportName),
      [nativeParameter(0, nativeNullableReference(exception))], nativeClass(exportName), "destroyChain"));
  }
  for (const exportName of ["Writable", "Duplex"] as const) {
    calls.push(
      call(exportName, "write", "write", [{ name: "chunk", type: stringProviderType }], booleanProviderType,
        [nativeParameter(0, nativeString)], nativeBool),
      call(exportName, "write", "writeBuffer", [{ name: "chunk", type: nodeBufferProviderType }], booleanProviderType,
        [nativeParameter(0, buffer)], nativeBool),
      call(exportName, "end", "end", [], providerClass(exportName), [], nativeClass(exportName)),
      call(exportName, "end", "endString", [{ name: "chunk", type: stringProviderType }], providerClass(exportName),
        [nativeParameter(0, nativeString)], nativeClass(exportName)),
      call(exportName, "end", "endBuffer", [{ name: "chunk", type: nodeBufferProviderType }], providerClass(exportName),
        [nativeParameter(0, buffer)], nativeClass(exportName)),
      call(exportName, "cork", "cork", [], voidProviderType, [], nativeVoid),
      call(exportName, "uncork", "uncork", [], voidProviderType, [], nativeVoid),
    );
  }
  calls.push(...eventCalls("Readable", [
    ["data", nodeBufferProviderType, buffer], ["end", undefined, undefined], ["error", errorProviderType, exception], ["close", undefined, undefined],
  ]));
  for (const exportName of ["Writable", "Duplex"] as const) {
    calls.push(...eventCalls(exportName, [
      ...exportName === "Duplex" ? [["data", nodeBufferProviderType, buffer], ["end", undefined, undefined]] as const : [],
      ["drain", undefined, undefined], ["finish", undefined, undefined], ["error", errorProviderType, exception], ["close", undefined, undefined],
    ]));
  }
  return Object.freeze(calls);
}

export function nodeStreamClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const properties: readonly (readonly ["Readable" | "Writable" | "Duplex", readonly string[]])[] = [
    ["Readable", ["readable", "readableEnded", "destroyed"]],
    ["Writable", ["writable", "writableEnded", "writableFinished", "writableNeedDrain", "destroyed"]],
    ["Duplex", ["writable", "writableEnded", "writableFinished", "writableNeedDrain", "destroyed"]],
  ];
  return properties.flatMap(([exportName, names]) => names.map(memberName => ({
    exportName, memberName, memberId: `node:stream.${exportName}.${memberName}`, memberKind: "property" as const,
    providerType: booleanProviderType, readonly: true as const, native: nativeSlot(owners[exportName], memberName, nativeBool),
  })));
}

export function nodeStreamExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeStreamModuleSpecifier, classCalls: nodeStreamClassCallTargetMembers(),
    classProperties: nodeStreamClassPropertyTargetMembers(), classes: classNames, classHeritage: {
      Readable: [providerClass("Stream")], Writable: [providerClass("Stream")],
      Duplex: [providerClass("Readable")], Transform: [providerClass("Duplex")],
    } });
}

function call(
  exportName: StreamClass, memberName: string, idSuffix: string, providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression, parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator,
  targetName = memberName, isStatic = false,
): NodejsClassCallTargetMetadata {
  return { exportName, memberName, memberId: `node:stream.${exportName}.${memberName}`,
    signatureId: `node:stream.${exportName}.${idSuffix}(${providerParameters.map(parameter => parameter.name).join(",")})`,
    memberKind: "method", providerParameters, providerReturnType, ...isStatic ? { static: true } : {},
    native: nativeCall({ owner: owners[exportName], kind: "method", targetName, static: isStatic, parameters, returnType,
      receiver: { kind: isStatic ? "none" : "instance" } }) };
}

function eventCalls(
  exportName: "Readable" | "Writable" | "Duplex",
  events: readonly (readonly [string, ProviderTypeExpression | undefined, NodejsNativeTypeLocator | undefined])[],
): readonly NodejsClassCallTargetMetadata[] {
  return ["on", "once", "off"].flatMap(methodName => events.map(([eventName, providerArgument, nativeArgument]) =>
    call(exportName, methodName, `${methodName}.${eventName}`, [
      { name: "event", type: { kind: "literal", value: eventName } },
      { name: "listener", type: callbackProviderType(`node:stream.${exportName}.${methodName}.${eventName}.listener`,
        providerArgument === undefined ? [] : [{ name: "value", type: providerArgument }]) },
    ], providerClass(exportName), [
      nativeParameter(0, nativeString),
      nativeParameter(1, nativeArgument === undefined ? nativeAction : nativeNamedType("System.Action`1", nativeArgument)),
    ], nativeClass(exportName))));
}

function providerClass(exportName: StreamClass): ProviderTypeExpression {
  return providerRef(nodeStreamModuleSpecifier, exportName);
}
function nativeClass(exportName: StreamClass): NodejsNativeTypeLocator {
  return nativeNamedType(owners[exportName].metadataName);
}
function optionalInt(name: string): ProviderParameterDeclaration {
  return { name, type: int32ProviderType, optional: true };
}

