import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeParameterMapping } from "../native/authoring.js";
import type { NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeDefault, nativeOwner, nativeParameter, nativeSlot } from "../native/authoring.js";
import { nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";
import { callbackProviderType, nodejsCapabilityModuleExports, providerRef, unionProviderType } from "../declarations/exports.js";
import { int32ProviderType, undefinedProviderType, voidProviderType } from "../model/source-types.js";
import { optionalNodeErrorProviderType } from "./util/node-error.js";

export const nodeZlibModuleSpecifier = "node:zlib";
const owner = nativeOwner("Tsonic.CSharp.Node.zlib");
const bufferProviderType = providerRef("node:buffer", "Buffer");
const optionsProviderType = providerRef(nodeZlibModuleSpecifier, "ZlibOptions");
const brotliOptionsProviderType = providerRef(nodeZlibModuleSpecifier, "BrotliOptions");
const transformProviderType = providerRef(nodeZlibModuleSpecifier, "ZlibTransform");
const buffer = nativeNamedType("Tsonic.CSharp.Node.Buffer");
const transform = nativeNamedType("Tsonic.CSharp.Node.ZlibTransform");
const callbackProvider = callbackProviderType("node:zlib.callback", [
  { name: "error", type: optionalNodeErrorProviderType },
  { name: "result", type: unionProviderType(bufferProviderType, undefinedProviderType) },
]);
const callback = nativeNamedType("System.Action`2", nativeNullableReference(nativeNamedType("System.Exception")), nativeNullableReference(buffer));
const codecs = [
  { name: "gzip", options: "ZlibOptions", factory: "createGzip" },
  { name: "gunzip", options: "ZlibOptions", factory: "createGunzip" },
  { name: "deflate", options: "ZlibOptions", factory: "createDeflate" },
  { name: "inflate", options: "ZlibOptions", factory: "createInflate" },
  { name: "deflateRaw", options: "ZlibOptions", factory: "createDeflateRaw" },
  { name: "inflateRaw", options: "ZlibOptions", factory: "createInflateRaw" },
  { name: "unzip", options: "ZlibOptions", factory: "createUnzip" },
  { name: "brotliCompress", options: "BrotliOptions", factory: "createBrotliCompress" },
  { name: "brotliDecompress", options: "BrotliOptions", factory: "createBrotliDecompress" },
] as const;

export function nodeZlibCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  const calls: NodejsModuleCallTargetMetadata[] = [];
  for (const codec of codecs) {
    const sourceOptions = codec.options === "BrotliOptions" ? brotliOptionsProviderType : optionsProviderType;
    const physicalOptions = nativeNamedType(`Tsonic.CSharp.Node.${codec.options}`);
    const bufferParameter = { name: "buffer", type: bufferProviderType };
    const optionsParameter = { name: "options", type: sourceOptions };
    const callbackParameter = { name: "callback", type: callbackProvider };
    calls.push(
      call(`${codec.name}Sync`, [bufferParameter], bufferProviderType,
        [nativeParameter(0, buffer), nativeDefault(nativeNullableReference(physicalOptions))], buffer),
      call(`${codec.name}Sync`, [bufferParameter, optionsParameter], bufferProviderType,
        [nativeParameter(0, buffer), nativeParameter(1, nativeNullableReference(physicalOptions))], buffer),
      call(codec.name, [bufferParameter, callbackParameter], voidProviderType,
        [nativeParameter(0, buffer), nativeParameter(1, callback)], { kind: "void" }),
      call(codec.name, [bufferParameter, optionsParameter, callbackParameter], voidProviderType,
        [nativeParameter(0, buffer), nativeParameter(1, physicalOptions), nativeParameter(2, callback)], { kind: "void" }),
    );
  }
  for (const codec of codecs) {
    const options = nativeNamedType(`Tsonic.CSharp.Node.${codec.options}`);
    calls.push(call(codec.factory, [{ name: "options", type: codec.options === "BrotliOptions" ? brotliOptionsProviderType : optionsProviderType, optional: true }],
      transformProviderType, [nativeParameter(0, nativeNullableReference(options))], transform));
  }
  return Object.freeze(calls);
}

export function nodeZlibClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const options: readonly (readonly [string, readonly string[]])[] = [
    ["ZlibOptions", ["level", "chunkSize", "maxOutputLength"]], ["BrotliOptions", ["quality", "chunkSize", "maxOutputLength"]],
  ];
  return options.flatMap(([exportName, names]) => names.map(memberName => ({
    exportName, memberName, memberId: `node:zlib.${exportName}.${memberName}`, memberKind: "property" as const,
    providerType: int32ProviderType, optional: true as const,
    native: nativeSlot(nativeOwner(`Tsonic.CSharp.Node.${exportName}`), memberName, nativeNullable({ kind: "source-primitive", name: "int32" })),
  })));
}

export function nodeZlibExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeZlibModuleSpecifier, moduleCalls: nodeZlibCallTargetMembers(),
    classProperties: nodeZlibClassPropertyTargetMembers(), classes: ["ZlibOptions", "BrotliOptions", "ZlibTransform"],
    classHeritage: { ZlibTransform: [providerRef("node:stream", "Transform")] } });
}

function call(
  exportName: string, providerParameters: readonly ProviderParameterDeclaration[], providerReturnType: ProviderTypeExpression,
  parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator,
): NodejsModuleCallTargetMetadata {
  const shape = providerParameters.map(parameter => parameter.name).join(",");
  return { exportName, signatureId: `node:zlib.${exportName}(${shape})`, providerParameters, providerReturnType,
    native: nativeCall({ owner, kind: "method", targetName: exportName, static: true, parameters, returnType, receiver: { kind: "none" } }) };
}

