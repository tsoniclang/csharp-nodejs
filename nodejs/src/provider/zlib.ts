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
  csharpVoidTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  callbackProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  numberProviderType,
  providerRef,
  unknownProviderType,
  voidProviderType,
} from "./capability-module.js";
import {
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "./members/target-member-metadata.js";

export const nodeZlibModuleSpecifier = "node:zlib";

const bufferProviderType = providerRef("node:buffer", "Buffer");
const optionsProviderType = providerRef(nodeZlibModuleSpecifier, "ZlibOptions");
const brotliOptionsProviderType = providerRef(nodeZlibModuleSpecifier, "BrotliOptions");
const transformProviderType = providerRef(nodeZlibModuleSpecifier, "ZlibTransform");
const bufferTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "Buffer");
const optionsTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "ZlibOptions");
const brotliOptionsTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "BrotliOptions");
const transformTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "ZlibTransform");
const zlibTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "zlib");
const exceptionTargetType = csharpNullableTargetType(
  nodejsTargetNamedType("System", "Exception"),
);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const voidTargetType = csharpVoidTargetType();
const callbackProvider = callbackProviderType("node:zlib.callback", [
  { name: "error", type: unknownProviderType },
  { name: "result", type: bufferProviderType },
]);
const callbackTarget = csharpDelegateTargetType("System.Action", [
  exceptionTargetType,
  bufferTargetType,
]);
const codecs = [
  { name: "gzip", options: "zlib" },
  { name: "gunzip", options: "zlib" },
  { name: "deflate", options: "zlib" },
  { name: "inflate", options: "zlib" },
  { name: "deflateRaw", options: "zlib" },
  { name: "inflateRaw", options: "zlib" },
  { name: "unzip", options: "zlib" },
  { name: "brotliCompress", options: "brotli" },
  { name: "brotliDecompress", options: "brotli" },
] as const;

export function nodeZlibExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeZlibModuleSpecifier,
    moduleCalls: nodeZlibCallTargetMembers(),
    classProperties: nodeZlibClassPropertyTargetMembers(),
    classes: ["ZlibOptions", "BrotliOptions", "ZlibTransform"],
    classHeritage: {
      ZlibTransform: [providerRef("node:stream", "Transform")],
    },
  });
}

export function nodeZlibCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  const calls: NodejsModuleCallTargetMetadata[] = [];
  for (const codec of codecs) {
    const providerOptions = codec.options === "brotli"
      ? brotliOptionsProviderType
      : optionsProviderType;
    const targetOptions = codec.options === "brotli"
      ? brotliOptionsTargetType
      : optionsTargetType;
    calls.push(
      moduleCall(`${codec.name}Sync`, [bufferParameter()], bufferProviderType, [
        targetParameter("buffer", bufferTargetType),
      ], bufferTargetType),
      moduleCall(`${codec.name}Sync`, [bufferParameter(), typedOptionsParameter(providerOptions)], bufferProviderType, [
        targetParameter("buffer", bufferTargetType),
        targetParameter("options", targetOptions),
      ], bufferTargetType),
      moduleCall(codec.name, [bufferParameter(), callbackParameter()], voidProviderType, [
        targetParameter("buffer", bufferTargetType),
        targetParameter("callback", callbackTarget),
      ], voidTargetType),
      moduleCall(codec.name, [bufferParameter(), typedOptionsParameter(providerOptions), callbackParameter()], voidProviderType, [
        targetParameter("buffer", bufferTargetType),
        targetParameter("options", targetOptions),
        targetParameter("callback", callbackTarget),
      ], voidTargetType),
    );
  }
  for (const factory of [
    { name: "createDeflate", options: "zlib" },
    { name: "createInflate", options: "zlib" },
    { name: "createGzip", options: "zlib" },
    { name: "createGunzip", options: "zlib" },
    { name: "createDeflateRaw", options: "zlib" },
    { name: "createInflateRaw", options: "zlib" },
    { name: "createUnzip", options: "zlib" },
    { name: "createBrotliCompress", options: "brotli" },
    { name: "createBrotliDecompress", options: "brotli" },
  ] as const) {
    const providerOptions = factory.options === "brotli"
      ? brotliOptionsProviderType
      : optionsProviderType;
    const targetOptions = factory.options === "brotli"
      ? brotliOptionsTargetType
      : optionsTargetType;
    calls.push(moduleCall(factory.name, [optionalTypedOptionsParameter(providerOptions)], transformProviderType, [
      targetParameter("options", targetOptions, { optional: true }),
    ], transformTargetType));
  }
  return Object.freeze(calls);
}

export function nodeZlibClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return Object.freeze([
    ...["level", "chunkSize", "maxOutputLength"].map(
      (memberName) => nodejsClassPropertyTargetMetadata({
        exportName: "ZlibOptions",
        memberName,
        memberId: `node:zlib.ZlibOptions.${memberName}`,
        targetMemberId: `Tsonic.CSharp.Node.ZlibOptions.${memberName}`,
        sourceName: memberName,
        targetName: memberName,
        memberKind: "property",
        providerType: numberProviderType,
        targetParameters: [],
        targetReturnType: nullableIntTargetType,
        declaringType: optionsTargetType,
        optional: true,
      }),
    ),
    ...["quality", "chunkSize", "maxOutputLength"].map((memberName) => nodejsClassPropertyTargetMetadata({
      exportName: "BrotliOptions",
      memberName,
      memberId: `node:zlib.BrotliOptions.${memberName}`,
      targetMemberId: `Tsonic.CSharp.Node.BrotliOptions.${memberName}`,
      sourceName: memberName,
      targetName: memberName,
      memberKind: "property",
      providerType: numberProviderType,
      targetParameters: [],
      targetReturnType: nullableIntTargetType,
      declaringType: brotliOptionsTargetType,
      optional: true,
    })),
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
    signatureId: `${nodeZlibModuleSpecifier}.${exportName}(${shape})`,
    targetMemberId: `Tsonic.CSharp.Node.zlib.${exportName}(${shape})`,
    sourceName: exportName,
    targetName: exportName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: zlibTargetType,
  });
}

function bufferParameter(): ProviderParameterDeclaration {
  return { name: "buffer", type: bufferProviderType };
}

function typedOptionsParameter(type: ProviderTypeExpression): ProviderParameterDeclaration {
  return { name: "options", type };
}

function optionalTypedOptionsParameter(type: ProviderTypeExpression): ProviderParameterDeclaration {
  return { ...typedOptionsParameter(type), optional: true };
}

function callbackParameter(): ProviderParameterDeclaration {
  return { name: "callback", type: callbackProvider };
}
