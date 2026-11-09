import type { ProviderSignatureDeclaration } from "@tsonic/tsts";
import type { CsharpProviderParameterRelation, CsharpProviderTypeParameterRelation } from "@tsonic/target-csharp/provider";
import type { NodejsNativeCallMapping } from "./locators.js";
import type { NodejsNativeResolution, NodejsSelectedNativeMember } from "./selection.js";
import { rejected, resolved } from "./selection.js";

export function nodejsParameterRelations(
  source: ProviderSignatureDeclaration,
  selected: NodejsSelectedNativeMember,
  mapping: NodejsNativeCallMapping,
): NodejsNativeResolution<readonly CsharpProviderParameterRelation[]> {
  const sourceSlots = new Set<number>();
  const nativeSlots = new Set<number>();
  const relations: CsharpProviderParameterRelation[] = [];
  if (mapping.receiver.kind === "target-parameter") nativeSlots.add(mapping.receiver.targetParameterIndex);
  for (const role of mapping.parameters) {
    const target = selected.member.parameters[role.targetParameterIndex];
    if (target === undefined || nativeSlots.has(role.targetParameterIndex)) {
      return rejected("Every Node Native physical parameter slot is covered exactly once.", selected.member.id);
    }
    nativeSlots.add(role.targetParameterIndex);
    if (role.kind === "native-default") {
      const original = selected.signature?.parameters[role.targetParameterIndex];
      if (original === undefined || original.unsupportedDefaultValue !== undefined ||
        original.rest !== true && (original.optional !== true || original.defaultValue === undefined)) {
        return rejected("A Node Native-only completion requires the actual original default or params metadata.", selected.member.id);
      }
      continue;
    }
    const parameter = source.parameters[role.sourceParameterIndex];
    if (parameter === undefined || sourceSlots.has(role.sourceParameterIndex)) {
      return rejected("Every authored Node Source parameter role is covered exactly once.", source.id);
    }
    sourceSlots.add(role.sourceParameterIndex);
    relations.push(Object.freeze({
      sourceParameterIndex: role.sourceParameterIndex, targetParameterIndex: role.targetParameterIndex,
      sourcePassingMode: parameter.passingMode ?? "by-value", targetPassingMode: target.passingMode,
      sourceAcceptsOmission: parameter.optional === true || parameter.defaultType !== undefined || parameter.rest === true,
      targetAcceptsOmission: target.optional === true || target.csharpOmittableOptionalArgument === true || target.paramsArray === true,
      sourceRest: parameter.rest === true, targetParamsArray: target.paramsArray === true,
    }));
  }
  return sourceSlots.size === source.parameters.length && nativeSlots.size === selected.member.parameters.length
    ? resolved(Object.freeze(relations))
    : rejected("Node call mappings require complete distinct Source-role and Native physical-slot coverage.", source.id);
}

export function nodejsTypeParameterRelationsAreComplete(
  relations: readonly CsharpProviderTypeParameterRelation[],
  sourceCount: number,
  targetCount: number,
): boolean {
  const sources = new Set<number>();
  const targets = new Set<number>();
  for (const relation of relations) {
    if (!Number.isInteger(relation.sourceTypeParameterIndex) || !Number.isInteger(relation.targetTypeParameterIndex) ||
      relation.sourceTypeParameterIndex < 0 || relation.sourceTypeParameterIndex >= sourceCount ||
      relation.targetTypeParameterIndex < 0 || relation.targetTypeParameterIndex >= targetCount ||
      sources.has(relation.sourceTypeParameterIndex) || targets.has(relation.targetTypeParameterIndex)) return false;
    sources.add(relation.sourceTypeParameterIndex);
    targets.add(relation.targetTypeParameterIndex);
  }
  return sources.size === sourceCount;
}
