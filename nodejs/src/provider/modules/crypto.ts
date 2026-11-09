import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import { int32ProviderType } from "../model/source-types.js";
import type { NodejsClassCallTargetMetadata, NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsUnsupportedTargetIdentity } from "../model/members.js";
import type { NodejsNativeParameterMapping } from "../native/authoring.js";
import type { NodejsNativeTypeDeclarationLocator, NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeOwner, nativeParameter } from "../native/authoring.js";
import { nativeArray, nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";
import { nodejsDefaultModuleObjectExports } from "../declarations/defaults.js";

export const nodeCryptoModuleSpecifier = "node:crypto";
export const nodeCryptoRandomUuidExportName = "randomUUID";
export const nodeCryptoRandomUuidSignatureId = "node:crypto.randomUUID()";
const stringProviderType: ProviderTypeExpression = { kind: "string" };
const unknownProviderType: ProviderTypeExpression = { kind: "unknown" };
const boolProviderType: ProviderTypeExpression = { kind: "boolean" };
const bufferProviderType: ProviderTypeExpression = { kind: "provider-ref", moduleSpecifier: "node:buffer", exportName: "Buffer" };
const stringArrayProviderType: ProviderTypeExpression = { kind: "array", elementType: stringProviderType };
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const nativeBool: NodejsNativeTypeLocator = { kind: "source-primitive", name: "bool" };
const nativeBuffer = nativeNamedType("Tsonic.CSharp.Node.Buffer");
const cryptoOwner = nativeOwner("Tsonic.CSharp.Node.crypto");
const hashOwner = nativeOwner("Tsonic.CSharp.Node.Hash");
const hmacOwner = nativeOwner("Tsonic.CSharp.Node.Hmac");
const hashProviderType: ProviderTypeExpression = { kind: "provider-ref", moduleSpecifier: nodeCryptoModuleSpecifier, exportName: "Hash" };
const hmacProviderType: ProviderTypeExpression = { kind: "provider-ref", moduleSpecifier: nodeCryptoModuleSpecifier, exportName: "Hmac" };
const stringParameter = (name: string, optional = false): ProviderParameterDeclaration =>
  ({ name, type: stringProviderType, ...(optional ? { optional: true } : {}) });
const intParameter = (name: string, optional = false): ProviderParameterDeclaration =>
  ({ name, type: int32ProviderType, ...(optional ? { optional: true } : {}) });

export function nodeCryptoCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    call("createHash", "node:crypto.createHash(System.String)", [stringParameter("algorithm")], hashProviderType,
      [nativeParameter(0, nativeString)], nativeNamedType(hashOwner.metadataName)),
    call("createHmac", "node:crypto.createHmac(System.String,System.String)", [stringParameter("algorithm"), stringParameter("key")], hmacProviderType,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeString)], nativeNamedType(hmacOwner.metadataName)),
    call("createHmac", "node:crypto.createHmac(System.String,Tsonic.CSharp.Node.Buffer)", [stringParameter("algorithm"), { name: "key", type: bufferProviderType }], hmacProviderType,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeBuffer)], nativeNamedType(hmacOwner.metadataName)),
    ...["getCiphers", "getCurves", "getHashes"].map(name => call(name, `node:crypto.${name}()`, [], stringArrayProviderType, [], nativeArray(nativeString))),
    call("randomBytes", "node:crypto.randomBytes(System.Int32)", [intParameter("size")], bufferProviderType,
      [nativeParameter(0, nativeInt)], nativeBuffer, "randomBytesBuffer"),
    call("randomFillSync", "node:crypto.randomFillSync(Tsonic.CSharp.Node.Buffer,System.Int32,System.Int32)",
      [{ name: "buffer", type: bufferProviderType }, intParameter("offset", true), intParameter("size", true)], bufferProviderType,
      [nativeParameter(0, nativeBuffer), nativeParameter(1, nativeInt), nativeParameter(2, nativeNullable(nativeInt))], nativeBuffer),
    call("randomInt", "node:crypto.randomInt(System.Int32)", [intParameter("max")], int32ProviderType,
      [nativeParameter(0, nativeInt)], nativeInt),
    call("randomInt", "node:crypto.randomInt(System.Int32,System.Int32)", [intParameter("min"), intParameter("max")], int32ProviderType,
      [nativeParameter(0, nativeInt), nativeParameter(1, nativeInt)], nativeInt),
    call(nodeCryptoRandomUuidExportName, nodeCryptoRandomUuidSignatureId, [], stringProviderType, [], nativeString),
    call("timingSafeEqual", "node:crypto.timingSafeEqual(Tsonic.CSharp.Node.Buffer,Tsonic.CSharp.Node.Buffer)",
      [{ name: "a", type: bufferProviderType }, { name: "b", type: bufferProviderType }], boolProviderType,
      [nativeParameter(0, nativeBuffer), nativeParameter(1, nativeBuffer)], nativeBool),
  ];
}

