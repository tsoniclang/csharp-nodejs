import type {
  ProviderExportDeclaration,
  ProviderMemberDeclaration,
  ProviderSignatureDeclaration,
} from "@tsonic/tsts";
import {
  nodejsExportMemberDeclarationIdentity,
  nodejsExportMemberSignatureDeclarationIdentity,
} from "./identity.js";
import type {
  NodejsProviderDeclarationIdentity,
} from "./identity.js";

export interface NodejsDefaultModuleObjectMetadata {
  readonly moduleSpecifier: string;
  readonly className: string;
  readonly writableProperties?: readonly string[];
}

export const nodejsDefaultModuleObjects = [
  { moduleSpecifier: "node:assert", className: "NodeAssertModule" },
  { moduleSpecifier: "node:buffer", className: "NodeBufferModule" },
  { moduleSpecifier: "node:crypto", className: "NodeCryptoModule" },
  { moduleSpecifier: "node:fs", className: "NodeFsModule" },
  { moduleSpecifier: "node:fs/promises", className: "NodeFsPromisesModule" },
  { moduleSpecifier: "node:http", className: "NodeHttpModule" },
  { moduleSpecifier: "node:os", className: "NodeOsModule" },
  { moduleSpecifier: "node:path", className: "NodePathModule" },
  {
    moduleSpecifier: "node:process",
    className: "NodeProcessModule",
    writableProperties: ["exitCode"],
  },
  { moduleSpecifier: "node:timers", className: "NodeTimersModule" },
  { moduleSpecifier: "node:url", className: "NodeUrlModule" },
  { moduleSpecifier: "node:util", className: "NodeUtilModule" },
] satisfies readonly NodejsDefaultModuleObjectMetadata[];

export function nodejsDefaultModuleObjectExports(
  moduleSpecifier: string,
  exports: readonly ProviderExportDeclaration[],
): readonly ProviderExportDeclaration[] {
  const metadata = nodejsDefaultModuleObjectMetadata(moduleSpecifier);
  if (metadata === undefined) {
    return [];
  }
  const members = exports.flatMap((declaration) =>
    nodejsDefaultModuleObjectMembersForDeclaration(metadata, declaration)
  );
  return members.length === 0
    ? []
    : [
        {
          id: nodejsDefaultModuleClassId(moduleSpecifier),
          name: metadata.className,
          exportKind: "default" as const,
          kind: "class" as const,
          members,
        },
      ];
}

export function nodejsDefaultModuleObjectMetadata(
  moduleSpecifier: string,
): NodejsDefaultModuleObjectMetadata | undefined {
  return nodejsDefaultModuleObjects.find((metadata) => metadata.moduleSpecifier === moduleSpecifier);
}

export function nodejsDefaultModuleMemberDeclarationIdentities(
  moduleSpecifier: string,
  exportName: string,
  signatureId: string | undefined,
): readonly NodejsProviderDeclarationIdentity[] {
  const metadata = nodejsDefaultModuleObjectMetadata(moduleSpecifier);
  if (metadata === undefined) {
    return [];
  }
  const memberId = nodejsDefaultModuleMemberId(moduleSpecifier, exportName);
  return signatureId === undefined
    ? [nodejsExportMemberDeclarationIdentity(moduleSpecifier, "default", exportName, memberId, true)]
    : [
        nodejsExportMemberDeclarationIdentity(moduleSpecifier, "default", exportName, memberId, true),
        nodejsExportMemberSignatureDeclarationIdentity(moduleSpecifier, "default", exportName, memberId, signatureId, true),
      ];
}

function nodejsDefaultModuleObjectMembersForDeclaration(
  metadata: NodejsDefaultModuleObjectMetadata,
  declaration: ProviderExportDeclaration,
): readonly ProviderMemberDeclaration[] {
  const exportName = providerExportName(declaration);
  if (exportName === "default") {
    return [];
  }
  switch (declaration.kind) {
    case "function":
      return [{
        id: nodejsDefaultModuleMemberIdForDeclaration(metadata.moduleSpecifier, declaration),
        name: exportName,
        kind: "method",
        static: true,
        signatures: declaration.signatures?.map((signature) => nodejsDefaultModuleSignature(signature)) ?? [],
      }];
    case "value":
      return declaration.type === undefined
        ? []
        : [{
            id: nodejsDefaultModuleMemberIdForDeclaration(metadata.moduleSpecifier, declaration),
            name: exportName,
            kind: "property",
            static: true,
            ...(metadata.writableProperties?.includes(exportName) === true
              ? {}
              : { readonly: true }),
            type: declaration.type,
          }];
    default:
      return [];
  }
}

function nodejsDefaultModuleSignature(signature: ProviderSignatureDeclaration): ProviderSignatureDeclaration {
  return {
    id: signature.id,
    ...(signature.name !== undefined ? { name: signature.name } : {}),
    parameters: signature.parameters,
    ...(signature.returnType !== undefined ? { returnType: signature.returnType } : {}),
    ...(signature.typeParameters !== undefined ? { typeParameters: signature.typeParameters } : {}),
    ...(signature.documentation !== undefined ? { documentation: signature.documentation } : {}),
  };
}

function providerExportName(declaration: ProviderExportDeclaration): string {
  return declaration.exportKind === "default" ? "default" : declaration.exportName ?? declaration.name;
}

function nodejsDefaultModuleMemberIdForDeclaration(
  moduleSpecifier: string,
  declaration: ProviderExportDeclaration,
): string {
  const metadata = nodejsDefaultModuleObjectMetadata(moduleSpecifier);
  if (metadata === undefined) {
    throw new Error(`Missing C# NodeJS default module object metadata for '${moduleSpecifier}'.`);
  }
  return nodejsDefaultModuleMemberId(moduleSpecifier, providerExportName(declaration));
}

function nodejsDefaultModuleClassId(moduleSpecifier: string): string {
  return `${moduleSpecifier}.default`;
}

function nodejsDefaultModuleMemberId(
  moduleSpecifier: string,
  exportName: string,
): string {
  return `${moduleSpecifier}.default.${exportName}`;
}
