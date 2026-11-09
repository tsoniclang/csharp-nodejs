import type { ProviderTypeExpression } from "@tsonic/tsts";
import { nativeNamedType } from "../../native/locators.js";
import { nativeOwner } from "../../native/authoring.js";

export const stringProviderType: ProviderTypeExpression = { kind: "string" };
export const boolProviderType: ProviderTypeExpression = { kind: "boolean" };
export const nodePathModuleSpecifier = "node:path";
export const parsedPathProviderType: ProviderTypeExpression = { kind: "provider-ref", moduleSpecifier: nodePathModuleSpecifier, exportName: "ParsedPath" };
export const pathModuleProviderType: ProviderTypeExpression = { kind: "provider-ref", moduleSpecifier: nodePathModuleSpecifier, exportName: "PathModule" };
export const pathOwner = nativeOwner("Tsonic.CSharp.Node.path");
export const pathModuleOwner = nativeOwner("Tsonic.CSharp.Node.PathModule");
export const parsedPathOwner = nativeOwner("Tsonic.CSharp.Node.ParsedPath");
export const nativeParsedPath = nativeNamedType(parsedPathOwner.metadataName);
export const nativePathModule = nativeNamedType(pathModuleOwner.metadataName);

