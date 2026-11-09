import type {
  CsharpAcquiredRuntimeNativeDeclaration,
  CsharpAcquiredRuntimeProviderNativeDomain,
  CsharpProviderTargetRejectionDiagnostic,
  CsharpTargetMember,
} from "@tsonic/target-csharp/provider";
import type {
  DotnetMemberDeclaration,
  DotnetSignatureDeclaration,
  DotnetTypeParameterDeclaration,
  DotnetTypeRef,
} from "@tsonic/target-csharp/provider/dotnet";
import type {
  NodejsNativeCallableLocator,
  NodejsNativeMemberLocator,
  NodejsNativeSlotLocator,
  NodejsNativeTypeDeclarationLocator,
  NodejsNativeTypeLocator,
} from "./locators.js";
import { csharpNodejsProviderPackageProviderIdentity } from "../model/identity.js";

export type NodejsNativeResolution<Value> =
  | { readonly kind: "resolved"; readonly value: Value }
  | { readonly kind: "rejected"; readonly diagnostic: CsharpProviderTargetRejectionDiagnostic };

export interface NodejsSelectedNativeMember {
  readonly declaration: CsharpAcquiredRuntimeNativeDeclaration;
  readonly original: DotnetMemberDeclaration;
  readonly signature: DotnetSignatureDeclaration | undefined;
  readonly member: CsharpTargetMember;
}

export interface NodejsNativeSelection {
  declaration(locator: NodejsNativeTypeDeclarationLocator): NodejsNativeResolution<CsharpAcquiredRuntimeNativeDeclaration>;
  member(locator: NodejsNativeMemberLocator): NodejsNativeResolution<NodejsSelectedNativeMember>;
}

