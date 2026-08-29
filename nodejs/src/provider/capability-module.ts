import type {
  ProviderExportDeclaration,
  ProviderMemberDeclaration,
  ProviderParameterDeclaration,
  ProviderSignatureDeclaration,
  ProviderTypeExpression,
} from "@tsonic/tsts";
import {
  csharpQualifiedTypeRenderShape,
  csharpTargetNamedType,
} from "@tsonic/target-csharp/provider";
import type {
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  nodejsDefaultModuleObjectExports,
} from "./module-defaults.js";
import type {
  NodejsClassCallTargetMetadata,
  NodejsClassPropertyTargetMetadata,
  NodejsModuleCallTargetMetadata,
  NodejsModulePropertyTargetMetadata,
} from "./members/target-member-metadata.js";

export const stringProviderType = Object.freeze({ kind: "string" }) satisfies ProviderTypeExpression;
export const numberProviderType = Object.freeze({ kind: "number" }) satisfies ProviderTypeExpression;
export const booleanProviderType = Object.freeze({ kind: "boolean" }) satisfies ProviderTypeExpression;
export const voidProviderType = Object.freeze({ kind: "void" }) satisfies ProviderTypeExpression;
export const unknownProviderType = Object.freeze({ kind: "unknown" }) satisfies ProviderTypeExpression;
export const undefinedProviderType = Object.freeze({ kind: "undefined" }) satisfies ProviderTypeExpression;

export function nodejsCapabilityModuleExports(options: {
  readonly moduleSpecifier: string;
  readonly moduleCalls?: readonly NodejsModuleCallTargetMetadata[];
  readonly moduleProperties?: readonly NodejsModulePropertyTargetMetadata[];
  readonly classCalls?: readonly NodejsClassCallTargetMetadata[];
  readonly classProperties?: readonly NodejsClassPropertyTargetMetadata[];
  readonly classes?: readonly string[];
  readonly classHeritage?: Readonly<Record<string, readonly ProviderTypeExpression[]>>;
  readonly additionalExports?: readonly ProviderExportDeclaration[];
  readonly includeDefaultExports?: boolean;
}): readonly ProviderExportDeclaration[] {
  const moduleCalls = options.moduleCalls ?? [];
  const moduleProperties = options.moduleProperties ?? [];
  const classCalls = options.classCalls ?? [];
  const classProperties = options.classProperties ?? [];
  const classNames = new Set([
    ...(options.classes ?? []),
    ...classCalls.map((member) => member.exportName),
  ]);
  const exports: readonly ProviderExportDeclaration[] = [
    ...providerModuleFunctions(options.moduleSpecifier, moduleCalls),
    ...moduleProperties.map((member): ProviderExportDeclaration => ({
      id: `${options.moduleSpecifier}.${member.exportName}`,
      name: member.exportName,
      kind: "value",
      type: member.providerType,
    })),
    ...[...classNames].sort().map((exportName): ProviderExportDeclaration => ({
      id: `${options.moduleSpecifier}.${exportName}`,
      name: exportName,
      kind: "class",
      ...((options.classHeritage?.[exportName]?.length ?? 0) === 0
        ? {}
        : {
            heritage: options.classHeritage![exportName]!.map((type) => ({
              kind: "extends" as const,
              type,
            })),
          }),
      members: [
        ...providerClassCallMembers(classCalls.filter((member) => member.exportName === exportName)),
        ...providerClassPropertyMembers(classProperties.filter((member) => member.exportName === exportName)),
      ],
    })),
    ...(options.additionalExports ?? []),
  ];
  return options.includeDefaultExports === false
    ? Object.freeze(exports)
    : Object.freeze([
        ...exports,
        ...nodejsDefaultModuleObjectExports(options.moduleSpecifier, exports),
      ]);
}

export function providerRef(
  moduleSpecifier: string,
  exportName: string,
  typeArguments?: readonly ProviderTypeExpression[],
): ProviderTypeExpression {
  return {
    kind: "provider-ref",
    moduleSpecifier,
    exportName,
    ...(typeArguments === undefined ? {} : { typeArguments }),
  };
}

export function callbackProviderType(
  id: string,
  parameters: readonly ProviderParameterDeclaration[],
  returnType: ProviderTypeExpression = voidProviderType,
): ProviderTypeExpression {
  return { kind: "function", id, parameters, returnType };
}

export function arrayProviderType(elementType: ProviderTypeExpression): ProviderTypeExpression {
  return { kind: "array", elementType };
}

export function unionProviderType(
  ...types: readonly ProviderTypeExpression[]
): ProviderTypeExpression {
  return { kind: "union", types };
}

export function nodejsTargetNamedType(
  namespace: string,
  name: string,
  id = `${namespace}.${name}`,
): TargetTypeRef {
  return csharpTargetNamedType(
    id,
    undefined,
    csharpQualifiedTypeRenderShape(namespace, name),
  );
}

function providerModuleFunctions(
  moduleSpecifier: string,
  members: readonly NodejsModuleCallTargetMetadata[],
): readonly ProviderExportDeclaration[] {
  return groupedBy(members, (member) => member.exportName).map(
    ([exportName, overloads]): ProviderExportDeclaration => ({
      id: `${moduleSpecifier}.${exportName}`,
      name: exportName,
      kind: "function",
      signatures: overloads.map((member) => nodejsProviderSignature(
        member.signatureId,
        member.providerParameters,
        member.providerReturnType,
      )),
    }),
  );
}

