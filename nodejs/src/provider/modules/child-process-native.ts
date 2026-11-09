import type { NodejsNativeCallMapping, NodejsNativeCallableLocator, NodejsNativeClassApplicationRecipe, NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeOwner } from "../native/authoring.js";
import { nativeArray, nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";

export const spawnReturnsDeclaration = nativeOwner("Tsonic.CSharp.Node.SpawnSyncReturns`1", 1);
export const spawnOptionsDeclaration = nativeOwner("Tsonic.CSharp.Node.SpawnSyncOptions`2", 2);
export const spawnErrorDeclaration = nativeOwner("Tsonic.CSharp.Node.SpawnSyncError");
const moduleDeclaration = nativeOwner("Tsonic.CSharp.Node.child_process");
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const buffer = nativeNamedType("Tsonic.CSharp.Node.Buffer");
const stdioDescriptor = nativeNullable(nativeNamedType("Tsonic.CSharp.Runtime.Union`2",
  { kind: "source-primitive", name: "float64" }, nativeString));

export function spawnSyncResultLocator(includeJsSurface: boolean): NodejsNativeCallableLocator {
  const argumentsType = includeJsSurface ? nativeNamedType("Tsonic.CSharp.Js.JSArray`1", nativeString) : nativeArray(nativeString);
  const input = includeJsSurface
    ? nativeNullable(nativeNamedType("Tsonic.CSharp.Runtime.Union`2", nativeNamedType("Tsonic.CSharp.Js.Uint8Array"), buffer))
    : buffer;
  const stdio = includeJsSurface ? nativeNamedType("Tsonic.CSharp.Js.JSArray`1", stdioDescriptor) : nativeArray(stdioDescriptor);
  return Object.freeze({
    owner: moduleDeclaration, kind: "method", targetName: "spawnSyncResult", static: true, typeParameterCount: 0,
    parameters: Object.freeze([
      { type: nativeString, passingMode: "by-value" as const },
      { type: argumentsType, passingMode: "by-value" as const },
      { type: nativeNullableReference(nativeNamedType(spawnOptionsDeclaration.metadataName, input, stdio)), passingMode: "by-value" as const },
    ]),
    returnType: nativeNamedType(spawnReturnsDeclaration.metadataName, buffer), returnPassing: "by-value",
  });
}

export function spawnSyncNativeMapping(includeJsSurface: boolean, withOptions: boolean): NodejsNativeCallMapping {
  return Object.freeze({
    selected: spawnSyncResultLocator(includeJsSurface), receiver: Object.freeze({ kind: "none" }),
    parameters: Object.freeze([
      { kind: "source" as const, sourceParameterIndex: 0, targetParameterIndex: 0 },
      { kind: "source" as const, sourceParameterIndex: 1, targetParameterIndex: 1 },
      withOptions ? { kind: "source" as const, sourceParameterIndex: 2, targetParameterIndex: 2 }
        : { kind: "native-default" as const, targetParameterIndex: 2 },
    ]),
    methodTypeParameters: Object.freeze([]), invocationTypeParameters: Object.freeze([]),
  });
}

export function spawnOptionsApplication(includeJsSurface: boolean): NodejsNativeClassApplicationRecipe {
  return Object.freeze({ kind: "native-parameter-application", original: spawnOptionsDeclaration,
    selected: spawnSyncResultLocator(includeJsSurface), targetParameterIndex: 2, referenceAnnotation: "required" });
}

export const spawnReturnsApplication: NodejsNativeClassApplicationRecipe = Object.freeze({
  kind: "source-class", original: spawnReturnsDeclaration,
  parameters: Object.freeze([{ sourceTypeParameterIndex: 0, targetTypeParameterIndex: 0 }]),
});
