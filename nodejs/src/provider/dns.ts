import type {
  ProviderExportDeclaration,
  ProviderParameterDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpDelegateTargetType,
  csharpJsArrayTargetType,
  csharpNullableTargetType,
  csharpNullableValueTargetType,
  csharpSourcePrimitiveTargetType,
  csharpStringTargetType,
  csharpTaskTargetType,
  csharpVoidTargetType,
  targetParameter,
} from "@tsonic/target-csharp/provider";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  arrayProviderType,
  booleanProviderType,
  callbackProviderType,
  nodejsCapabilityModuleExports,
  nodejsTargetNamedType,
  numberProviderType,
  providerRef,
  stringProviderType,
  unknownProviderType,
  voidProviderType,
} from "./capability-module.js";
import {
  nodejsClassPropertyTargetMetadata,
  nodejsModuleCallTargetMetadata,
  nodejsModulePropertyTargetMetadata,
} from "./members/target-member-metadata.js";
import type {
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
  NodejsModulePropertyTargetMetadata,
} from "./members/target-member-metadata.js";

export const nodeDnsModuleSpecifier = "node:dns";
export const nodeDnsPromisesModuleSpecifier = "node:dns/promises";

const lookupOptionsProviderType = providerRef(nodeDnsModuleSpecifier, "LookupOptions");
const lookupAllOptionsProviderType = providerRef(nodeDnsModuleSpecifier, "LookupAllOptions");
const lookupAddressProviderType = providerRef(nodeDnsModuleSpecifier, "LookupAddress");
const lookupAddressArrayProviderType = arrayProviderType(lookupAddressProviderType);
const lookupOptionsTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "LookupOptions");
const lookupAddressTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "LookupAddress");
const lookupAddressArrayTargetType = csharpJsArrayTargetType(lookupAddressTargetType);
const dnsTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "dns");
const dnsPromisesTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "dns_promises");
const dnsPromisesInstanceTargetType = nodejsTargetNamedType("Tsonic.CSharp.Node", "DnsPromises");
const stringTargetType = csharpStringTargetType();
const intTargetType = csharpSourcePrimitiveTargetType("int32");
const nullableIntTargetType = csharpNullableValueTargetType(intTargetType);
const boolTargetType = csharpSourcePrimitiveTargetType("bool");
const nullableBoolTargetType = csharpNullableValueTargetType(boolTargetType);
const nullableStringTargetType = csharpNullableTargetType(stringTargetType);
const voidTargetType = csharpVoidTargetType();
const exceptionTargetType = csharpNullableTargetType(nodejsTargetNamedType("System", "Exception"));
const lookupCallbackProviderType = callbackProviderType("node:dns.lookup.callback", [
  { name: "error", type: unknownProviderType },
  { name: "address", type: stringProviderType },
  { name: "family", type: numberProviderType },
]);
const lookupCallbackTargetType = csharpDelegateTargetType("System.Action", [
  exceptionTargetType,
  stringTargetType,
  intTargetType,
]);
const lookupAllCallbackProviderType = callbackProviderType("node:dns.lookup-all.callback", [
  { name: "error", type: unknownProviderType },
  { name: "addresses", type: lookupAddressArrayProviderType },
]);
const lookupAllCallbackTargetType = csharpDelegateTargetType("System.Action", [
  exceptionTargetType,
  lookupAddressArrayTargetType,
]);

export function nodeDnsExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeDnsModuleSpecifier,
    moduleCalls: nodeDnsCallTargetMembers(),
    moduleProperties: nodeDnsPropertyTargetMembers(),
    classProperties: nodeDnsClassPropertyTargetMembers(),
    classes: ["LookupOptions", "LookupAllOptions", "LookupAddress", "DnsPromises"],
  });
}

export function nodeDnsPromisesExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({
    moduleSpecifier: nodeDnsPromisesModuleSpecifier,
    moduleCalls: nodeDnsPromisesCallTargetMembers(),
  });
}

export function nodeDnsCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return Object.freeze([
    moduleCall(nodeDnsModuleSpecifier, dnsTargetType, "lookup", [
      stringParameter("hostname"),
      callbackParameter("callback", lookupCallbackProviderType),
    ], voidProviderType, [
      targetParameter("hostname", stringTargetType),
      targetParameter("callback", lookupCallbackTargetType),
    ], voidTargetType),
    moduleCall(nodeDnsModuleSpecifier, dnsTargetType, "lookup", [
      stringParameter("hostname"),
      { name: "options", type: lookupOptionsProviderType },
      callbackParameter("callback", lookupCallbackProviderType),
    ], voidProviderType, [
      targetParameter("hostname", stringTargetType),
      targetParameter("options", lookupOptionsTargetType),
      targetParameter("callback", lookupCallbackTargetType),
    ], voidTargetType),
    moduleCall(nodeDnsModuleSpecifier, dnsTargetType, "lookup", [
      stringParameter("hostname"),
      { name: "options", type: lookupAllOptionsProviderType },
      callbackParameter("callback", lookupAllCallbackProviderType),
    ], voidProviderType, [
      targetParameter("hostname", stringTargetType),
      targetParameter("options", lookupOptionsTargetType),
      targetParameter("callback", lookupAllCallbackTargetType),
    ], voidTargetType),
  ]);
}

