import type {
  ExtensionDiagnostic,
  ProviderDeclarationModel,
  ProviderExportDeclaration,
  ProviderImportDeclaration,
  ProviderMemberDeclaration,
  ProviderParameterDeclaration,
  ProviderRequestedExport,
  ProviderModuleContext,
  ProviderModuleResolution,
  ProviderOwnership,
  ProviderSignatureDeclaration,
  ProviderTypeParameterDeclaration,
  ProviderTypeExpression,
  TargetBindingProvider,
} from "@tsonic/tsts";
import {
  csharpNodejsProviderPackageProviderIdentity,
  csharpNodejsVirtualDeclarationFileName,
} from "./identity.js";
import {
  nodeAssertExports,
  nodeAssertModuleSpecifier,
} from "./assert.js";
import {
  nodeBufferExports,
  nodeBufferModuleSpecifier,
} from "./buffer.js";
import {
  nodeCryptoExports,
  nodeCryptoModuleSpecifier,
} from "./crypto.js";
import {
  nodeFsExports,
  nodeFsModuleSpecifier,
  nodeFsPromisesExports,
  nodeFsPromisesModuleSpecifier,
} from "./filesystem/index.js";
import {
  nodeOsExports,
  nodeOsModuleSpecifier,
} from "./os.js";
import {
  nodePathExports,
  nodePathModuleSpecifier,
} from "./path.js";
import {
  nodeProcessExports,
  nodeProcessModuleSpecifier,
} from "./process.js";
import {
  nodeUtilExports,
  nodeUtilModuleSpecifier,
} from "./util.js";
import {
  nodeUrlExports,
  nodeUrlModuleSpecifier,
} from "./url.js";
import {
  canonicalNodejsModuleSpecifier,
  isSupportedNodejsModuleSpecifier,
} from "./module-specifiers.js";

const canonicalModules = new Map<string, readonly ProviderExportDeclaration[]>([
  [nodeAssertModuleSpecifier, nodeAssertExports()],
  [nodeBufferModuleSpecifier, nodeBufferExports()],
  [nodePathModuleSpecifier, nodePathExports()],
  [nodeFsModuleSpecifier, nodeFsExports()],
  [nodeFsPromisesModuleSpecifier, nodeFsPromisesExports()],
  [nodeCryptoModuleSpecifier, nodeCryptoExports()],
  [nodeOsModuleSpecifier, nodeOsExports()],
  [nodeProcessModuleSpecifier, nodeProcessExports()],
  [nodeUtilModuleSpecifier, nodeUtilExports()],
  [nodeUrlModuleSpecifier, nodeUrlExports()],
]);

export function createCsharpNodejsProviderPackageBindingProvider(): TargetBindingProvider {
  return {
    identity: csharpNodejsProviderPackageProviderIdentity,
    ownsModule(specifier: string, _context: ProviderModuleContext): ProviderOwnership {
      return isSupportedNodejsModuleSpecifier(specifier) ? { kind: "owned" } : { kind: "unowned" };
    },
    resolveModule(specifier: string, _context: ProviderModuleContext): ProviderModuleResolution | ExtensionDiagnostic {
      const canonicalSpecifier = canonicalNodejsModuleSpecifier(specifier);
      if (canonicalSpecifier === undefined) {
        return nodejsProviderDiagnostic("NODEJS_PROVIDER_PACKAGE_MODULE_UNOWNED", 9300001, `C# NodeJS provider package does not own '${specifier}'.`);
      }
      return {
        kind: "virtual",
        moduleSpecifier: specifier,
        virtualFileName: csharpNodejsVirtualDeclarationFileName(specifier),
        providerModuleId: canonicalSpecifier,
        evidence: [{ message: "C# NodeJS provider package supplied virtual module." }],
      };
    },
    getDeclarationModel(module: ProviderModuleResolution): ProviderDeclarationModel | ExtensionDiagnostic {
      const canonicalSpecifier = canonicalNodejsModuleSpecifier(module.moduleSpecifier);
      const exports = canonicalSpecifier === undefined ? undefined : canonicalModules.get(canonicalSpecifier);
      const publicExports = canonicalSpecifier === undefined || exports === undefined
        ? undefined
        : exports.map((declaration) => rebaseNodejsProviderExport(
            declaration,
            canonicalSpecifier,
            module.moduleSpecifier,
          ));
      return canonicalSpecifier === undefined || exports === undefined
        ? nodejsProviderDiagnostic("NODEJS_PROVIDER_PACKAGE_MODULE_MISSING", 9300002, `C# NodeJS provider package has no declaration model for '${module.moduleSpecifier}'.`)
        : {
            moduleSpecifier: module.moduleSpecifier,
            providerModuleId: canonicalSpecifier,
            imports: nodejsProviderImportsForExports(canonicalSpecifier, publicExports ?? []),
            exports: publicExports ?? [],
            evidence: [{ message: "C# NodeJS provider package virtual declaration model." }],
          };
    },
  };
}

