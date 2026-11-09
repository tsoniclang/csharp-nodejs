import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsModuleCallTargetMetadata } from "../model/target-members.js";
import type { NodejsUnsupportedTargetIdentity } from "../model/members.js";
import { nativeCall, nativeOwner, nativeParameter } from "../native/authoring.js";
import { nativeNullableReference } from "../native/locators.js";
import { nodejsDefaultModuleObjectExports } from "../declarations/defaults.js";

export const nodeAssertModuleSpecifier = "node:assert";
export const nodeAssertOkExportName = "ok";
export const nodeAssertOkSignatureId = "node:assert.ok(System.Boolean,System.String)";
export const nodeAssertFailExportName = "fail";
export const nodeAssertFailSignatureId = "node:assert.fail(System.String)";
export const nodeAssertStrictEqualExportName = "strictEqual";
export const nodeAssertStrictEqualSignatureId = "node:assert.strictEqual(System.Object,System.Object,System.String)";
export const nodeAssertNotStrictEqualExportName = "notStrictEqual";
export const nodeAssertNotStrictEqualSignatureId = "node:assert.notStrictEqual(System.Object,System.Object,System.String)";
export const nodeAssertEqualExportName = "equal";
export const nodeAssertEqualSignatureId = "node:assert.equal(System.Object,System.Object,System.String)";
export const nodeAssertNotEqualExportName = "notEqual";
export const nodeAssertNotEqualSignatureId = "node:assert.notEqual(System.Object,System.Object,System.String)";
export const nodeAssertDeepStrictEqualExportName = "deepStrictEqual";
export const nodeAssertDeepStrictEqualSignatureId = "node:assert.deepStrictEqual(System.Object,System.Object,System.String)";
export const nodeAssertThrowsExportName = "throws";
export const nodeAssertThrowsSignatureId = "node:assert.throws(Function,System.String)";
export const nodeAssertMatchExportName = "match";
export const nodeAssertMatchSignatureId = "node:assert.match(System.String,System.Object,System.String)";
export const nodeAssertIfErrorExportName = "ifError";
export const nodeAssertIfErrorSignatureId = "node:assert.ifError(System.Object)";
const owner = nativeOwner("Tsonic.CSharp.Node.assert");
const boolProviderType: ProviderTypeExpression = { kind: "boolean" };
const stringProviderType: ProviderTypeExpression = { kind: "string" };
const unknownProviderType: ProviderTypeExpression = { kind: "unknown" };
const voidProviderType: ProviderTypeExpression = { kind: "void" };
const callbackProviderType: ProviderTypeExpression = { kind: "function", id: "node:assert.callback", parameters: [], returnType: unknownProviderType };
const message = nativeNullableReference({ kind: "string" });
const payload = nativeNullableReference({ kind: "object" });

export function nodeAssertCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  const equality = [
    [nodeAssertStrictEqualExportName, nodeAssertStrictEqualSignatureId],
    [nodeAssertNotStrictEqualExportName, nodeAssertNotStrictEqualSignatureId],
  ] as const;
  return [{
    exportName: nodeAssertOkExportName, signatureId: nodeAssertOkSignatureId,
    providerParameters: [{ name: "value", type: boolProviderType }, stringParameter("message", true)], providerReturnType: voidProviderType,
    native: nativeCall({ owner, kind: "method", targetName: "ok", static: true,
      parameters: [nativeParameter(0, { kind: "source-primitive", name: "bool" }), nativeParameter(1, message)],
      returnType: { kind: "void" }, receiver: { kind: "none" } }),
  }, {
    exportName: nodeAssertFailExportName, signatureId: nodeAssertFailSignatureId,
    providerParameters: [stringParameter("message", true)], providerReturnType: voidProviderType,
    native: nativeCall({ owner, kind: "method", targetName: "fail", static: true,
      parameters: [nativeParameter(0, message)], returnType: { kind: "void" }, receiver: { kind: "none" } }),
  }, ...equality.map(([exportName, signatureId]) => ({
    exportName, signatureId, providerParameters: [unknownParameter("actual"), unknownParameter("expected"), stringParameter("message", true)],
    providerReturnType: voidProviderType,
    native: nativeCall({ owner, kind: "method", targetName: exportName, static: true,
      parameters: [nativeParameter(0, payload), nativeParameter(1, payload), nativeParameter(2, message)],
      returnType: { kind: "void" }, receiver: { kind: "none" } }),
  }))];
}

