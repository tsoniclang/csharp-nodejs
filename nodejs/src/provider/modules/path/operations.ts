import type { ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsNativeParameterMapping } from "../../native/authoring.js";
import type { NodejsNativeTypeLocator } from "../../native/locators.js";
import { nativeParameter } from "../../native/authoring.js";
import { nativeArray, nativeNullableReference } from "../../native/locators.js";
import { boolProviderType, nativeParsedPath, parsedPathProviderType, stringProviderType } from "./types.js";

interface PathOperation {
  readonly name: string;
  readonly signature: string;
  readonly sourceParameters: readonly ProviderParameterDeclaration[];
  readonly sourceReturn: ProviderTypeExpression;
  readonly parameters: readonly NodejsNativeParameterMapping[];
  readonly returnType: NodejsNativeTypeLocator;
}
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeBool: NodejsNativeTypeLocator = { kind: "source-primitive", name: "bool" };
const stringParameter = (name: string, optional = false): ProviderParameterDeclaration =>
  ({ name, type: stringProviderType, ...(optional ? { optional: true } : {}) });
const paths: ProviderParameterDeclaration = { name: "paths", type: { kind: "array", elementType: stringProviderType }, rest: true };

export const pathOperations: readonly PathOperation[] = Object.freeze([
  { name: "basename", signature: "(System.String,System.String)", sourceParameters: [stringParameter("path"), stringParameter("suffix", true)],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeString), nativeParameter(1, nativeNullableReference(nativeString))], returnType: nativeString },
  ...["dirname", "extname"].map(name => ({ name, signature: "(System.String)", sourceParameters: [stringParameter("path")],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeString)], returnType: nativeString })),
  { name: "isAbsolute", signature: "(System.String)", sourceParameters: [stringParameter("path")],
    sourceReturn: boolProviderType, parameters: [nativeParameter(0, nativeString)], returnType: nativeBool },
  { name: "join", signature: "(System.String[])", sourceParameters: [paths],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeArray(nativeString))], returnType: nativeString },
  { name: "matchesGlob", signature: "(System.String,System.String)", sourceParameters: [stringParameter("path"), stringParameter("pattern")],
    sourceReturn: boolProviderType, parameters: [nativeParameter(0, nativeString), nativeParameter(1, nativeString)], returnType: nativeBool },
  { name: "normalize", signature: "(System.String)", sourceParameters: [stringParameter("path")],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeString)], returnType: nativeString },
  { name: "parse", signature: "(System.String)", sourceParameters: [stringParameter("path")],
    sourceReturn: parsedPathProviderType, parameters: [nativeParameter(0, nativeString)], returnType: nativeParsedPath },
  { name: "relative", signature: "(System.String,System.String)", sourceParameters: [stringParameter("from"), stringParameter("to")],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeString), nativeParameter(1, nativeString)], returnType: nativeString },
  { name: "resolve", signature: "(System.String[])", sourceParameters: [paths],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeArray(nativeString))], returnType: nativeString },
  { name: "toNamespacedPath", signature: "(System.String)", sourceParameters: [stringParameter("path")],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeString)], returnType: nativeString },
  { name: "format", signature: "(Tsonic.CSharp.Node.ParsedPath)", sourceParameters: [{ name: "pathObject", type: parsedPathProviderType }],
    sourceReturn: stringProviderType, parameters: [nativeParameter(0, nativeParsedPath)], returnType: nativeString },
]);

