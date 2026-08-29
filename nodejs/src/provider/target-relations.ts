import type {
  ProviderParameterDeclaration,
} from "@tsonic/tsts";
import {
  csharpQualifiedTypeRenderShape,
  csharpJsEventLoopBinaryExecutionDriver,
  csharpProviderPolicyContribution,
  csharpSourcePrimitiveTargetType,
  csharpTargetNamedType,
  getCsharpNullableElementTargetType,
} from "@tsonic/target-csharp/provider";
import type {
  CsharpProviderArgumentAdapter,
  CsharpProviderParameterRelation,
  CsharpProviderSourceIdentity,
  CsharpProviderSourceIdentityBase,
  CsharpProviderTargetRejectionDiagnostic,
  CsharpProviderTargetRejection,
  CsharpProviderTargetRelation,
  CsharpTargetBindingFact,
  CsharpTargetMember,
  TargetTypeRef,
} from "@tsonic/target-csharp/provider";
import {
  csharpNodejsProviderPackageProviderIdentity,
} from "./identity.js";
import type {
  NodejsProviderDeclarationIdentity,
} from "./identity.js";
import {
  nodejsTargetMemberMetadataRecords,
  nodejsUnsupportedTargetMetadataRecords,
} from "./members/provider-records.js";
import type {
  NodejsUnsupportedTargetMetadataRecord,
} from "./members/provider-records.js";
import {
  nodejsPublicModuleSpecifiers,
} from "./module-specifiers.js";
import {
  nodejsProviderTargetBinding,
  nodejsProviderTargetTypeRows,
} from "./target-bindings.js";
import {
  nodejsCanonicalProviderExports,
} from "./provider.js";

export function createCsharpNodejsProviderPolicyContribution() {
  return csharpProviderPolicyContribution(
    csharpNodejsProviderPackageProviderIdentity.id,
    csharpNodejsProviderPackageProviderIdentity.version,
    nodejsProviderTargetRelations(),
    nodejsProviderTargetRejections(),
    csharpJsEventLoopBinaryExecutionDriver,
  );
}

export function nodejsProviderTargetRelations():
  readonly CsharpProviderTargetRelation[] {
  return Object.freeze([
    ...nodejsProviderTypeRelations(),
    ...nodejsProviderMemberRelations(),
  ]);
}

export function nodejsProviderTargetRejections():
  readonly CsharpProviderTargetRejection[] {
  return Object.freeze(
    nodejsUnsupportedTargetMetadataRecords().flatMap((record) =>
      record.declarationIdentities.flatMap((identity) =>
        nodejsPublicModuleSpecifiers(identity.providerModuleId).map(
          (moduleSpecifier) =>
            nodejsProviderTargetRejection(record, identity, moduleSpecifier),
        ),
      )
    ),
  );
}

function nodejsProviderTypeRelations():
  readonly CsharpProviderTargetRelation[] {
  return nodejsProviderTargetTypeRows.flatMap((row) => {
    const binding = nodejsProviderTargetBinding(
      row.moduleSpecifier,
      row.exportName,
    );
    if (binding === undefined) {
      throw new Error(
        `Missing C# NodeJS target binding for '${row.moduleSpecifier}' export '${row.exportName}'.`,
      );
    }
    return nodejsPublicModuleSpecifiers(row.moduleSpecifier).map(
      (moduleSpecifier): CsharpProviderTargetRelation => ({
        kind: "type",
        source: {
          kind: "type",
          ...providerSourceIdentityBase(
            {
              providerId: csharpNodejsProviderPackageProviderIdentity.id,
              providerVersion:
                csharpNodejsProviderPackageProviderIdentity.version,
              providerModuleId: row.moduleSpecifier,
              moduleSpecifier: row.moduleSpecifier,
              artifactFileName: "",
              exportId: `${row.moduleSpecifier}.${row.exportName}`,
              exportName: row.exportName,
            },
            moduleSpecifier,
          ),
        },
        targetBinding: binding,
        bindingTypeParameters: [],
        ...(row.objectLiteralConstruction === undefined
          ? {}
          : {
              objectLiteralConstruction: {
                kind: row.objectLiteralConstruction,
              },
            }),
      }),
    );
  });
}