export function nodeAssertExports(): readonly ProviderExportDeclaration[] {
  const exports: readonly ProviderExportDeclaration[] = [
    ...nodeAssertUnsupportedCalls.map(entry => ({ id: `node:assert.${entry.exportName}`, name: entry.exportName, kind: "function" as const,
      signatures: [{ id: entry.signatureId, parameters: entry.parameters, returnType: voidProviderType }] })),
    ...nodeAssertCallTargetMembers().map(entry => ({ id: `node:assert.${entry.exportName}`, name: entry.exportName, kind: "function" as const,
      signatures: [{ id: entry.signatureId, parameters: entry.providerParameters, returnType: entry.providerReturnType }] })),
  ];
  return [...exports, ...nodejsDefaultModuleObjectExports(nodeAssertModuleSpecifier, exports)];
}

export function nodeAssertUnsupportedTargetIdentities(): readonly NodejsUnsupportedTargetIdentity[] {
  return nodeAssertUnsupportedCalls.map(({ exportName, signatureId, targetIdentityId, displayName }) => ({ exportName, signatureId, targetIdentityId, displayName }));
}

function stringParameter(name: string, optional = false): ProviderParameterDeclaration {
  return { name, type: stringProviderType, ...(optional ? { optional: true } : {}) };
}
function unknownParameter(name: string, optional = false): ProviderParameterDeclaration {
  return { name, type: unknownProviderType, ...(optional ? { optional: true } : {}) };
}
function callbackParameter(name: string): ProviderParameterDeclaration {
  return { name, type: callbackProviderType };
}
const equalityAssertParameters = [unknownParameter("actual"), unknownParameter("expected"), stringParameter("message", true)];
const nodeAssertUnsupportedCalls = [
  {
    exportName: nodeAssertEqualExportName,
    signatureId: nodeAssertEqualSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.assert.equal(System.Object,System.Object,System.String)",
    displayName: "unsupported NodeJS assert.equal",
    parameters: equalityAssertParameters,
  },
  {
    exportName: nodeAssertNotEqualExportName,
    signatureId: nodeAssertNotEqualSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.assert.notEqual(System.Object,System.Object,System.String)",
    displayName: "unsupported NodeJS assert.notEqual",
    parameters: equalityAssertParameters,
  },
  {
    exportName: nodeAssertDeepStrictEqualExportName,
    signatureId: nodeAssertDeepStrictEqualSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.assert.deepStrictEqual(System.Object,System.Object,System.String)",
    displayName: "unsupported NodeJS assert.deepStrictEqual",
    parameters: equalityAssertParameters,
  },
  {
    exportName: nodeAssertThrowsExportName,
    signatureId: nodeAssertThrowsSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.assert.throws(Function,System.String)",
    displayName: "unsupported NodeJS assert.throws",
    parameters: [
      callbackParameter("fn"),
      stringParameter("message", true),
    ],
  },
  {
    exportName: nodeAssertMatchExportName,
    signatureId: nodeAssertMatchSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.assert.match(System.String,System.Object,System.String)",
    displayName: "unsupported NodeJS assert.match",
    parameters: [
      stringParameter("string"),
      unknownParameter("regexp"),
      stringParameter("message", true),
    ],
  },
  {
    exportName: nodeAssertIfErrorExportName,
    signatureId: nodeAssertIfErrorSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.assert.ifError(System.Object)",
    displayName: "unsupported NodeJS assert.ifError",
    parameters: [
      unknownParameter("value"),
    ],
  },
] satisfies readonly {
  readonly exportName: string;
  readonly signatureId: string;
  readonly targetIdentityId: string;
  readonly displayName: string;
  readonly parameters: readonly ProviderParameterDeclaration[];
}[];