function rebaseNodejsProviderExport(
  declaration: ProviderExportDeclaration,
  canonicalModuleSpecifier: string,
  publicModuleSpecifier: string,
): ProviderExportDeclaration {
  const mapType = (type: ProviderTypeExpression): ProviderTypeExpression =>
    rebaseNodejsProviderType(type, canonicalModuleSpecifier, publicModuleSpecifier);
  return {
    ...declaration,
    ...(declaration.type === undefined ? {} : { type: mapType(declaration.type) }),
    ...(declaration.typeParameters === undefined
      ? {}
      : { typeParameters: declaration.typeParameters.map((parameter) => rebaseNodejsProviderTypeParameter(parameter, mapType)) }),
    ...(declaration.heritage === undefined
      ? {}
      : { heritage: declaration.heritage.map((entry) => ({ ...entry, type: mapType(entry.type) })) }),
    ...(declaration.signatures === undefined
      ? {}
      : { signatures: declaration.signatures.map((signature) => rebaseNodejsProviderSignature(signature, mapType)) }),
    ...(declaration.members === undefined
      ? {}
      : { members: declaration.members.map((member) => rebaseNodejsProviderMember(member, mapType)) }),
  };
}

function rebaseNodejsProviderMember(
  member: ProviderMemberDeclaration,
  mapType: (type: ProviderTypeExpression) => ProviderTypeExpression,
): ProviderMemberDeclaration {
  return {
    ...member,
    ...(member.type === undefined ? {} : { type: mapType(member.type) }),
    ...(member.signatures === undefined
      ? {}
      : { signatures: member.signatures.map((signature) => rebaseNodejsProviderSignature(signature, mapType)) }),
  };
}

function rebaseNodejsProviderSignature(
  signature: ProviderSignatureDeclaration,
  mapType: (type: ProviderTypeExpression) => ProviderTypeExpression,
): ProviderSignatureDeclaration {
  return {
    ...signature,
    parameters: signature.parameters.map((parameter) => rebaseNodejsProviderParameter(parameter, mapType)),
    ...(signature.returnType === undefined ? {} : { returnType: mapType(signature.returnType) }),
    ...(signature.typeParameters === undefined
      ? {}
      : { typeParameters: signature.typeParameters.map((parameter) => rebaseNodejsProviderTypeParameter(parameter, mapType)) }),
  };
}

function rebaseNodejsProviderParameter(
  parameter: ProviderParameterDeclaration,
  mapType: (type: ProviderTypeExpression) => ProviderTypeExpression,
): ProviderParameterDeclaration {
  return {
    ...parameter,
    type: mapType(parameter.type),
    ...(parameter.defaultType === undefined ? {} : { defaultType: mapType(parameter.defaultType) }),
  };
}

function rebaseNodejsProviderTypeParameter(
  parameter: ProviderTypeParameterDeclaration,
  mapType: (type: ProviderTypeExpression) => ProviderTypeExpression,
): ProviderTypeParameterDeclaration {
  return {
    ...parameter,
    ...(parameter.constraints === undefined ? {} : { constraints: parameter.constraints.map(mapType) }),
    ...(parameter.defaultType === undefined ? {} : { defaultType: mapType(parameter.defaultType) }),
  };
}

function rebaseNodejsProviderType(
  type: ProviderTypeExpression,
  canonicalModuleSpecifier: string,
  publicModuleSpecifier: string,
): ProviderTypeExpression {
  const mapType = (nested: ProviderTypeExpression): ProviderTypeExpression =>
    rebaseNodejsProviderType(nested, canonicalModuleSpecifier, publicModuleSpecifier);
  switch (type.kind) {
    case "provider-ref":
      return {
        ...type,
        moduleSpecifier: type.moduleSpecifier === canonicalModuleSpecifier
          ? publicModuleSpecifier
          : type.moduleSpecifier,
        ...(type.typeArguments === undefined ? {} : { typeArguments: type.typeArguments.map(mapType) }),
      };
    case "target-named":
      return {
        ...type,
        ...(type.typeArguments === undefined ? {} : { typeArguments: type.typeArguments.map(mapType) }),
        ...(type.sourceShape === undefined ? {} : { sourceShape: mapType(type.sourceShape) }),
      };
    case "array":
      return { ...type, elementType: mapType(type.elementType) };
    case "tuple":
      return { ...type, elementTypes: type.elementTypes.map(mapType) };
    case "union":
    case "intersection":
      return { ...type, types: type.types.map(mapType) };
    case "function":
      return {
        ...type,
        parameters: type.parameters.map((parameter) => rebaseNodejsProviderParameter(parameter, mapType)),
        returnType: mapType(type.returnType),
        ...(type.typeParameters === undefined
          ? {}
          : { typeParameters: type.typeParameters.map((parameter) => rebaseNodejsProviderTypeParameter(parameter, mapType)) }),
      };
    case "opaque":
      return type.sourceShape === undefined
        ? type
        : { ...type, sourceShape: mapType(type.sourceShape) };
    default:
      return type;
  }
}

