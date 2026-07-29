import type {
  ProviderParameterDeclaration,
} from "@tsonic/tsts";
import {
  csharpProviderRelationsContribution,
} from "@tsonic/target-csharp";
import type {
  CsharpProviderParameterRelation,
  CsharpProviderSourceIdentityBase,
  CsharpProviderTargetRelation,
  CsharpTargetBindingFact,
  CsharpTargetMember,
} from "@tsonic/target-csharp";
import {
  csharpNodejsProviderPackageProviderIdentity,
} from "./identity.js";
import type {
  NodejsProviderDeclarationIdentity,
} from "./identity.js";
import {
  nodejsTargetMemberMetadataRecords,
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

export function createCsharpNodejsProviderRelationsContribution() {
  return csharpProviderRelationsContribution(
    csharpNodejsProviderPackageProviderIdentity.id,
    csharpNodejsProviderPackageProviderIdentity.version,
    nodejsProviderTargetRelations(),
  );
}

export function nodejsProviderTargetRelations():
  readonly CsharpProviderTargetRelation[] {
  return Object.freeze([
    ...nodejsProviderTypeRelations(),
    ...nodejsProviderMemberRelations(),
  ]);
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
  const source = providerSourceIdentityBase(identity, moduleSpecifier);
  const targetBinding = targetBindingForMember(identity, targetMember);
  if (identity.signatureId !== undefined) {
    return {
      kind: "signature",
      source: {
        kind: "signature",
        ...source,
        ...providerMemberIdentity(identity),
        signatureId: identity.signatureId,
      },
      targetBinding,
      targetMember,
      receiver: targetReceiver(identity, targetMember),
      parameters: providerParameterRelations(
        identity,
        providerSignatureParameters(identity),
        targetMember,
      ),
      bindingTypeParameters: [],
      methodTypeParameters: [],
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
      source: {
        kind: "member",
        ...source,
        memberId: member.memberId,
        memberStatic: member.memberStatic,
        memberKey: member.memberKey,
      },
      targetBinding,
      targetMember,
      receiver: targetReceiver(identity, targetMember),
      bindingTypeParameters: [],
    };
  }
  if (targetMember.static !== true) {
    throw new Error(
      `C# NodeJS provider export value '${identity.providerModuleId}:${identity.exportName ?? "<export>"}' must relate to a static target member.`,
    );
  }
  return {
    kind: "value",
    source: { kind: "value", ...source },
    targetBinding,
    targetMember,
  };
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
    return {
      sourceParameterIndex: index,
      targetParameterIndex: index,
      sourcePassingMode: source.passingMode ?? "by-value",
      targetPassingMode: target.passingMode,
      sourceAcceptsOmission:
        source.optional === true || source.defaultType !== undefined,
      targetAcceptsOmission:
        target.optional === true ||
        target.csharpOmittableOptionalArgument === true,
      sourceRest: source.rest === true,
      targetParamsArray: target.paramsArray === true,
    };
  }));
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
