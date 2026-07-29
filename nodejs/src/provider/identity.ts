import {
  TstsSourceProviderContractVersion,
} from "@tsonic/tsts";
import type {
  ProviderIdentity,
  ProviderVirtualDeclarationFact,
} from "@tsonic/tsts";
import {
  csharpProviderVersion,
} from "@tsonic/target-csharp";

export const csharpNodejsProviderPackageProviderIdentity = {
  id: "tsonic.csharp.provider-package.nodejs",
  version: csharpProviderVersion,
  extensionContractVersion: TstsSourceProviderContractVersion,
  displayName: "Tsonic C# NodeJS provider package",
} satisfies ProviderIdentity;

export interface NodejsProviderDeclarationIdentity {
  readonly providerId: string;
  readonly providerVersion: string;
  readonly providerModuleId: string;
  readonly moduleSpecifier: string;
  readonly artifactFileName: string;
  readonly exportName?: string;
  readonly exportId?: string;
  readonly memberName?: string;
  readonly memberKey?: ProviderVirtualDeclarationFact["memberKey"];
  readonly memberId?: string;
  readonly memberStatic?: boolean;
  readonly signatureId?: string;
}

export function csharpNodejsVirtualDeclarationFileName(specifier: string): string {
  return `tsts-provider://csharp-nodejs/${encodeURIComponent(specifier)}.d.ts`;
}

export function nodejsExportDeclarationIdentity(
  moduleSpecifier: string,
  exportName: string,
): NodejsProviderDeclarationIdentity {
  return {
    providerId: csharpNodejsProviderPackageProviderIdentity.id,
    providerVersion: csharpNodejsProviderPackageProviderIdentity.version,
    providerModuleId: moduleSpecifier,
    moduleSpecifier,
    artifactFileName: csharpNodejsVirtualDeclarationFileName(moduleSpecifier),
    exportName,
    exportId: `${moduleSpecifier}.${exportName}`,
  };
}

export function nodejsExportSignatureDeclarationIdentity(
  moduleSpecifier: string,
  exportName: string,
  signatureId: string,
): NodejsProviderDeclarationIdentity {
  return {
    ...nodejsExportDeclarationIdentity(moduleSpecifier, exportName),
    signatureId,
  };
}

export function nodejsExportMemberDeclarationIdentity(
  moduleSpecifier: string,
  exportName: string,
  memberName: string,
  memberId: string,
  memberStatic = false,
): NodejsProviderDeclarationIdentity {
  return {
    ...nodejsExportDeclarationIdentity(moduleSpecifier, exportName),
    memberName,
    memberKey: { kind: "property-key", name: memberName },
    memberId,
    memberStatic,
  };
}

export function nodejsExportMemberSignatureDeclarationIdentity(
  moduleSpecifier: string,
  exportName: string,
  memberName: string,
  memberId: string,
  signatureId: string,
  memberStatic = false,
): NodejsProviderDeclarationIdentity {
  return {
    ...nodejsExportMemberDeclarationIdentity(
      moduleSpecifier,
      exportName,
      memberName,
      memberId,
      memberStatic,
    ),
    signatureId,
  };
}

export function nodejsProviderDeclarationIdentityKey(declaration: NodejsProviderDeclarationIdentity): string {
  return [
    declaration.providerId,
    declaration.providerVersion,
    declaration.providerModuleId,
    declaration.moduleSpecifier,
    declaration.exportName ?? "",
    declaration.exportId ?? "",
    declaration.memberName ?? "",
    declaration.memberKey === undefined
      ? ""
      : `${declaration.memberKey.kind}:${declaration.memberKey.name}`,
    declaration.memberId ?? "",
    declaration.memberStatic === undefined
      ? ""
      : declaration.memberStatic
        ? "static"
        : "instance",
    declaration.signatureId ?? "",
  ].join("\u0000");
}

export function isCsharpNodejsProviderDeclaration(
  declaration: ProviderVirtualDeclarationFact,
): declaration is ProviderVirtualDeclarationFact & NodejsProviderDeclarationIdentity {
  return declaration.providerId === csharpNodejsProviderPackageProviderIdentity.id;
}