function nodejsProviderImportsForExports(
  moduleSpecifier: string,
  exports: readonly ProviderExportDeclaration[],
): readonly ProviderImportDeclaration[] {
  const importsByModule = new Map<string, Map<string, ProviderRequestedExport>>();
  const addImport = (type: ProviderTypeExpression): void => {
    if (type.kind !== "provider-ref") {
      return;
    }
    const canonicalSpecifier = canonicalNodejsModuleSpecifier(type.moduleSpecifier);
    if (canonicalSpecifier === undefined) {
      return;
    }
    if (canonicalSpecifier === moduleSpecifier) {
      return;
    }
    const moduleImports = importsByModule.get(canonicalSpecifier) ?? new Map<string, ProviderRequestedExport>();
    const request = {
      exportedName: type.exportName,
      ...(type.localName !== undefined ? { localName: type.localName } : {}),
      kind: "type",
    } satisfies ProviderRequestedExport;
    moduleImports.set(`${request.exportedName}\u0000${request.localName ?? ""}`, request);
    importsByModule.set(canonicalSpecifier, moduleImports);
  };
  for (const declaration of exports) {
    visitProviderExportTypes(declaration, addImport);
  }
  return [...importsByModule.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([importModuleSpecifier, requestedExports]) => ({
      moduleSpecifier: importModuleSpecifier,
      namedImports: [...requestedExports.values()].sort((left, right) =>
        left.exportedName === right.exportedName
          ? (left.localName ?? "").localeCompare(right.localName ?? "")
          : left.exportedName.localeCompare(right.exportedName)
      ),
      typeOnly: true,
    }));
}

function visitProviderExportTypes(
  declaration: ProviderExportDeclaration,
  visit: (type: ProviderTypeExpression) => void,
): void {
  visitOptionalProviderType(declaration.type, visit);
  visitProviderTypeParameters(declaration.typeParameters, visit);
  for (const heritage of declaration.heritage ?? []) {
    visitProviderType(heritage.type, visit);
  }
  for (const signature of declaration.signatures ?? []) {
    visitProviderTypeParameters(signature.typeParameters, visit);
    for (const parameter of signature.parameters) {
      visitProviderType(parameter.type, visit);
      visitOptionalProviderType(parameter.defaultType, visit);
    }
    visitOptionalProviderType(signature.returnType, visit);
  }
  for (const member of declaration.members ?? []) {
    visitOptionalProviderType(member.type, visit);
    for (const signature of member.signatures ?? []) {
      visitProviderTypeParameters(signature.typeParameters, visit);
      for (const parameter of signature.parameters) {
        visitProviderType(parameter.type, visit);
        visitOptionalProviderType(parameter.defaultType, visit);
      }
      visitOptionalProviderType(signature.returnType, visit);
    }
  }
}

function visitProviderTypeParameters(
  typeParameters: readonly { readonly constraints?: readonly ProviderTypeExpression[]; readonly defaultType?: ProviderTypeExpression }[] | undefined,
  visit: (type: ProviderTypeExpression) => void,
): void {
  for (const typeParameter of typeParameters ?? []) {
    for (const constraint of typeParameter.constraints ?? []) {
      visitProviderType(constraint, visit);
    }
    visitOptionalProviderType(typeParameter.defaultType, visit);
  }
}

function visitOptionalProviderType(
  type: ProviderTypeExpression | undefined,
  visit: (type: ProviderTypeExpression) => void,
): void {
  if (type !== undefined) {
    visitProviderType(type, visit);
  }
}

function visitProviderType(
  type: ProviderTypeExpression,
  visit: (type: ProviderTypeExpression) => void,
): void {
  visit(type);
  switch (type.kind) {
    case "array":
      visitProviderType(type.elementType, visit);
      return;
    case "tuple":
      for (const elementType of type.elementTypes) {
        visitProviderType(elementType, visit);
      }
      return;
    case "union":
    case "intersection":
      for (const child of type.types) {
        visitProviderType(child, visit);
      }
      return;
    case "function":
      visitProviderTypeParameters(type.typeParameters, visit);
      for (const parameter of type.parameters) {
        visitProviderType(parameter.type, visit);
        visitOptionalProviderType(parameter.defaultType, visit);
      }
      visitProviderType(type.returnType, visit);
      return;
    case "provider-ref":
      for (const typeArgument of type.typeArguments ?? []) {
        visitProviderType(typeArgument, visit);
      }
      return;
    case "target-named":
      for (const typeArgument of type.typeArguments ?? []) {
        visitProviderType(typeArgument, visit);
      }
      visitOptionalProviderType(type.sourceShape, visit);
      return;
    default:
      return;
  }
}

function nodejsProviderDiagnostic(
  extensionCode: string,
  numericCode: number,
  message: string,
): ExtensionDiagnostic {
  return {
    extensionId: csharpNodejsProviderPackageProviderIdentity.id,
    extensionCode,
    numericCode,
    category: "error",
    message,
  };
}
