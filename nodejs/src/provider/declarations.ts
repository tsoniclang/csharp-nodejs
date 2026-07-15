import {
  providerVirtualDeclarationFactKey,
} from "@tsonic/tsts";
import type {
  CheckedCallMappingRequest,
  CheckedElementAccessMappingRequest,
  CheckedPropertyAccessMappingRequest,
  ExtensionFactSubject,
  ExtensionObservationContext,
} from "@tsonic/tsts";
import {
  isCsharpNodejsProviderDeclaration,
} from "./identity.js";
import type {
  NodejsProviderDeclarationIdentity,
} from "./identity.js";

export function getNodejsCheckedCallDeclaration(
  request: CheckedCallMappingRequest,
  context: ExtensionObservationContext<"operation.mapCheckedCall">,
): NodejsProviderDeclarationIdentity | undefined {
  if (request.sourceSelectedSignature === undefined) {
    return undefined;
  }
  return firstProviderDeclarationWithSignature(context, [
    request.sourceSelectedDeclaration,
    request.sourceSelectedSignature,
    request.sourceCalleeDeclaration,
    request.sourceCalleeSymbol,
  ]);
}

export function getNodejsCallDeclarationWithoutSelectedSignature(
  request: CheckedCallMappingRequest,
  context: ExtensionObservationContext<"operation.mapCheckedCall">,
): NodejsProviderDeclarationIdentity | undefined {
  if (request.sourceSelectedSignature !== undefined) {
    return undefined;
  }
  for (const subject of [
    request.sourceSelectedDeclaration,
    request.sourceCalleeDeclaration,
    request.sourceCalleeSymbol,
  ]) {
    const declaration = getProviderExportDeclaration(context, subject);
    if (declaration !== undefined) {
      return declaration;
    }
  }
  return undefined;
}

export function getNodejsCheckedPropertyDeclaration(
  request: CheckedPropertyAccessMappingRequest,
  context: ExtensionObservationContext<"operation.mapCheckedPropertyAccess">,
): NodejsProviderDeclarationIdentity | undefined {
  for (const subject of [
    request.sourceSelectedSymbol,
    request.sourceSelectedDeclaration,
  ]) {
    const declaration = getProviderExportDeclaration(context, subject);
    if (declaration !== undefined) {
      return declaration;
    }
  }
  return undefined;
}

export function getNodejsCheckedElementDeclaration(
  request: CheckedElementAccessMappingRequest,
  context: ExtensionObservationContext<"operation.mapCheckedElementAccess">,
): NodejsProviderDeclarationIdentity | undefined {
  for (const subject of [
    request.sourceSelectedSymbol,
    request.sourceSelectedDeclaration,
  ]) {
    const declaration = getProviderExportDeclaration(context, subject);
    if (declaration !== undefined) {
      return declaration;
    }
  }
  return undefined;
}

function getProviderExportDeclaration(
  context: ExtensionObservationContext,
  subject: ExtensionFactSubject | undefined,
): NodejsProviderDeclarationIdentity | undefined {
  const declaration = context.facts.get(subject, providerVirtualDeclarationFactKey);
  return declaration === undefined || !isCsharpNodejsProviderDeclaration(declaration)
    ? undefined
    : declaration;
}

function firstProviderDeclarationWithSignature(
  context: ExtensionObservationContext,
  subjects: readonly (ExtensionFactSubject | undefined)[],
): NodejsProviderDeclarationIdentity | undefined {
  for (const subject of subjects) {
    const declaration = getProviderExportDeclaration(context, subject);
    if (declaration?.signatureId !== undefined) {
      return declaration;
    }
  }
  return undefined;
}
