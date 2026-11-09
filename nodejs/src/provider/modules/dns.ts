import type { ProviderExportDeclaration, ProviderParameterDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsClassPropertyTargetMetadata, NodejsModuleCallTargetMetadata, NodejsModulePropertyTargetMetadata } from "../model/target-members.js";
import type { NodejsNativeParameterMapping } from "../native/authoring.js";
import type { NodejsNativeTypeLocator } from "../native/locators.js";
import { nativeCall, nativeOwner, nativeParameter, nativeSlot } from "../native/authoring.js";
import { nativeNamedType, nativeNullable, nativeNullableReference } from "../native/locators.js";
import { arrayProviderType, callbackProviderType, nodejsCapabilityModuleExports, providerRef } from "../declarations/exports.js";
import { booleanProviderType, int32ProviderType, stringProviderType, voidProviderType } from "../model/source-types.js";
import { optionalNodeErrorProviderType } from "./util/node-error.js";

export const nodeDnsModuleSpecifier = "node:dns";
export const nodeDnsPromisesModuleSpecifier = "node:dns/promises";
const dnsOwner = nativeOwner("Tsonic.CSharp.Node.dns");
const promisesOwner = nativeOwner("Tsonic.CSharp.Node.dns_promises");
const optionsOwner = nativeOwner("Tsonic.CSharp.Node.LookupOptions");
const addressOwner = nativeOwner("Tsonic.CSharp.Node.LookupAddress");
const lookupOptionsProviderType = providerRef(nodeDnsModuleSpecifier, "LookupOptions");
const lookupAllOptionsProviderType = providerRef(nodeDnsModuleSpecifier, "LookupAllOptions");
const lookupAddressProviderType = providerRef(nodeDnsModuleSpecifier, "LookupAddress");
const lookupAddressArrayProviderType = arrayProviderType(lookupAddressProviderType);
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const nativeBool: NodejsNativeTypeLocator = { kind: "source-primitive", name: "bool" };
const options = nativeNullableReference(nativeNamedType(optionsOwner.metadataName));
const address = nativeNamedType(addressOwner.metadataName);
const addresses = nativeNamedType("Tsonic.CSharp.Js.JSArray`1", address);
const exception = nativeNullableReference(nativeNamedType("System.Exception"));
const lookupCallbackProviderType = callbackProviderType("node:dns.lookup.callback", [
  { name: "error", type: optionalNodeErrorProviderType }, { name: "address", type: stringProviderType }, { name: "family", type: int32ProviderType },
]);
const lookupAllCallbackProviderType = callbackProviderType("node:dns.lookup-all.callback", [
  { name: "error", type: optionalNodeErrorProviderType }, { name: "addresses", type: lookupAddressArrayProviderType },
]);
const callback = nativeNamedType("System.Action`3", exception, nativeString, nativeInt);
const allCallback = nativeNamedType("System.Action`2", exception, addresses);

export function nodeDnsCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  const hostname = { name: "hostname", type: stringProviderType };
  return [
    call(false, "lookup", "hostname,callback", [hostname, { name: "callback", type: lookupCallbackProviderType }], voidProviderType,
      [nativeParameter(0, nativeString), nativeParameter(1, callback)], { kind: "void" }),
    call(false, "lookup", "hostname,lookup-options,callback", [hostname, { name: "options", type: lookupOptionsProviderType }, { name: "callback", type: lookupCallbackProviderType }], voidProviderType,
      [nativeParameter(0, nativeString), nativeParameter(1, options), nativeParameter(2, callback)], { kind: "void" }),
    call(false, "lookup", "hostname,lookup-all-options,callback", [hostname, { name: "options", type: lookupAllOptionsProviderType }, { name: "callback", type: lookupAllCallbackProviderType }], voidProviderType,
      [nativeParameter(0, nativeString), nativeParameter(1, options), nativeParameter(2, allCallback)], { kind: "void" }),
  ];
}

