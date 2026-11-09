import type { ProviderExportDeclaration, ProviderSignatureDeclaration } from "@tsonic/tsts";
import type { CsharpProviderSourceIdentity, CsharpProviderSourceIdentityBase } from "@tsonic/target-csharp/provider";
import type { NodejsProviderDeclarationIdentity } from "../model/identity.js";
import { nodejsCanonicalProviderExports, nodejsProviderModules } from "../assembly/modules.js";

export function nodejsSourceRelationIndex(includeJsSurface: boolean) {
  const declarations = new Map<string, ProviderExportDeclaration>();
  const signatures = new Map<string, ProviderSignatureDeclaration>();
  for (const { moduleSpecifier } of nodejsProviderModules) {
    const exports = nodejsCanonicalProviderExports(moduleSpecifier, includeJsSurface);
    if (exports === undefined) throw new TypeError(`Missing Node Source module '${moduleSpecifier}'.`);
    for (const declaration of exports) {
      insert(declarations, sourceKey(moduleSpecifier, declaration.id), declaration);
      for (const signature of declaration.signatures ?? []) {
        insert(signatures, sourceKey(moduleSpecifier, declaration.id, undefined, signature.id), signature);
      }
      for (const member of declaration.members ?? []) {
        for (const signature of member.signatures ?? []) {
          insert(signatures, sourceKey(moduleSpecifier, declaration.id, member.id, signature.id), signature);
        }
      }
    }
  }
  return Object.freeze({
    declaration(moduleSpecifier: string, exportId: string) {
      return declarations.get(sourceKey(moduleSpecifier, exportId));
    },
    signature(identity: NodejsProviderDeclarationIdentity) {
      return identity.exportId === undefined || identity.signatureId === undefined ? undefined
        : signatures.get(sourceKey(identity.providerModuleId, identity.exportId, identity.memberId, identity.signatureId));
    },
  });
}

export function nodejsProviderSourceIdentity(
  identity: NodejsProviderDeclarationIdentity,
  moduleSpecifier: string,
): Exclude<CsharpProviderSourceIdentity, { readonly kind: "type" }> {
  const base = nodejsProviderSourceIdentityBase(identity, moduleSpecifier);
  if (identity.signatureId !== undefined) {
    return { kind: "signature", ...base, ...memberIdentity(identity), signatureId: identity.signatureId };
  }
  if (identity.memberId !== undefined) {
    if (identity.memberStatic === undefined || identity.memberKey === undefined) {
      throw new TypeError("A Node Source member relation retains its exact authored identity.");
    }
    return { kind: "member", ...base, memberId: identity.memberId, memberStatic: identity.memberStatic, memberKey: identity.memberKey };
  }
  return { kind: "value", ...base };
}

export function nodejsProviderSourceIdentityBase(
  identity: NodejsProviderDeclarationIdentity,
  moduleSpecifier: string,
): CsharpProviderSourceIdentityBase {
  if (identity.exportId === undefined || identity.exportName === undefined) {
    throw new TypeError("A Node Source export relation retains its exact authored identity.");
  }
  return { providerId: identity.providerId, providerVersion: identity.providerVersion,
    providerModuleId: identity.providerModuleId, moduleSpecifier, exportId: identity.exportId, exportName: identity.exportName };
}

function memberIdentity(identity: NodejsProviderDeclarationIdentity) {
  if (identity.memberId === undefined && identity.memberStatic === undefined && identity.memberKey === undefined) return {};
  if (identity.memberId === undefined || identity.memberStatic === undefined || identity.memberKey === undefined) {
    throw new TypeError("A Node Source signature relation retains its complete authored member identity.");
  }
  return { memberId: identity.memberId, memberStatic: identity.memberStatic, memberKey: identity.memberKey };
}

function sourceKey(moduleSpecifier: string, exportId: string, memberId?: string, signatureId?: string): string {
  return `${moduleSpecifier}\u0000${exportId}\u0000${memberId ?? ""}\u0000${signatureId ?? ""}`;
}

function insert<Value>(index: Map<string, Value>, key: string, value: Value): void {
  if (index.has(key)) throw new TypeError(`Duplicate authored Node Source identity '${key}'.`);
  index.set(key, value);
}