export function nodeCryptoClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const groups: readonly (readonly [string, NodejsNativeTypeDeclarationLocator, ProviderTypeExpression])[] = [
    ["Hash", hashOwner, hashProviderType], ["Hmac", hmacOwner, hmacProviderType],
  ];
  return groups.flatMap(([exportName, owner, result]) => [
    classCall(exportName, owner, "update", `node:crypto.${exportName}.update(System.String,System.String)`,
      [stringParameter("data"), stringParameter("inputEncoding", true)], result,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeNullableReference(nativeString))], nativeNamedType(owner.metadataName)),
    classCall(exportName, owner, "update", `node:crypto.${exportName}.update(Tsonic.CSharp.Node.Buffer)`,
      [{ name: "data", type: bufferProviderType }], result, [nativeParameter(0, nativeBuffer)], nativeNamedType(owner.metadataName)),
    classCall(exportName, owner, "digest", `node:crypto.${exportName}.digest(System.String)`,
      [stringParameter("encoding")], stringProviderType, [nativeParameter(0, nativeNullableReference(nativeString))], nativeString),
    classCall(exportName, owner, "digest", `node:crypto.${exportName}.digest()`, [], bufferProviderType, [], nativeBuffer, "digestBuffer"),
    ...(exportName === "Hash" ? [classCall(exportName, owner, "copy", "node:crypto.Hash.copy()", [], result, [], nativeNamedType(owner.metadataName))] : []),
  ]);
}

export function nodeCryptoExports(): readonly ProviderExportDeclaration[] {
  const calls = new Map<string, NodejsModuleCallTargetMetadata[]>();
  for (const entry of nodeCryptoCallTargetMembers()) {
    const entries = calls.get(entry.exportName);
    if (entries === undefined) calls.set(entry.exportName, [entry]);
    else entries.push(entry);
  }
  const classCalls = nodeCryptoClassCallTargetMembers();
  const exports: readonly ProviderExportDeclaration[] = [
    ...["Hash", "Hmac"].map(exportName => {
      const members = new Map<string, NodejsClassCallTargetMetadata[]>();
      for (const entry of classCalls) {
        if (entry.exportName !== exportName) continue;
        const entries = members.get(entry.memberId);
        if (entries === undefined) members.set(entry.memberId, [entry]);
        else entries.push(entry);
      }
      return { id: `node:crypto.${exportName}`, name: exportName, kind: "class" as const,
        members: [...members.values()].map(entries => {
          const first = entries[0]!;
          return { id: first.memberId, name: first.memberName, kind: first.memberKind,
            signatures: entries.map(entry => ({ id: entry.signatureId, parameters: entry.providerParameters, returnType: entry.providerReturnType })) };
        }) };
    }),
    ...nodeCryptoUnsupportedCalls.map(entry => ({ id: `node:crypto.${entry.exportName}`, name: entry.exportName, kind: "function" as const,
      signatures: [{ id: entry.signatureId, parameters: entry.providerParameters, returnType: entry.providerReturnType }] })),
    ...[...calls].map(([exportName, entries]) => ({ id: `node:crypto.${exportName}`, name: exportName, kind: "function" as const,
      signatures: entries.map(entry => ({ id: entry.signatureId, parameters: entry.providerParameters, returnType: entry.providerReturnType })) })),
  ];
  return [...exports, ...nodejsDefaultModuleObjectExports(nodeCryptoModuleSpecifier, exports)];
}

export function nodeCryptoUnsupportedTargetIdentities(): readonly NodejsUnsupportedTargetIdentity[] {
  return nodeCryptoUnsupportedCalls.map(({ exportName, signatureId, targetIdentityId, displayName }) =>
    ({ exportName, signatureId, targetIdentityId, displayName }));
}