export function createNodejsNativeSelection(
  domain: CsharpAcquiredRuntimeProviderNativeDomain,
): NodejsNativeSelection {
  const declarations = new Map<string, CsharpAcquiredRuntimeNativeDeclaration[]>();
  const memberIndexes = new Map<CsharpAcquiredRuntimeNativeDeclaration, {
    readonly originals: ReadonlyMap<string, readonly DotnetMemberDeclaration[]>;
    readonly members: ReadonlyMap<string, readonly CsharpTargetMember[]>;
  }>();
  for (const declaration of domain.declarations) {
    const originalId = declaration.original.targetId;
    if (declaration.binding.id !== originalId || declaration.definition.carrier.id !== originalId) {
      throw new TypeError("An acquired Node Native declaration retains the same original, binding and definition identity.");
    }
    const candidates = declarations.get(declaration.original.metadataName);
    if (candidates === undefined) declarations.set(declaration.original.metadataName, [declaration]);
    else candidates.push(declaration);
  }
  function declaration(locator: NodejsNativeTypeDeclarationLocator): NodejsNativeResolution<CsharpAcquiredRuntimeNativeDeclaration> {
    const candidates = (declarations.get(locator.metadataName) ?? []).filter(candidate =>
      (candidate.original.typeParameters?.length ?? 0) === locator.typeParameterCount);
    return candidates.length === 1 && candidates[0] !== undefined
      ? resolved(candidates[0])
      : rejected("The authored Node Native type locator must identify exactly one complete original in the selected image.", locator.metadataName);
  }
  function namedIdentity(metadataName: string): NodejsNativeResolution<string> {
    const candidates = declarations.get(metadataName) ?? [];
    return candidates.length === 1 && candidates[0] !== undefined
      ? resolved(candidates[0].original.targetId)
      : rejected("A Node Native signature reference must identify one actual declaration in the selected reference closure.", metadataName);
  }
  function memberIndex(owner: CsharpAcquiredRuntimeNativeDeclaration) {
    const existing = memberIndexes.get(owner);
    if (existing !== undefined) return existing;
    const originals = new Map<string, DotnetMemberDeclaration[]>();
    const members = new Map<string, CsharpTargetMember[]>();
    for (const original of owner.original.members ?? []) {
      const key = memberKey(original.kind, original.targetName, original.static === true);
      const entries = originals.get(key);
      if (entries === undefined) originals.set(key, [original]);
      else entries.push(original);
    }
    for (const { member } of owner.definition.members) {
      const entries = members.get(member.id);
      if (entries === undefined) members.set(member.id, [member]);
      else entries.push(member);
    }
    const index = Object.freeze({ originals, members });
    memberIndexes.set(owner, index);
    return index;
  }
  function matchesType(
    actual: DotnetTypeRef,
    locator: NodejsNativeTypeLocator,
    classParameters: readonly DotnetTypeParameterDeclaration[],
    methodParameters: readonly DotnetTypeParameterDeclaration[],
  ): NodejsNativeResolution<boolean> {
    switch (locator.kind) {
      case "void":
      case "string":
      case "object":
        return resolved(actual.kind === locator.kind);
      case "source-primitive":
        return resolved(actual.kind === "source-primitive" && actual.name === locator.name);
      case "class-parameter":
      case "method-parameter": {
        const parameter = (locator.kind === "class-parameter" ? classParameters : methodParameters)[locator.index];
        return parameter === undefined
          ? rejected("A Node Native locator addresses the exact original class or method binder vector.", String(locator.index))
          : resolved(actual.kind === "type-parameter" && actual.identity === parameter.identity);
      }
      case "named": {
        const identity = namedIdentity(locator.metadataName);
        if (identity.kind === "rejected") return identity;
        if (actual.kind !== "named" || actual.targetId !== identity.value) return resolved(false);
        const actualArguments = actual.typeArguments ?? [];
        if (actualArguments.length !== locator.typeArguments.length) return resolved(false);
        for (const [index, argument] of locator.typeArguments.entries()) {
          const actualArgument = actualArguments[index];
          if (actualArgument === undefined) return resolved(false);
          const result = matchesType(actualArgument, argument, classParameters, methodParameters);
          if (result.kind === "rejected" || !result.value) return result;
        }
        return resolved(true);
      }
      case "array":
        return actual.kind !== "array" || actual.rank !== locator.rank
          ? resolved(false)
          : matchesType(actual.elementType, locator.elementType, classParameters, methodParameters);
      case "nullable":
      case "nullable-reference":
        return actual.kind !== locator.kind
          ? resolved(false)
          : matchesType(actual.elementType, locator.elementType, classParameters, methodParameters);
    }
  }
  function selectCallable(
    owner: CsharpAcquiredRuntimeNativeDeclaration,
    locator: NodejsNativeCallableLocator,
  ): NodejsNativeResolution<NodejsSelectedNativeMember> {
    const candidates: NodejsSelectedNativeMember[] = [];
    const index = memberIndex(owner);
    for (const original of index.originals.get(memberKey(locator.kind, locator.targetName, locator.static)) ?? []) {
      for (const signature of original.signatures ?? []) {
        const methodParameters = signature.typeParameters ?? [];
        if (methodParameters.length !== locator.typeParameterCount || signature.parameters.length !== locator.parameters.length ||
          (signature.returnPassing ?? "by-value") !== locator.returnPassing) continue;
        let matches = true;
        for (const [index, parameter] of locator.parameters.entries()) {
          const actual = signature.parameters[index];
          if (actual === undefined || actual.passingMode !== parameter.passingMode) {
            matches = false;
            break;
          }
          const result = matchesType(actual.type, parameter.type, owner.original.typeParameters ?? [], methodParameters);
          if (result.kind === "rejected") return result;
          if (!result.value) {
            matches = false;
            break;
          }
        }
        if (!matches) continue;
        const actualReturn = signature.targetReturnType ?? signature.returnType;
        const result = original.kind === "constructor"
          ? resolved(locator.returnType.kind === "named" && locator.returnType.metadataName === owner.original.metadataName &&
            locator.returnType.typeArguments.length === (owner.original.typeParameters?.length ?? 0) &&
            locator.returnType.typeArguments.every((argument, parameterIndex) =>
              argument.kind === "class-parameter" && argument.index === parameterIndex))
          : actualReturn === undefined
            ? rejected("The selected original Native callable has no physical return receipt.", signature.id)
            : matchesType(actualReturn, locator.returnType, owner.original.typeParameters ?? [], methodParameters);
        if (result.kind === "rejected") return result;
        if (!result.value) continue;
        const members = index.members.get(signature.id) ?? [];
        if (members.length !== 1 || members[0] === undefined) {
          return rejected("The selected original signature requires its one canonical complete Native member.", signature.id);
        }
        candidates.push(Object.freeze({ declaration: owner, original, signature, member: members[0] }));
      }
    }
    return candidates.length === 1 && candidates[0] !== undefined ? resolved(candidates[0])
      : rejected("The complete authored Node Native callable locator must identify exactly one original signature.", locator.targetName);
  }
  function selectSlot(
    owner: CsharpAcquiredRuntimeNativeDeclaration,
    locator: NodejsNativeSlotLocator,
  ): NodejsNativeResolution<NodejsSelectedNativeMember> {
    const candidates: NodejsSelectedNativeMember[] = [];
    const index = memberIndex(owner);
    for (const original of index.originals.get(memberKey(locator.kind, locator.targetName, locator.static)) ?? []) {
      if (original.type === undefined) continue;
      const result = matchesType(original.type, locator.type, owner.original.typeParameters ?? [], []);
      if (result.kind === "rejected") return result;
      if (!result.value) continue;
      const members = index.members.get(original.targetId) ?? [];
      if (members.length !== 1 || members[0] === undefined) {
        return rejected("The selected original Native slot requires its canonical independent access/storage metadata.", original.targetId);
      }
      candidates.push(Object.freeze({ declaration: owner, original, signature: undefined, member: members[0] }));
    }
    return candidates.length === 1 && candidates[0] !== undefined ? resolved(candidates[0])
      : rejected("The complete authored Node Native slot locator must identify exactly one original slot.", locator.targetName);
  }
  return Object.freeze({
    declaration,
    member(locator: NodejsNativeMemberLocator): NodejsNativeResolution<NodejsSelectedNativeMember> {
      const owner = declaration(locator.owner);
      if (owner.kind === "rejected") return owner;
      return locator.kind === "property" || locator.kind === "field"
        ? selectSlot(owner.value, locator) : selectCallable(owner.value, locator);
    },
  });
}

function memberKey(kind: DotnetMemberDeclaration["kind"], name: string, isStatic: boolean): string {
  return `${kind}\u0000${isStatic ? "static" : "instance"}\u0000${name}`;
}

export function resolved<Value>(value: Value): Extract<NodejsNativeResolution<Value>, { readonly kind: "resolved" }> {
  return Object.freeze({ kind: "resolved", value });
}

export function rejected(message: string, locator: string): Extract<NodejsNativeResolution<never>, { readonly kind: "rejected" }> {
  return Object.freeze({ kind: "rejected", diagnostic: Object.freeze({
    extensionId: csharpNodejsProviderPackageProviderIdentity.id,
    extensionCode: "CSHARP_NODEJS_NATIVE_DECLARATION_MAPPING_NOT_PROVEN",
    numericCode: 9100204,
    category: "error",
    message,
    evidence: Object.freeze([{ message: `Authored Native locator: ${locator}` }]),
  }) });
}
