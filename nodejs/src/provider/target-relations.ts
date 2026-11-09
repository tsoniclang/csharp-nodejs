import { csharpJsEventLoopBinaryExecutionDriver, csharpProviderPolicyContribution } from "@tsonic/target-csharp/provider";
import type {
  CsharpAcquiredRuntimeProviderNativeDomain, CsharpProviderTargetRelation, CsharpProviderTargetRejection,
  CsharpProviderTypeParameterRelation, CsharpTargetBindingFact,
} from "@tsonic/target-csharp/provider";
import { csharpNodejsProviderPackageProviderIdentity, nodejsExportDeclarationIdentity } from "./model/identity.js";
import { nodejsTargetMemberMetadataRecords, nodejsUnsupportedTargetMetadataRecords } from "./assembly/member-records.js";
import { nodejsPublicModuleSpecifiers } from "./declarations/specifiers.js";
import { nodejsProviderSourceIdentity, nodejsProviderSourceIdentityBase, nodejsSourceRelationIndex } from "./declarations/relation-source.js";
import { nodejsNativeClassMappings } from "./modules/class-mappings.js";
import { nodejsNativeClassApplication } from "./native/applications.js";
import { createNodejsNativeSelection, rejected, resolved } from "./native/selection.js";
import type { NodejsNativeResolution } from "./native/selection.js";
import { nodejsParameterRelations, nodejsTypeParameterRelationsAreComplete } from "./native/parameter-relations.js";

interface NodejsClassBinding {
  readonly binding: CsharpTargetBindingFact;
  readonly parameters: readonly CsharpProviderTypeParameterRelation[];
  readonly sourceParameterCount: number;
}