export function nodeDnsPromisesCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return Object.freeze([
    moduleCall(nodeDnsPromisesModuleSpecifier, dnsPromisesTargetType, "lookup", [
      stringParameter("hostname"),
      optionalParameter("options", lookupOptionsProviderType),
    ], promiseProviderType(lookupAddressProviderType), [
      targetParameter("hostname", stringTargetType),
      targetParameter("options", lookupOptionsTargetType, { optional: true }),
    ], csharpTaskTargetType(lookupAddressTargetType)),
    moduleCall(nodeDnsPromisesModuleSpecifier, dnsPromisesTargetType, "lookup", [
      stringParameter("hostname"),
      { name: "options", type: lookupAllOptionsProviderType },
    ], promiseProviderType(lookupAddressArrayProviderType), [
      targetParameter("hostname", stringTargetType),
      targetParameter("options", lookupOptionsTargetType),
    ], csharpTaskTargetType(lookupAddressArrayTargetType), "lookupAll"),
  ]);
}

export function nodeDnsPropertyTargetMembers(): readonly NodejsModulePropertyTargetMetadata[] {
  return Object.freeze([
    nodejsModulePropertyTargetMetadata({
      exportName: "promises",
      targetMemberId: "Tsonic.CSharp.Node.dns.promises",
      sourceName: "promises",
      targetName: "promises",
      providerType: providerRef(nodeDnsModuleSpecifier, "DnsPromises"),
      targetReturnType: dnsPromisesInstanceTargetType,
      declaringType: dnsTargetType,
    }),
  ]);
}

export function nodeDnsClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  return Object.freeze([
    optionProperty("LookupOptions", "family", numberProviderType, nullableIntTargetType),
    optionProperty("LookupOptions", "hints", numberProviderType, nullableIntTargetType),
    optionProperty("LookupOptions", "all", booleanProviderType, nullableBoolTargetType),
    optionProperty("LookupOptions", "order", stringProviderType, nullableStringTargetType),
    optionProperty("LookupAllOptions", "family", numberProviderType, nullableIntTargetType),
    optionProperty("LookupAllOptions", "hints", numberProviderType, nullableIntTargetType),
    optionProperty("LookupAllOptions", "all", { kind: "literal", value: true }, nullableBoolTargetType, false),
    optionProperty("LookupAllOptions", "order", stringProviderType, nullableStringTargetType),
    readonlyProperty("LookupAddress", "address", stringProviderType, stringTargetType),
    readonlyProperty("LookupAddress", "family", numberProviderType, intTargetType),
  ]);
}

function moduleCall(
  moduleSpecifier: string,
  declaringType: TargetTypeRef,
  exportName: string,
  providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression,
  targetParameters: Parameters<typeof nodejsModuleCallTargetMetadata>[0]["targetParameters"],
  targetReturnType: TargetTypeRef,
  targetName = exportName,
): NodejsModuleCallTargetMetadata {
  const shape = providerParameters.map((parameter) => parameter.name).join(",");
  return nodejsModuleCallTargetMetadata({
    exportName,
    signatureId: `${moduleSpecifier}.${exportName}(${shape})`,
    targetMemberId: `${declaringType.kind === "target-named" ? declaringType.id : moduleSpecifier}.${targetName}(${shape})`,
    sourceName: exportName,
    targetName,
    providerParameters,
    providerReturnType,
    targetParameters,
    targetReturnType,
    declaringType,
  });
}

function optionProperty(
  exportName: "LookupOptions" | "LookupAllOptions",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  optional = true,
): NodejsClassPropertyTargetMetadata {
  return {
    ...property(exportName, memberName, providerType, targetReturnType, lookupOptionsTargetType),
    ...(optional ? { optional: true } : {}),
  };
}

function readonlyProperty(
  exportName: "LookupAddress",
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return {
    ...property(exportName, memberName, providerType, targetReturnType, lookupAddressTargetType),
    readonly: true,
  };
}

function property(
  exportName: string,
  memberName: string,
  providerType: ProviderTypeExpression,
  targetReturnType: TargetTypeRef,
  declaringType: TargetTypeRef,
): NodejsClassPropertyTargetMetadata {
  return nodejsClassPropertyTargetMetadata({
    exportName,
    memberName,
    memberId: `${nodeDnsModuleSpecifier}.${exportName}.${memberName}`,
    targetMemberId: `Tsonic.CSharp.Node.${declaringType.kind === "target-named" ? declaringType.id : exportName}.${memberName}`,
    sourceName: memberName,
    targetName: memberName,
    memberKind: "property",
    providerType,
    targetParameters: [],
    targetReturnType,
    declaringType,
  });
}

function stringParameter(name: string): ProviderParameterDeclaration {
  return { name, type: stringProviderType };
}

function callbackParameter(name: string, type: ProviderTypeExpression): ProviderParameterDeclaration {
  return { name, type };
}

function optionalParameter(name: string, type: ProviderTypeExpression): ProviderParameterDeclaration {
  return { name, type, optional: true };
}

function promiseProviderType(resultType: ProviderTypeExpression): ProviderTypeExpression {
  return { kind: "source-global", name: "Promise", typeArguments: [resultType] };
}