function nodejsProviderMemberRelations():
  readonly CsharpProviderTargetRelation[] {
  return nodejsTargetMemberMetadataRecords().flatMap((record) =>
    record.declarationIdentities.flatMap((identity) =>
      nodejsPublicModuleSpecifiers(identity.providerModuleId).map(
        (moduleSpecifier) =>
          nodejsProviderTargetRelation(
            identity,
            moduleSpecifier,
            record.member,
          ),
      ),
    ));
}

function nodejsProviderTargetRelation(
  identity: NodejsProviderDeclarationIdentity,
  moduleSpecifier: string,
  targetMember: CsharpTargetMember,
): CsharpProviderTargetRelation {
  const source = nodejsProviderSourceIdentity(identity, moduleSpecifier);
  const targetBinding = targetBindingForMember(identity, targetMember);
  if (source.kind === "signature") {
    return {
      kind: "signature",
      source,
      targetBinding,
      targetMember,
      receiver: targetReceiver(identity, targetMember),
      parameters: providerParameterRelations(
        identity,
        providerSignatureParameters(identity),
        targetMember,
      ),
      bindingTypeParameters: [],
      bindingTypeArgumentSource:
        providerBindingTypeArgumentSource(targetMember),
      methodTypeParameters: [],
      invocationTypeParameters: [],
      selectedTypeParameterCount: 0,
    };
  }
  if (source.kind === "member") {
    return {
      kind: "member",
      source,
      targetBinding,
      targetMember,
      receiver: targetReceiver(identity, targetMember),
      bindingTypeParameters: [],
      bindingTypeArgumentSource:
        targetMember.static === true ? "callee" : "receiver",
    };
  }
  if (targetMember.static !== true) {
    throw new Error(
      `C# NodeJS provider export value '${identity.providerModuleId}:${identity.exportName ?? "<export>"}' must relate to a static target member.`,
    );
  }
  return {
    kind: "value",
    source,
    targetBinding,
    targetMember,
  };
}

function nodejsProviderTargetRejection(
  record: NodejsUnsupportedTargetMetadataRecord,
  identity: NodejsProviderDeclarationIdentity,
  moduleSpecifier: string,
): CsharpProviderTargetRejection {
  const source = nodejsProviderSourceIdentity(identity, moduleSpecifier);
  return {
    source,
    diagnostic: {
      extensionId: csharpNodejsProviderPackageProviderIdentity.id,
      extensionCode: "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED",
      numericCode: 9100203,
      category: "error",
      message:
        `C# NodeJS provider package hard-rejected selected ${sourceOperationKind(source)} ${formatProviderSourceIdentity(source)}: ${record.identity.displayName} has no closed target/runtime operation metadata.`,
      evidence: [{
        message:
          `Selected provider target policy rejection '${record.identity.targetIdentityId}'.`,
      }],
    } satisfies CsharpProviderTargetRejectionDiagnostic,
  };
}

function nodejsProviderSourceIdentity(
  identity: NodejsProviderDeclarationIdentity,
  moduleSpecifier: string,
): Exclude<CsharpProviderSourceIdentity, { readonly kind: "type" }> {
  const base = providerSourceIdentityBase(identity, moduleSpecifier);
  if (identity.signatureId !== undefined) {
    return {
      kind: "signature",
      ...base,
      ...providerMemberIdentity(identity),
      signatureId: identity.signatureId,
    };
  }
  if (identity.memberId !== undefined) {
    const member = providerMemberIdentity(identity);
    if (
      member.memberId === undefined ||
      member.memberStatic === undefined ||
      member.memberKey === undefined
    ) {
      throw new Error(
        `Incomplete C# NodeJS provider member identity '${identity.providerModuleId}:${identity.exportName ?? "<export>"}:${identity.memberId}'.`,
      );
    }
    return {
      kind: "member",
      ...base,
      memberId: member.memberId,
      memberStatic: member.memberStatic,
      memberKey: member.memberKey,
    };
  }
  return { kind: "value", ...base };
}