function call(
  exportName: string, signatureId: string, providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression, parameters: readonly NodejsNativeParameterMapping[],
  returnType: NodejsNativeTypeLocator, targetName = exportName,
): NodejsModuleCallTargetMetadata {
  return Object.freeze({ exportName, signatureId, providerParameters, providerReturnType,
    native: nativeCall({ owner: cryptoOwner, kind: "method", targetName, static: true, parameters, returnType, receiver: { kind: "none" } }) });
}

function classCall(
  exportName: string, owner: NodejsNativeTypeDeclarationLocator, memberName: string, signatureId: string,
  providerParameters: readonly ProviderParameterDeclaration[], providerReturnType: ProviderTypeExpression,
  parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator, targetName = memberName,
): NodejsClassCallTargetMetadata {
  return Object.freeze({ exportName, memberName, memberId: `node:crypto.${exportName}.${memberName}`, signatureId, memberKind: "method",
    providerParameters, providerReturnType,
    native: nativeCall({ owner, kind: "method", targetName, static: false, parameters, returnType, receiver: { kind: "instance" } }) });
}

function optionalUnknownParameter(name: string): ProviderParameterDeclaration {
  return { name, type: unknownProviderType, optional: true };
}

const nodeCryptoUnsupportedCalls = [
  {
    exportName: "createCipheriv",
    signatureId: "node:crypto.createCipheriv(System.String,System.Object,System.Object)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.crypto.createCipheriv(System.String,System.Object,System.Object)",
    displayName: "unsupported NodeJS crypto.createCipheriv",
    providerParameters: [
      { name: "algorithm", type: stringProviderType },
      { name: "key", type: unknownProviderType },
      { name: "iv", type: unknownProviderType },
    ],
    providerReturnType: unknownProviderType,
  },
  {
    exportName: "createDecipheriv",
    signatureId: "node:crypto.createDecipheriv(System.String,System.Object,System.Object)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.crypto.createDecipheriv(System.String,System.Object,System.Object)",
    displayName: "unsupported NodeJS crypto.createDecipheriv",
    providerParameters: [
      { name: "algorithm", type: stringProviderType },
      { name: "key", type: unknownProviderType },
      { name: "iv", type: unknownProviderType },
    ],
    providerReturnType: unknownProviderType,
  },
  {
    exportName: "scryptSync",
    signatureId: "node:crypto.scryptSync(System.Object,System.Object,System.Int32,System.Object)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.crypto.scryptSync(System.Object,System.Object,System.Int32,System.Object)",
    displayName: "unsupported NodeJS crypto.scryptSync",
    providerParameters: [
      { name: "password", type: unknownProviderType },
      { name: "salt", type: unknownProviderType },
      { name: "keylen", type: int32ProviderType },
      optionalUnknownParameter("options"),
    ],
    providerReturnType: bufferProviderType,
  },
  {
    exportName: "pbkdf2Sync",
    signatureId: "node:crypto.pbkdf2Sync(System.Object,System.Object,System.Int32,System.Int32,System.String)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.crypto.pbkdf2Sync(System.Object,System.Object,System.Int32,System.Int32,System.String)",
    displayName: "unsupported NodeJS crypto.pbkdf2Sync",
    providerParameters: [
      { name: "password", type: unknownProviderType },
      { name: "salt", type: unknownProviderType },
      { name: "iterations", type: int32ProviderType },
      { name: "keylen", type: int32ProviderType },
      { name: "digest", type: stringProviderType },
    ],
    providerReturnType: bufferProviderType,
  },
  {
    exportName: "createSign",
    signatureId: "node:crypto.createSign(System.String)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.crypto.createSign(System.String)",
    displayName: "unsupported NodeJS crypto.createSign",
    providerParameters: [{ name: "algorithm", type: stringProviderType }],
    providerReturnType: unknownProviderType,
  },
  {
    exportName: "createVerify",
    signatureId: "node:crypto.createVerify(System.String)",
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.crypto.createVerify(System.String)",
    displayName: "unsupported NodeJS crypto.createVerify",
    providerParameters: [{ name: "algorithm", type: stringProviderType }],
    providerReturnType: unknownProviderType,
  },
] satisfies readonly {
  readonly exportName: string;
  readonly signatureId: string;
  readonly targetIdentityId: string;
  readonly displayName: string;
  readonly providerParameters: readonly ProviderParameterDeclaration[];
  readonly providerReturnType: ProviderTypeExpression;
}[];

