import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsModuleCallTargetMetadata } from "../../model/target-members.js";
import type { NodejsUnsupportedTargetIdentity } from "../../model/members.js";
import { nativeCall, nativeDefault, nativeOwner, nativeParameter } from "../../native/authoring.js";
import { nativeNamedType, nativeNullableReference } from "../../native/locators.js";
import { int32ProviderType } from "../../model/source-types.js";
import { nodeUtilTextDecoderExportDeclaration } from "./text-decoder.js";
import { nodeErrorExportDeclaration } from "./node-error.js";
import { nodejsDefaultModuleObjectExports } from "../../declarations/defaults.js";

const unknownProviderType: ProviderTypeExpression = { kind: "unknown" };
const stringProviderType: ProviderTypeExpression = { kind: "string" };
const boolProviderType: ProviderTypeExpression = { kind: "boolean" };
const voidProviderType: ProviderTypeExpression = { kind: "void" };
const owner = nativeOwner("Tsonic.CSharp.Node.util");
export const nodeUtilModuleSpecifier = "node:util";
export const nodeUtilFormatExportName = "format";
export const nodeUtilFormatSignatureId = "node:util.format(System.Object,System.Object[])";
export const nodeUtilFormatWithOptionsExportName = "formatWithOptions";
export const nodeUtilFormatWithOptionsSignatureId = "node:util.formatWithOptions(System.Object,System.Object,System.Object[])";
export const nodeUtilInspectExportName = "inspect";
export const nodeUtilInspectSignatureId = "node:util.inspect(System.Object)";
export const nodeUtilDebuglogExportName = "debuglog";
export const nodeUtilDebuglogSignatureId = "node:util.debuglog(System.String)";
export const nodeUtilDeprecateExportName = "deprecate";
export const nodeUtilDeprecateSignatureId = "node:util.deprecate(Function,System.String,System.String)";
export const nodeUtilIsDeepStrictEqualExportName = "isDeepStrictEqual";
export const nodeUtilIsDeepStrictEqualSignatureId = "node:util.isDeepStrictEqual(System.Object,System.Object)";
export const nodeUtilStripVtControlCharactersExportName = "stripVTControlCharacters";
export const nodeUtilStripVtControlCharactersSignatureId = "node:util.stripVTControlCharacters(System.String)";
export const nodeUtilToUsvStringExportName = "toUSVString";
export const nodeUtilToUsvStringSignatureId = "node:util.toUSVString(System.String)";


export function nodeUtilCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    { exportName: "convertProcessSignalToExitCode", signatureId: "node:util.convertProcessSignalToExitCode(System.String)",
      providerParameters: [stringParameter("signal")], providerReturnType: int32ProviderType,
      native: nativeCall({ owner, kind: "method", targetName: "convertProcessSignalToExitCode", static: true,
        parameters: [nativeParameter(0, { kind: "string" })], returnType: { kind: "source-primitive", name: "int32" }, receiver: { kind: "none" } }) },
    ...["getSystemErrorMessage", "getSystemErrorName"].map(exportName => ({
      exportName, signatureId: `node:util.${exportName}(System.Int32)`,
      providerParameters: [{ name: "err", type: int32ProviderType }], providerReturnType: stringProviderType,
      native: nativeCall({ owner, kind: "method", targetName: exportName, static: true,
        parameters: [nativeParameter(0, { kind: "source-primitive", name: "int32" })], returnType: { kind: "string" }, receiver: { kind: "none" } }),
    })),
    { exportName: "styleText", signatureId: "node:util.styleText(System.String,System.String)",
      providerParameters: [stringParameter("format"), stringParameter("text")], providerReturnType: stringProviderType,
      native: nativeCall({ owner, kind: "method", targetName: "styleText", static: true,
        parameters: [nativeParameter(0, { kind: "string" }), nativeParameter(1, { kind: "string" }),
          nativeDefault(nativeNullableReference(nativeNamedType("Tsonic.CSharp.Node.StyleTextOptions")))],
        returnType: { kind: "string" }, receiver: { kind: "none" } }) },
    { exportName: nodeUtilStripVtControlCharactersExportName, signatureId: nodeUtilStripVtControlCharactersSignatureId,
      providerParameters: [stringParameter("str")], providerReturnType: stringProviderType,
      native: nativeCall({ owner, kind: "method", targetName: "stripVTControlCharacters", static: true,
        parameters: [nativeParameter(0, { kind: "string" })], returnType: { kind: "string" }, receiver: { kind: "none" } }) },
    { exportName: nodeUtilToUsvStringExportName, signatureId: nodeUtilToUsvStringSignatureId,
      providerParameters: [stringParameter("string")], providerReturnType: stringProviderType,
      native: nativeCall({ owner, kind: "method", targetName: "toUSVString", static: true,
        parameters: [nativeParameter(0, { kind: "string" })], returnType: { kind: "string" }, receiver: { kind: "none" } }) },
  ];
}