function sourceOperationKind(source: CsharpProviderSourceIdentity): string {
  return source.kind === "member" || source.kind === "value"
    ? "property"
    : source.kind === "signature"
      ? "call"
      : "type";
}

function formatProviderSourceIdentity(
  source: CsharpProviderSourceIdentity,
): string {
  const member = source.kind === "member" || source.kind === "signature"
    ? ` member '${source.memberId ?? "<export>"}'`
    : "";
  const signature = source.kind === "signature"
    ? ` signature '${source.signatureId}'`
    : "";
  return `'${source.moduleSpecifier}' export '${source.exportName}'${member}${signature}`;
}

function providerBindingTypeArgumentSource(
  member: CsharpTargetMember,
): Extract<
  CsharpProviderTargetRelation,
  { readonly kind: "member" | "signature" }
>["bindingTypeArgumentSource"] {
  return member.kind === "constructor"
    ? "selected-operation-type-arguments"
    : member.static === true
      ? "callee"
      : "receiver";
}

function providerSignatureParameters(
  identity: NodejsProviderDeclarationIdentity,
): readonly ProviderParameterDeclaration[] {
  const exports = nodejsCanonicalProviderExports(identity.providerModuleId);
  const declaration = exports?.find((candidate) =>
    candidate.id === identity.exportId);
  const signature = identity.memberId === undefined
    ? declaration?.signatures?.find((candidate) =>
        candidate.id === identity.signatureId)
    : declaration?.members
        ?.find((candidate) => candidate.id === identity.memberId)
        ?.signatures?.find((candidate) =>
          candidate.id === identity.signatureId);
  if (signature === undefined) {
    throw new Error(
      `C# NodeJS provider relation '${formatProviderIdentity(identity)}' does not identify an exact source signature.`,
    );
  }
  return signature.parameters;
}

function providerSourceIdentityBase(
  identity: NodejsProviderDeclarationIdentity,
  moduleSpecifier: string,
): CsharpProviderSourceIdentityBase {
  if (
    identity.exportId === undefined ||
    identity.exportName === undefined
  ) {
    throw new Error(
      `Incomplete C# NodeJS provider export identity '${identity.providerModuleId}:${identity.exportName ?? "<export>"}'.`,
    );
  }
  return {
    providerId: identity.providerId,
    providerVersion: identity.providerVersion,
    providerModuleId: identity.providerModuleId,
    moduleSpecifier,
    exportId: identity.exportId,
    exportName: identity.exportName,
  };
}

function providerMemberIdentity(
  identity: NodejsProviderDeclarationIdentity,
): Pick<
  Extract<
    CsharpProviderTargetRelation,
    { readonly kind: "member" | "signature" }
  >["source"],
  "memberId" | "memberStatic" | "memberKey"
> {
  const present = identity.memberId !== undefined ||
    identity.memberStatic !== undefined ||
    identity.memberKey !== undefined;
  if (!present) {
    return {};
  }
  if (
    identity.memberId === undefined ||
    identity.memberStatic === undefined ||
    identity.memberKey === undefined
  ) {
    throw new Error(
      `Incomplete C# NodeJS provider member identity '${identity.providerModuleId}:${identity.exportName ?? "<export>"}'.`,
    );
  }
  return {
    memberId: identity.memberId,
    memberStatic: identity.memberStatic,
    memberKey: identity.memberKey,
  };
}

function targetBindingForMember(
  identity: NodejsProviderDeclarationIdentity,
  member: CsharpTargetMember,
): CsharpTargetBindingFact {
  const exportedBinding = identity.exportName === undefined
    ? undefined
    : nodejsProviderTargetBinding(
        identity.providerModuleId,
        identity.exportName,
      );
  if (exportedBinding !== undefined) {
    return exportedBinding;
  }
  const declaringType = member.declaringType;
  if (declaringType?.kind !== "target-named") {
    throw new Error(
      `C# NodeJS target member '${member.id}' has no exact target owner binding.`,
    );
  }
  return Object.freeze({
    id: declaringType.id,
    sourceName: declaringType.id,
    targetName: declaringType.id,
    target: "csharp",
    kind: "class",
    csharpType: declaringType,
  });
}

