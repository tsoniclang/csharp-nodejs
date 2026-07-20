import type {
  CheckedOperationMappingResult,
  TargetTypeRef,
} from "@tsonic/tsts";
import type {
  CsharpTargetOperationFact,
} from "@tsonic/target-csharp";
import {
  csharpTargetOperationFromMember,
  targetOperation,
  targetOperationFromMember,
} from "@tsonic/target-csharp";
import type {
  NodejsProviderDeclarationIdentity,
} from "../identity.js";
import {
  getNodejsIndexerTargetMemberFromReceiverTypeMetadata,
  getNodejsPropertyTargetMemberFromMetadata,
} from "./metadata-index.js";

export function getCsharpNodejsPropertyOperation(
  declaration: NodejsProviderDeclarationIdentity,
): { readonly mapping: CheckedOperationMappingResult; readonly csharpOperation: CsharpTargetOperationFact } | undefined {
  const member = getNodejsPropertyTargetMemberFromMetadata(declaration);
  return operationFromNodejsTargetMember(member);
}

export function getCsharpNodejsElementOperationForReceiverType(
  receiverType: TargetTypeRef | undefined,
): { readonly mapping: CheckedOperationMappingResult; readonly csharpOperation: CsharpTargetOperationFact } | undefined {
  return operationFromNodejsTargetMember(getNodejsIndexerTargetMemberFromReceiverTypeMetadata(receiverType));
}

function operationFromNodejsTargetMember(
  member: ReturnType<typeof getNodejsPropertyTargetMemberFromMetadata>,
): { readonly mapping: CheckedOperationMappingResult; readonly csharpOperation: CsharpTargetOperationFact } | undefined {
  return member === undefined
    ? undefined
    : {
        mapping: {
          operation: member.returnType === undefined
            ? targetOperationFromMember(member)
            : targetOperation(member.id, member.kind === "field" || member.kind === "event" ? "property" : member.kind, member.targetName),
          ...(member.returnType === undefined ? {} : { resultType: member.returnType }),
        },
        csharpOperation: csharpTargetOperationFromMember(member),
      };
}