export function createCsharpNodejsProviderPolicyContribution(
  domain: CsharpAcquiredRuntimeProviderNativeDomain,
  includeJsSurfaceMembers = true,
) {
  const selection = createNodejsNativeSelection(domain);
  const sources = nodejsSourceRelationIndex(includeJsSurfaceMembers);
  const bindings = new Map<string, NodejsNativeResolution<NodejsClassBinding>>();
  const relations: CsharpProviderTargetRelation[] = [];
  const rejections: CsharpProviderTargetRejection[] = [...nodejsProviderTargetRejections()];
  for (const mapping of nodejsNativeClassMappings) {
    const identity = nodejsExportDeclarationIdentity(mapping.moduleSpecifier, mapping.exportName);
    const declaration = sources.declaration(mapping.moduleSpecifier, `${mapping.moduleSpecifier}.${mapping.exportName}`);
    const recipe = mapping.application(includeJsSurfaceMembers);
    const application = nodejsNativeClassApplication(selection, recipe);
    const parameters = recipe.kind === "source-class" ? recipe.parameters : [];
    const sourceParameterCount = declaration?.typeParameters?.length ?? 0;
    const result: NodejsNativeResolution<NodejsClassBinding> = declaration === undefined
      ? rejected("A Node type mapping requires its original authored Source declaration.", mapping.exportName)
      : application.kind === "rejected" ? application
      : !nodejsTypeParameterRelationsAreComplete(parameters, sourceParameterCount, application.value.typeParameters?.length ?? 0)
        ? rejected("Node class binder roles cover the complete original Source vector and actual Native binder positions.", mapping.exportName)
        : resolved({ binding: application.value, parameters, sourceParameterCount });
    const key = bindingKey(mapping.moduleSpecifier, mapping.exportName);
    if (bindings.has(key)) throw new TypeError(`Duplicate Node class mapping '${key}'.`);
    bindings.set(key, result);
    for (const moduleSpecifier of nodejsPublicModuleSpecifiers(mapping.moduleSpecifier)) {
      const source = { kind: "type" as const, ...nodejsProviderSourceIdentityBase(identity, moduleSpecifier) };
      if (result.kind === "rejected") rejections.push({ source, diagnostic: result.diagnostic });
      else relations.push({ kind: "type", source, targetBinding: result.value.binding, bindingTypeParameters: result.value.parameters,
        ...(mapping.objectLiteralConstruction === undefined ? {} : { objectLiteralConstruction: { kind: mapping.objectLiteralConstruction } }) });
    }
  }
  for (const record of nodejsTargetMemberMetadataRecords(includeJsSurfaceMembers)) {
    const selected = selection.member(record.native.selected);
    for (const identity of record.declarationIdentities) {
      const classBinding = identity.exportName === undefined ? undefined : bindings.get(bindingKey(identity.providerModuleId, identity.exportName));
      for (const moduleSpecifier of nodejsPublicModuleSpecifiers(identity.providerModuleId)) {
        const source = nodejsProviderSourceIdentity(identity, moduleSpecifier);
        if (selected.kind === "rejected") {
          rejections.push({ source, diagnostic: selected.diagnostic });
          continue;
        }
        if (classBinding?.kind === "rejected") {
          rejections.push({ source, diagnostic: classBinding.diagnostic });
          continue;
        }
        const targetBinding = classBinding?.value.binding ?? selected.value.declaration.binding;
        const bindingTypeParameters = classBinding?.value.parameters ?? [];
        const bindingTypeArgumentSource = selected.value.member.static === true ? "callee" as const : "receiver" as const;
        if (source.kind === "value") {
          if (selected.value.member.static !== true) {
            rejections.push({ source, diagnostic: rejected("A Node exported value requires its actual static Native slot.", selected.value.member.id).diagnostic });
          } else relations.push({ kind: "value", source, targetBinding, targetMember: selected.value.member });
          continue;
        }
        if (source.kind === "member") {
          relations.push({ kind: "member", source, targetBinding, targetMember: selected.value.member,
            receiver: record.native.receiver, bindingTypeParameters, bindingTypeArgumentSource });
          continue;
        }
        const signature = sources.signature(identity);
        if (signature === undefined || !("parameters" in record.native)) {
          rejections.push({ source, diagnostic: rejected("A Node callable mapping requires its exact authored Source head and Native callable locator.", source.signatureId).diagnostic });
          continue;
        }
        const parameterRelations = nodejsParameterRelations(signature, selected.value, record.native);
        if (parameterRelations.kind === "rejected") {
          rejections.push({ source, diagnostic: parameterRelations.diagnostic });
          continue;
        }
        const sourceSignatureCount = signature.typeParameters?.length ?? 0;
        const methodRelations = [...record.native.methodTypeParameters, ...record.native.invocationTypeParameters];
        if (!nodejsTypeParameterRelationsAreComplete(methodRelations, sourceSignatureCount, selected.value.member.typeParameters?.length ?? 0)) {
          rejections.push({ source, diagnostic: rejected("Node callable binder roles retain the complete original Source and actual Native method vectors.", source.signatureId).diagnostic });
          continue;
        }
        relations.push({ kind: "signature", source, targetBinding, targetMember: selected.value.member,
          receiver: record.native.receiver, parameters: parameterRelations.value, bindingTypeParameters,
          bindingTypeArgumentSource: selected.value.member.kind === "constructor" ? "class-type-arguments" : bindingTypeArgumentSource,
          methodTypeParameters: record.native.methodTypeParameters, invocationTypeParameters: record.native.invocationTypeParameters,
          sourceTypeParameterCounts: { class: classBinding?.value.sourceParameterCount ?? 0, signature: sourceSignatureCount } });
      }
    }
  }
  return csharpProviderPolicyContribution({
    providerId: csharpNodejsProviderPackageProviderIdentity.id,
    providerVersion: csharpNodejsProviderPackageProviderIdentity.version,
    targetBindings: Object.freeze(domain.declarations.map(declaration => declaration.binding)),
    nativeDeclarations: Object.freeze(domain.declarations.map(declaration => declaration.definition)),
    relations: Object.freeze(relations), rejections: Object.freeze(rejections),
    binaryExecutionDriver: csharpJsEventLoopBinaryExecutionDriver,
  });
}

export function nodejsProviderTargetRejections(): readonly CsharpProviderTargetRejection[] {
  return Object.freeze(nodejsUnsupportedTargetMetadataRecords().flatMap(record => record.declarationIdentities.flatMap(identity =>
    nodejsPublicModuleSpecifiers(identity.providerModuleId).map(moduleSpecifier => ({
      source: nodejsProviderSourceIdentity(identity, moduleSpecifier),
      diagnostic: {
        extensionId: csharpNodejsProviderPackageProviderIdentity.id,
        extensionCode: "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED", numericCode: 9100203, category: "error" as const,
        message: `C# NodeJS provider package hard-rejected selected operation '${moduleSpecifier}' export '${identity.exportName}' signature '${identity.signatureId ?? "<none>"}': ${record.identity.displayName} has no closed target/runtime operation metadata.`,
        evidence: [{ message: `Selected provider target policy rejection '${record.identity.targetIdentityId}'.` }],
      },
    })))));
}

function bindingKey(moduleSpecifier: string, exportName: string): string {
  return `${moduleSpecifier}\u0000${exportName}`;
}
