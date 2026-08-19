import type {
  CsharpTargetMember,
} from "@tsonic/target-csharp/provider";
import type {
  NodejsClassCallTargetMember,
  NodejsClassPropertyTargetMember,
  NodejsModuleCallTargetMember,
} from "../members/types.js";
import {
  nodeBufferAllocMemberId,
  nodeBufferAllocSignatureId,
  nodeBufferAllocUnsafeMemberId,
  nodeBufferAllocUnsafeSignatureId,
  nodeBufferAllocUnsafeSlowMemberId,
  nodeBufferAllocUnsafeSlowSignatureId,
  nodeBufferAtobSignatureId,
  nodeBufferBtoaSignatureId,
  nodeBufferAtobExportName,
  nodeBufferBtoaExportName,
  nodeBufferByteLengthMemberId,
  nodeBufferByteLengthExportName,
  nodeBufferByteLengthSignatureId,
  nodeBufferCompareExportName,
  nodeBufferCompareMemberId,
  nodeBufferCompareSignatureId,
  nodeBufferCompareInstanceExportName,
  nodeBufferCompareInstanceMemberId,
  nodeBufferCompareInstanceSignatureId,
  nodeBufferConcatMemberId,
  nodeBufferConcatExportName,
  nodeBufferConcatSignatureId,
  nodeBufferCopyExportName,
  nodeBufferCopyMemberId,
  nodeBufferCopySignatureId,
  nodeBufferEqualsMemberId,
  nodeBufferEqualsExportName,
  nodeBufferEqualsSignatureId,
  nodeBufferExportName,
  nodeBufferIncludesExportName,
  nodeBufferIncludesMemberId,
  nodeBufferIncludesSignatureId,
  nodeBufferIndexOfExportName,
  nodeBufferIndexOfMemberId,
  nodeBufferIndexOfSignatureId,
  nodeBufferLastIndexOfExportName,
  nodeBufferLastIndexOfMemberId,
  nodeBufferLastIndexOfSignatureId,
  nodeBufferAllocExportName,
  nodeBufferAllocUnsafeExportName,
  nodeBufferAllocUnsafeSlowExportName,
  nodeBufferFromStringMemberId,
  nodeBufferFromBufferSignatureId,
  nodeBufferFromNumberArraySignatureId,
  nodeBufferFromStringSignatureId,
  nodeBufferFromExportName,
  nodeBufferIsBufferExportName,
  nodeBufferIsBufferMemberId,
  nodeBufferIsBufferSignatureId,
  nodeBufferIsAsciiExportName,
  nodeBufferIsAsciiSignatureId,
  nodeBufferIsEncodingMemberId,
  nodeBufferIsEncodingExportName,
  nodeBufferIsEncodingSignatureId,
  nodeBufferIsUtf8ExportName,
  nodeBufferIsUtf8SignatureId,
  nodeBufferLengthMemberId,
  nodeBufferOfExportName,
  nodeBufferOfMemberId,
  nodeBufferOfSignatureId,
  nodeBufferPoolSizeMemberId,
  nodeBufferReadUInt8ExportName,
  nodeBufferReadUInt8MemberId,
  nodeBufferReadUInt8SignatureId,
  nodeBufferSliceExportName,
  nodeBufferSliceMemberId,
  nodeBufferSliceSignatureId,
  nodeBufferSubarrayExportName,
  nodeBufferSubarrayMemberId,
  nodeBufferSubarraySignatureId,
  nodeBufferToStringExportName,
  nodeBufferToStringMemberId,
  nodeBufferToStringSignatureId,
  nodeBufferTranscodeExportName,
  nodeBufferTranscodeSignatureId,
  nodeBufferWriteExportName,
  nodeBufferWriteMemberId,
  nodeBufferWriteSignatureId,
  nodeBufferWriteUInt8ExportName,
  nodeBufferWriteUInt8MemberId,
  nodeBufferWriteUInt8SignatureId,
} from "./identities.js";
import {
  getNodeBufferCompareInstanceTargetMember,
  getNodeBufferCopyTargetMember,
  getNodeBufferEqualsTargetMember,
  getNodeBufferIncludesTargetMember,
  getNodeBufferIndexOfTargetMember,
  getNodeBufferLastIndexOfTargetMember,
  getNodeBufferLengthTargetMember,
  getNodeBufferReadUInt8TargetMember,
  getNodeBufferSliceTargetMember,
  getNodeBufferSubarrayTargetMember,
  getNodeBufferToStringTargetMember,
  getNodeBufferWriteTargetMember,
  getNodeBufferWriteUInt8TargetMember,
} from "./instance-members.js";
import {
  getNodeBufferAllocTargetMember,
  getNodeBufferAllocUnsafeSlowTargetMember,
  getNodeBufferAllocUnsafeTargetMember,
  getNodeBufferAtobTargetMember,
  getNodeBufferBtoaTargetMember,
  getNodeBufferByteLengthTargetMember,
  getNodeBufferCompareTargetMember,
  getNodeBufferConcatTargetMember,
  getNodeBufferFromBufferTargetMember,
  getNodeBufferFromNumberArrayTargetMember,
  getNodeBufferFromStringTargetMember,
  getNodeBufferIsAsciiTargetMember,
  getNodeBufferIsBufferTargetMember,
  getNodeBufferIsEncodingTargetMember,
  getNodeBufferIsUtf8TargetMember,
  getNodeBufferOfTargetMember,
  getNodeBufferPoolSizeTargetMember,
  getNodeBufferTranscodeTargetMember,
} from "./static-members.js";
import {
  nodeBufferNumericClassCallTargetMembers,
} from "./numeric-members.js";