export function nodeUtilExports(): readonly ProviderExportDeclaration[] {
  const exports: readonly ProviderExportDeclaration[] = [
    nodeErrorExportDeclaration(), nodeUtilTextDecoderExportDeclaration(),
    ...nodeUtilUnsupportedCalls.map(entry => ({ id: `node:util.${entry.exportName}`, name: entry.exportName, kind: "function" as const,
      signatures: [{ id: entry.signatureId, parameters: entry.parameters, returnType: entry.returnType }] })),
    ...nodeUtilCallTargetMembers().map(entry => ({ id: `node:util.${entry.exportName}`, name: entry.exportName, kind: "function" as const,
      signatures: [{ id: entry.signatureId, parameters: entry.providerParameters, returnType: entry.providerReturnType }] })),
  ];
  return [...exports, ...nodejsDefaultModuleObjectExports(nodeUtilModuleSpecifier, exports)];
}

export function nodeUtilUnsupportedTargetIdentities(): readonly NodejsUnsupportedTargetIdentity[] {
  return nodeUtilUnsupportedCalls.map(({ exportName, signatureId, targetIdentityId, displayName }) => ({ exportName, signatureId, targetIdentityId, displayName }));
}
function unknownParameter(name: string, optional = false): ProviderParameterDeclaration {
  return {
    name,
    type: unknownProviderType,
    ...(optional ? { optional: true } : {}),
  };
}

function stringParameter(name: string, optional = false): ProviderParameterDeclaration {
  return {
    name,
    type: stringProviderType,
    ...(optional ? { optional: true } : {}),
  };
}

function callbackParameter(name: string, id: string): ProviderParameterDeclaration {
  return {
    name,
    type: callbackProviderType(id),
  };
}

function callbackProviderType(id: string): ProviderTypeExpression {
  return {
    kind: "function",
    id,
    parameters: [{ name: "args", type: { kind: "array", elementType: unknownProviderType }, rest: true }],
    returnType: voidProviderType,
  };
}

function unknownRestParameter(name: string): ProviderParameterDeclaration {
  return {
    name,
    type: { kind: "array", elementType: unknownProviderType },
    rest: true,
  };
}

const nodeUtilUnsupportedCalls = [
  {
    exportName: nodeUtilFormatExportName,
    signatureId: nodeUtilFormatSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.util.format(System.Object,System.Object[])",
    displayName: "unsupported NodeJS util.format",
    parameters: [
      unknownParameter("format", true),
      unknownRestParameter("args"),
    ],
    returnType: stringProviderType,
  },
  {
    exportName: nodeUtilFormatWithOptionsExportName,
    signatureId: nodeUtilFormatWithOptionsSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.util.formatWithOptions(System.Object,System.Object,System.Object[])",
    displayName: "unsupported NodeJS util.formatWithOptions",
    parameters: [
      unknownParameter("inspectOptions"),
      unknownParameter("formatValue"),
      unknownRestParameter("args"),
    ],
    returnType: stringProviderType,
  },
  {
    exportName: nodeUtilInspectExportName,
    signatureId: nodeUtilInspectSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.util.inspect(System.Object)",
    displayName: "unsupported NodeJS util.inspect",
    parameters: [
      unknownParameter("object"),
    ],
    returnType: stringProviderType,
  },
  {
    exportName: nodeUtilDebuglogExportName,
    signatureId: nodeUtilDebuglogSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.util.debuglog(System.String)",
    displayName: "unsupported NodeJS util.debuglog",
    parameters: [
      stringParameter("section"),
    ],
    returnType: callbackProviderType("node:util.debuglog.callback-result"),
  },
  {
    exportName: nodeUtilDeprecateExportName,
    signatureId: nodeUtilDeprecateSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.util.deprecate(Function,System.String,System.String)",
    displayName: "unsupported NodeJS util.deprecate",
    parameters: [
      callbackParameter("fn", "node:util.deprecate.callback-parameter"),
      stringParameter("msg"),
      stringParameter("code", true),
    ],
    returnType: callbackProviderType("node:util.deprecate.callback-result"),
  },
  {
    exportName: nodeUtilIsDeepStrictEqualExportName,
    signatureId: nodeUtilIsDeepStrictEqualSignatureId,
    targetIdentityId: "unsupported:Tsonic.CSharp.Node.util.isDeepStrictEqual(System.Object,System.Object)",
    displayName: "unsupported NodeJS util.isDeepStrictEqual",
    parameters: [
      unknownParameter("val1"),
      unknownParameter("val2"),
    ],
    returnType: boolProviderType,
  },
] satisfies readonly {
  readonly exportName: string;
  readonly signatureId: string;
  readonly targetIdentityId: string;
  readonly displayName: string;
  readonly parameters: readonly ProviderParameterDeclaration[];
  readonly returnType: ProviderTypeExpression;
}[];