function providerClassCallMembers(
  members: readonly NodejsClassCallTargetMetadata[],
): readonly ProviderMemberDeclaration[] {
  return groupedBy(members, (member) => member.memberId).map(([, overloads]) => {
    const first = overloads[0]!;
    return {
      id: first.memberId,
      name: first.memberName,
      kind: first.memberKind,
      signatures: overloads.map((member) => nodejsProviderSignature(
        member.signatureId,
        member.providerParameters,
        member.providerReturnType,
      )),
    };
  });
}

export function nodejsProviderSignature(
  signatureId: string,
  parameters: readonly ProviderParameterDeclaration[],
  returnType: ProviderTypeExpression | undefined,
): ProviderSignatureDeclaration {
  return {
    id: signatureId,
    parameters: parameters.map((parameter, index) => ({
      ...parameter,
      type: scopeProviderCallableType(
        parameter.type,
        `${signatureId}.parameter[${index}]`,
      ),
      ...(parameter.defaultType === undefined
        ? {}
        : {
            defaultType: scopeProviderCallableType(
              parameter.defaultType,
              `${signatureId}.parameter[${index}].default`,
            ),
          }),
    })),
    ...(returnType === undefined
      ? {}
      : {
          returnType: scopeProviderCallableType(
            returnType,
            `${signatureId}.return`,
          ),
        }),
  };
}

function scopeProviderCallableType(
  type: ProviderTypeExpression,
  occurrenceIdentity: string,
): ProviderTypeExpression {
  switch (type.kind) {
    case "function":
      return {
        ...type,
        id: occurrenceIdentity,
        parameters: type.parameters.map((parameter, index) => ({
          ...parameter,
          type: scopeProviderCallableType(
            parameter.type,
            `${occurrenceIdentity}.parameter[${index}]`,
          ),
          ...(parameter.defaultType === undefined
            ? {}
            : {
                defaultType: scopeProviderCallableType(
                  parameter.defaultType,
                  `${occurrenceIdentity}.parameter[${index}].default`,
                ),
              }),
        })),
        returnType: scopeProviderCallableType(
          type.returnType,
          `${occurrenceIdentity}.return`,
        ),
        ...(type.typeParameters === undefined
          ? {}
          : {
              typeParameters: type.typeParameters.map((parameter, index) => ({
                ...parameter,
                ...(parameter.constraints === undefined
                  ? {}
                  : {
                      constraints: parameter.constraints.map((constraint, constraintIndex) =>
                        scopeProviderCallableType(
                          constraint,
                          `${occurrenceIdentity}.typeParameter[${index}].constraint[${constraintIndex}]`,
                        )),
                    }),
                ...(parameter.defaultType === undefined
                  ? {}
                  : {
                      defaultType: scopeProviderCallableType(
                        parameter.defaultType,
                        `${occurrenceIdentity}.typeParameter[${index}].default`,
                      ),
                    }),
              })),
            }),
      };
    case "array":
      return {
        ...type,
        elementType: scopeProviderCallableType(
          type.elementType,
          `${occurrenceIdentity}.element`,
        ),
      };
    case "tuple":
      return {
        ...type,
        elementTypes: type.elementTypes.map((elementType, index) =>
          scopeProviderCallableType(
            elementType,
            `${occurrenceIdentity}.element[${index}]`,
          )),
      };
    case "union":
    case "intersection":
      return {
        ...type,
        types: type.types.map((memberType, index) =>
          scopeProviderCallableType(
            memberType,
            `${occurrenceIdentity}.member[${index}]`,
          )),
      };
    case "provider-ref":
    case "source-global":
      return {
        ...type,
        ...(type.typeArguments === undefined
          ? {}
          : {
              typeArguments: type.typeArguments.map((argument, index) =>
                scopeProviderCallableType(
                  argument,
                  `${occurrenceIdentity}.typeArgument[${index}]`,
                )),
            }),
      };
    case "any":
    case "unknown":
    case "void":
    case "never":
    case "undefined":
    case "boolean":
    case "string":
    case "number":
    case "bigint":
    case "object":
    case "literal":
    case "source-primitive":
    case "type-parameter":
      return type;
  }
}

function providerClassPropertyMembers(
  members: readonly NodejsClassPropertyTargetMetadata[],
): readonly ProviderMemberDeclaration[] {
  return members.map((member) => ({
    id: member.memberId,
    name: member.memberName,
    kind: "property" as const,
    ...(member.readonly === true ? { readonly: true } : {}),
    ...(member.optional === true ? { optional: true } : {}),
    type: member.providerType,
  }));
}

function groupedBy<T>(
  entries: readonly T[],
  key: (entry: T) => string,
): readonly [string, readonly T[]][] {
  const groups = new Map<string, T[]>();
  for (const entry of entries) {
    const entryKey = key(entry);
    groups.set(entryKey, [...(groups.get(entryKey) ?? []), entry]);
  }
  return [...groups.entries()];
}