export {
  getNodeBufferLengthTargetMember,
} from "./instance-members.js";

export function nodeBufferModuleCallTargetMembers(): readonly NodejsModuleCallTargetMember[] {
  return [
    { exportName: nodeBufferAtobExportName, signatureId: nodeBufferAtobSignatureId, member: getNodeBufferAtobTargetMember() },
    { exportName: nodeBufferBtoaExportName, signatureId: nodeBufferBtoaSignatureId, member: getNodeBufferBtoaTargetMember() },
    { exportName: nodeBufferIsAsciiExportName, signatureId: nodeBufferIsAsciiSignatureId, member: getNodeBufferIsAsciiTargetMember() },
    { exportName: nodeBufferIsUtf8ExportName, signatureId: nodeBufferIsUtf8SignatureId, member: getNodeBufferIsUtf8TargetMember() },
    { exportName: nodeBufferTranscodeExportName, signatureId: nodeBufferTranscodeSignatureId, member: getNodeBufferTranscodeTargetMember() },
  ];
}

export function nodeBufferClassCallTargetMembers(): readonly NodejsClassCallTargetMember[] {
  return [
    nodeBufferStaticClassCallTargetMember(nodeBufferFromExportName, nodeBufferFromStringMemberId, nodeBufferFromStringSignatureId, getNodeBufferFromStringTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferFromExportName, nodeBufferFromStringMemberId, nodeBufferFromNumberArraySignatureId, getNodeBufferFromNumberArrayTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferFromExportName, nodeBufferFromStringMemberId, nodeBufferFromBufferSignatureId, getNodeBufferFromBufferTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferAllocExportName, nodeBufferAllocMemberId, nodeBufferAllocSignatureId, getNodeBufferAllocTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferAllocUnsafeExportName, nodeBufferAllocUnsafeMemberId, nodeBufferAllocUnsafeSignatureId, getNodeBufferAllocUnsafeTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferAllocUnsafeSlowExportName, nodeBufferAllocUnsafeSlowMemberId, nodeBufferAllocUnsafeSlowSignatureId, getNodeBufferAllocUnsafeSlowTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferByteLengthExportName, nodeBufferByteLengthMemberId, nodeBufferByteLengthSignatureId, getNodeBufferByteLengthTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferCompareExportName, nodeBufferCompareMemberId, nodeBufferCompareSignatureId, getNodeBufferCompareTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferConcatExportName, nodeBufferConcatMemberId, nodeBufferConcatSignatureId, getNodeBufferConcatTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferIsBufferExportName, nodeBufferIsBufferMemberId, nodeBufferIsBufferSignatureId, getNodeBufferIsBufferTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferIsEncodingExportName, nodeBufferIsEncodingMemberId, nodeBufferIsEncodingSignatureId, getNodeBufferIsEncodingTargetMember()),
    nodeBufferStaticClassCallTargetMember(nodeBufferOfExportName, nodeBufferOfMemberId, nodeBufferOfSignatureId, getNodeBufferOfTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferEqualsExportName, nodeBufferEqualsMemberId, nodeBufferEqualsSignatureId, getNodeBufferEqualsTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferCompareInstanceExportName, nodeBufferCompareInstanceMemberId, nodeBufferCompareInstanceSignatureId, getNodeBufferCompareInstanceTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferCopyExportName, nodeBufferCopyMemberId, nodeBufferCopySignatureId, getNodeBufferCopyTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferIncludesExportName, nodeBufferIncludesMemberId, nodeBufferIncludesSignatureId, getNodeBufferIncludesTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferIndexOfExportName, nodeBufferIndexOfMemberId, nodeBufferIndexOfSignatureId, getNodeBufferIndexOfTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferLastIndexOfExportName, nodeBufferLastIndexOfMemberId, nodeBufferLastIndexOfSignatureId, getNodeBufferLastIndexOfTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferSliceExportName, nodeBufferSliceMemberId, nodeBufferSliceSignatureId, getNodeBufferSliceTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferSubarrayExportName, nodeBufferSubarrayMemberId, nodeBufferSubarraySignatureId, getNodeBufferSubarrayTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferToStringExportName, nodeBufferToStringMemberId, nodeBufferToStringSignatureId, getNodeBufferToStringTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferWriteExportName, nodeBufferWriteMemberId, nodeBufferWriteSignatureId, getNodeBufferWriteTargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferReadUInt8ExportName, nodeBufferReadUInt8MemberId, nodeBufferReadUInt8SignatureId, getNodeBufferReadUInt8TargetMember()),
    nodeBufferClassCallTargetMember(nodeBufferWriteUInt8ExportName, nodeBufferWriteUInt8MemberId, nodeBufferWriteUInt8SignatureId, getNodeBufferWriteUInt8TargetMember()),
    ...nodeBufferNumericClassCallTargetMembers(),
  ];
}

function nodeBufferStaticClassCallTargetMember(
  memberName: string,
  memberId: string,
  signatureId: string,
  member: CsharpTargetMember,
): NodejsClassCallTargetMember {
  return {
    ...nodeBufferClassCallTargetMember(
      memberName,
      memberId,
      signatureId,
      member,
    ),
    static: true,
  };
}

export function nodeBufferClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMember[] {
  return [
    {
      exportName: nodeBufferExportName,
      memberName: "length",
      memberId: nodeBufferLengthMemberId,
      member: getNodeBufferLengthTargetMember(),
    },
    {
      exportName: nodeBufferExportName,
      memberName: "poolSize",
      memberId: nodeBufferPoolSizeMemberId,
      member: getNodeBufferPoolSizeTargetMember(),
    },
  ];
}

function nodeBufferClassCallTargetMember(
  memberName: string,
  memberId: string,
  signatureId: string,
  member: CsharpTargetMember,
): NodejsClassCallTargetMember {
  return {
    exportName: nodeBufferExportName,
    memberName,
    memberId,
    signatureId,
    member,
  };
}