function targetReceiver(
  identity: NodejsProviderDeclarationIdentity,
  member: CsharpTargetMember,
): Extract<
  CsharpProviderTargetRelation,
  { readonly kind: "member" | "signature" }
>["receiver"] {
  if (
    identity.memberId === undefined ||
    member.static === true ||
    member.kind === "constructor"
  ) {
    return { kind: "none" };
  }
  return { kind: "instance" };
}

function providerParameterRelations(
  identity: NodejsProviderDeclarationIdentity,
  sourceParameters: readonly ProviderParameterDeclaration[] | undefined,
  member: CsharpTargetMember,
): readonly CsharpProviderParameterRelation[] {
  if (sourceParameters === undefined) {
    throw new Error(
      `C# NodeJS provider signature '${identity.signatureId ?? "<missing>"}' has no canonical source parameter model.`,
    );
  }
  if (sourceParameters.length !== member.parameters.length) {
    throw new Error(
      `C# NodeJS provider signature '${identity.signatureId ?? "<missing>"}' exposes ${sourceParameters.length} source parameters but relates to ${member.parameters.length} target parameters.`,
    );
  }
  return Object.freeze(sourceParameters.map((source, index) => {
    const target = member.parameters[index]!;
    const argumentAdapter = nodejsProviderArgumentAdapter(
      source,
      target.type,
    );
    return {
      sourceParameterIndex: index,
      targetParameterIndex: index,
      sourcePassingMode: source.passingMode ?? "by-value",
      targetPassingMode: target.passingMode,
      sourceAcceptsOmission:
        source.optional === true ||
        source.defaultType !== undefined ||
        source.rest === true,
      targetAcceptsOmission:
        target.optional === true ||
        target.csharpOmittableOptionalArgument === true ||
        target.paramsArray === true,
      sourceRest: source.rest === true,
      targetParamsArray: target.paramsArray === true,
      ...(argumentAdapter === undefined ? {} : { argumentAdapter }),
    };
  }));
}

function nodejsProviderArgumentAdapter(
  source: ProviderParameterDeclaration,
  target: TargetTypeRef,
): CsharpProviderArgumentAdapter | undefined {
  if (source.type.kind !== "number") {
    return undefined;
  }
  const resultType = getCsharpNullableElementTargetType(target) ?? target;
  if (resultType.kind !== "source-primitive") {
    return undefined;
  }
  const targetName = systemConvertTargetName(resultType.name);
  if (targetName === undefined) {
    return undefined;
  }
  const inputType = csharpSourcePrimitiveTargetType("float64");
  return Object.freeze({
    kind: "static-method",
    id: `System.Convert.${targetName}(System.Double)`,
    declaringType: csharpTargetNamedType(
      "System.Convert",
      undefined,
      csharpQualifiedTypeRenderShape("System", "Convert"),
    ),
    targetName,
    inputType,
    resultType,
  });
}

function systemConvertTargetName(
  kind: Extract<TargetTypeRef, { readonly kind: "source-primitive" }>["name"],
): string | undefined {
  switch (kind) {
    case "bool":
      return "ToBoolean";
    case "int8":
      return "ToSByte";
    case "uint8":
      return "ToByte";
    case "int16":
      return "ToInt16";
    case "uint16":
      return "ToUInt16";
    case "int32":
    case "native-int":
      return "ToInt32";
    case "uint32":
    case "native-uint":
      return "ToUInt32";
    case "int64":
      return "ToInt64";
    case "uint64":
      return "ToUInt64";
    case "float16":
    case "float32":
      return "ToSingle";
    case "float64":
      return undefined;
    case "decimal":
      return "ToDecimal";
    case "char":
    case "int128":
    case "uint128":
      return undefined;
  }
}

function formatProviderIdentity(
  identity: NodejsProviderDeclarationIdentity,
): string {
  return [
    identity.providerModuleId,
    identity.exportId ?? "<export>",
    identity.memberId ?? "<member>",
    identity.signatureId ?? "<signature>",
  ].join("::");
}
