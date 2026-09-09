import type {
  ProviderExportDeclaration,
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
import type {
  TargetParameter,
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  booleanProviderType,
  callbackProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  numberProviderType,
  providerRef,
  stringProviderType,
  voidProviderType,
} from "./declarations.js";
import {
  nodejsClassCallTargetMetadata,
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
} from "../members/target-member-metadata.js";

export const nodeReadlineModuleSpecifier = "node:readline";

const targetTypes = {
  Interface: nodejsTargetNamedType("Tsonic.CSharp.Node", "Interface"),
  ReadLineOptions: nodejsTargetNamedType("Tsonic.CSharp.Node", "InterfaceOptions"),
  Readable: nodejsTargetNamedType("Tsonic.CSharp.Node", "Readable"),
  Writable: nodejsTargetNamedType("Tsonic.CSharp.Node", "Writable"),
};
const readlineTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "readline");
const interfaceProviderType = providerRef(nodeReadlineModuleSpecifier, "Interface");
const optionsProviderType = providerRef(nodeReadlineModuleSpecifier, "ReadLineOptions");
const stringTargetType = csharpStringTargetType();
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const nullableBoolTargetType = csharpNullableValueTargetType(boolTargetType);
const voidTargetType = csharpVoidTargetType();
const questionCallbackProviderType = callbackProviderType("node:readline.question-callback", [
  { name: "answer", type: stringProviderType },
]);
const questionCallbackTargetType = csharpDelegateTargetType("System.Action", [stringTargetType]);

export function nodeReadlineExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeReadlineModuleSpecifier,
    moduleCalls: nodeReadlineCallTargetMembers(),
    classCalls: nodeReadlineClassCallTargetMembers(),
    classProperties: nodeReadlineClassPropertyTargetMembers(),
    classes: ["Interface", "ReadLineOptions"],
  });
}

export function nodeReadlineCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [nodejsModuleCallTargetMetadata({
    exportName: "createInterface",
    signatureId: "node:readline.createInterface(InterfaceOptions)",
    targetMemberId: "Tsonic.CSharp.Node.readline.createInterface(Tsonic.CSharp.Node.InterfaceOptions)",
    sourceName: "createInterface",
    targetName: "createInterface",
    providerParameters: [{ name: "options", type: optionsProviderType }],
    providerReturnType: interfaceProviderType,
    targetParameters: [targetParameter("options", targetTypes.ReadLineOptions)],
    targetReturnType: targetTypes.Interface,
    declaringType: readlineTargetType,
  })];
}

export function nodeReadlineClassCallTargetMembers(): readonly NodejsClassCallTargetMetadata[] {
  const method = (
    memberName: string,
    providerParameters: NodejsClassCallTargetMetadata["providerParameters"],
    providerReturnType: ProviderTypeExpression,
    targetParameters: readonly TargetParameter[],
    targetReturnType: TargetTypeRef,
  ): NodejsClassCallTargetMetadata => nodejsClassCallTargetMetadata({
    exportName: "Interface",
    memberName,
    memberId: `node:readline.Interface.${memberName}`,
    signatureId: `node:readline.Interface.${memberName}(${providerParameters.map((parameter) => parameter.type.kind).join(",")})`,
    targetMemberId: `Tsonic.CSharp.Node.Interface.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "method",
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType: targetTypes.Interface,
  });
  return [
    method(
      "question",
      [{ name: "query", type: stringProviderType }, { name: "callback", type: questionCallbackProviderType }],
      voidProviderType,
      [targetParameter("query", stringTargetType), targetParameter("callback", questionCallbackTargetType)],
      voidTargetType,
    ),
    method("write", [{ name: "data", type: stringProviderType }], voidProviderType, [targetParameter("data", stringTargetType)], voidTargetType),
    method("pause", [], interfaceProviderType, [], targetTypes.Interface),
    method("resume", [], interfaceProviderType, [], targetTypes.Interface),
    method("close", [], voidProviderType, [], voidTargetType),
    method("setPrompt", [{ name: "prompt", type: stringProviderType }], voidProviderType, [targetParameter("prompt", stringTargetType)], voidTargetType),
    method("getPrompt", [], stringProviderType, [], stringTargetType),
    method("prompt", [], voidProviderType, [], voidTargetType),
  ];
}

export function nodeReadlineClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const rows: readonly [string, string, ProviderTypeExpression, TargetTypeRef, boolean, boolean][] = [
    ["Interface", "line", stringProviderType, stringTargetType, false, true],
    ["Interface", "cursor", numberProviderType, intTargetType, false, true],
    ["ReadLineOptions", "input", providerRef("node:stream", "Readable"), targetTypes.Readable, false, false],
    ["ReadLineOptions", "output", providerRef("node:stream", "Writable"), targetTypes.Writable, true, false],
    ["ReadLineOptions", "terminal", booleanProviderType, nullableBoolTargetType, true, false],
    ["ReadLineOptions", "prompt", stringProviderType, nullableStringTargetType, true, false],
    ["ReadLineOptions", "historySize", numberProviderType, nullableIntTargetType, true, false],
    ["ReadLineOptions", "removeHistoryDuplicates", booleanProviderType, nullableBoolTargetType, true, false],
  ];
  return rows.map(([exportName, memberName, providerType, targetType, optional, readonly]) =>
    nodejsClassPropertyTargetMetadata({
      exportName,
      memberName,
      memberId: `node:readline.${exportName}.${memberName}`,
      targetMemberId: `Tsonic.CSharp.Node.${exportName === "ReadLineOptions" ? "InterfaceOptions" : exportName}.${memberName}`,
      sourceName: memberName,
      targetName: memberName,
      memberKind: "property",
      providerType,
      targetParameters: [],
      targetReturnType: targetType,
      declaringType: targetTypes[exportName as "Interface" | "ReadLineOptions"],
      ...(optional ? { optional: true } : {}),
      ...(readonly ? { readonly: true } : {}),
    }));
}