export function nodeDnsPromisesCallTargetMembers(): readonly NodejsModuleCallTargetMetadata[] {
  return [
    call(true, "lookup", "hostname,lookup-options", [{ name: "hostname", type: stringProviderType }, { name: "options", type: lookupOptionsProviderType, optional: true }],
      promiseProviderType(lookupAddressProviderType), [nativeParameter(0, nativeString), nativeParameter(1, options)], nativeNamedType("System.Threading.Tasks.Task`1", address)),
    call(true, "lookup", "hostname,lookup-all-options", [{ name: "hostname", type: stringProviderType }, { name: "options", type: lookupAllOptionsProviderType }],
      promiseProviderType(lookupAddressArrayProviderType), [nativeParameter(0, nativeString), nativeParameter(1, options)], nativeNamedType("System.Threading.Tasks.Task`1", addresses), "lookupAll"),
  ];
}

export function nodeDnsPropertyTargetMembers(): readonly NodejsModulePropertyTargetMetadata[] {
  return [{ exportName: "promises", providerType: providerRef(nodeDnsModuleSpecifier, "DnsPromises"),
    native: nativeSlot(dnsOwner, "promises", nativeNamedType("Tsonic.CSharp.Node.DnsPromises"), true) }];
}

export function nodeDnsClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMetadata[] {
  const result: NodejsClassPropertyTargetMetadata[] = [];
  for (const exportName of ["LookupOptions", "LookupAllOptions"]) {
    const fields: readonly (readonly [string, ProviderTypeExpression, NodejsNativeTypeLocator, boolean])[] = [
      ["family", int32ProviderType, nativeNullable(nativeInt), true], ["hints", int32ProviderType, nativeNullable(nativeInt), true],
      ["all", exportName === "LookupAllOptions" ? { kind: "literal", value: true } : booleanProviderType, nativeNullable(nativeBool), exportName !== "LookupAllOptions"],
      ["order", stringProviderType, nativeNullableReference(nativeString), true],
    ];
    for (const [memberName, providerType, physical, optional] of fields) {
      result.push({ exportName, memberName, memberId: `node:dns.${exportName}.${memberName}`, memberKind: "property", providerType,
        ...optional ? { optional: true } : {}, native: nativeSlot(optionsOwner, memberName, physical) });
    }
  }
  result.push(
    { exportName: "LookupAddress", memberName: "address", memberId: "node:dns.LookupAddress.address", memberKind: "property",
      providerType: stringProviderType, readonly: true, native: nativeSlot(addressOwner, "address", nativeString) },
    { exportName: "LookupAddress", memberName: "family", memberId: "node:dns.LookupAddress.family", memberKind: "property",
      providerType: int32ProviderType, readonly: true, native: nativeSlot(addressOwner, "family", nativeInt) },
  );
  return Object.freeze(result);
}

export function nodeDnsExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeDnsModuleSpecifier, moduleCalls: nodeDnsCallTargetMembers(),
    moduleProperties: nodeDnsPropertyTargetMembers(), classProperties: nodeDnsClassPropertyTargetMembers(),
    classes: ["LookupOptions", "LookupAllOptions", "LookupAddress", "DnsPromises"] });
}
export function nodeDnsPromisesExports(): readonly ProviderExportDeclaration[] {
  return nodejsCapabilityModuleExports({ moduleSpecifier: nodeDnsPromisesModuleSpecifier, moduleCalls: nodeDnsPromisesCallTargetMembers() });
}

function call(
  promises: boolean, exportName: string, identityShape: string, providerParameters: readonly ProviderParameterDeclaration[],
  providerReturnType: ProviderTypeExpression, parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator, targetName = exportName,
): NodejsModuleCallTargetMetadata {
  return { exportName, signatureId: `${promises ? nodeDnsPromisesModuleSpecifier : nodeDnsModuleSpecifier}.${exportName}(${identityShape})`,
    providerParameters, providerReturnType, native: nativeCall({ owner: promises ? promisesOwner : dnsOwner, kind: "method",
      targetName, static: true, parameters, returnType, receiver: { kind: "none" } }) };
}
function promiseProviderType(resultType: ProviderTypeExpression): ProviderTypeExpression {
  return { kind: "source-global", name: "Promise", typeArguments: [resultType] };
}

