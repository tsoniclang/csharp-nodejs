import type { NodejsClassCallTargetMember, NodejsClassPropertyTargetMember, NodejsModuleCallTargetMember } from "../../model/members.js";
import type { NodejsNativeCallMapping, NodejsNativeTypeLocator } from "../../native/locators.js";
import type { NodejsNativeParameterMapping } from "../../native/authoring.js";
import { nativeCall, nativeDefault, nativeOwner, nativeParameter, nativeSlot } from "../../native/authoring.js";
import { nativeArray, nativeNamedType, nativeNullable, nativeNullableReference } from "../../native/locators.js";
import * as identity from "./identities.js";
import { nodeBufferNumericClassCallTargetMembers } from "./numeric-members.js";

const owner = nativeOwner("Tsonic.CSharp.Node.Buffer");
const moduleOwner = nativeOwner("Tsonic.CSharp.Node.buffer");
const buffer = nativeNamedType(owner.metadataName);
const nativeString: NodejsNativeTypeLocator = { kind: "string" };
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const nativeByte: NodejsNativeTypeLocator = { kind: "source-primitive", name: "uint8" };
const nativeBool: NodejsNativeTypeLocator = { kind: "source-primitive", name: "bool" };
const nativeObject: NodejsNativeTypeLocator = { kind: "object" };
const nullableInt = nativeNullable(nativeInt);

export function nodeBufferModuleCallTargetMembers(): readonly NodejsModuleCallTargetMember[] {
  return [
    moduleCall("atob", identity.nodeBufferAtobSignatureId, [nativeParameter(0, nativeString)], nativeString),
    moduleCall("btoa", identity.nodeBufferBtoaSignatureId, [nativeParameter(0, nativeString)], nativeString),
    moduleCall("isAscii", identity.nodeBufferIsAsciiSignatureId, [nativeParameter(0, buffer)], nativeBool),
    moduleCall("isUtf8", identity.nodeBufferIsUtf8SignatureId, [nativeParameter(0, buffer)], nativeBool),
    moduleCall("transcode", identity.nodeBufferTranscodeSignatureId,
      [nativeParameter(0, buffer), nativeParameter(1, nativeString), nativeParameter(2, nativeString)], buffer),
  ];
}

export function nodeBufferClassCallTargetMembers(includeJsSurfaceMembers = true): readonly NodejsClassCallTargetMember[] {
  return [
    classCall("from", identity.nodeBufferFromStringMemberId, identity.nodeBufferFromStringSignatureId,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeString)], buffer, true),
    classCall("from", identity.nodeBufferFromStringMemberId, identity.nodeBufferFromNumberArraySignatureId,
      [nativeParameter(0, nativeArray(nativeInt))], buffer, true),
    classCall("from", identity.nodeBufferFromStringMemberId, identity.nodeBufferFromBufferSignatureId,
      [nativeParameter(0, buffer)], buffer, true),
    ...includeJsSurfaceMembers ? [classCall("from", identity.nodeBufferFromStringMemberId, identity.nodeBufferFromUint8ArraySignatureId,
      [nativeParameter(0, nativeNamedType("Tsonic.CSharp.Js.Uint8Array"))], buffer, true)] : [],
    classCall("alloc", identity.nodeBufferAllocMemberId, identity.nodeBufferAllocSignatureId,
      [nativeParameter(0, nativeInt), nativeDefault(nativeNullableReference(nativeObject)), nativeDefault(nativeNullableReference(nativeString))], buffer, true),
    classCall("allocUnsafe", identity.nodeBufferAllocUnsafeMemberId, identity.nodeBufferAllocUnsafeSignatureId, [nativeParameter(0, nativeInt)], buffer, true),
    classCall("allocUnsafeSlow", identity.nodeBufferAllocUnsafeSlowMemberId, identity.nodeBufferAllocUnsafeSlowSignatureId, [nativeParameter(0, nativeInt)], buffer, true),
    classCall("byteLength", identity.nodeBufferByteLengthMemberId, identity.nodeBufferByteLengthSignatureId,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeString)], nativeInt, true),
    classCall("compare", identity.nodeBufferCompareMemberId, identity.nodeBufferCompareSignatureId,
      [nativeParameter(0, buffer), nativeParameter(1, buffer)], nativeInt, true),
    classCall("concat", identity.nodeBufferConcatMemberId, identity.nodeBufferConcatSignatureId,
      [nativeParameter(0, nativeNamedType("System.Collections.Generic.IReadOnlyList`1", buffer)), nativeParameter(1, nullableInt)], buffer, true),
    classCall("isBuffer", identity.nodeBufferIsBufferMemberId, identity.nodeBufferIsBufferSignatureId,
      [nativeParameter(0, nativeNullableReference(nativeObject))], nativeBool, true),
    classCall("isEncoding", identity.nodeBufferIsEncodingMemberId, identity.nodeBufferIsEncodingSignatureId,
      [nativeParameter(0, nativeString)], nativeBool, true),
    classCall("of", identity.nodeBufferOfMemberId, identity.nodeBufferOfSignatureId, [nativeParameter(0, nativeArray(nativeInt))], buffer, true),
    classCall("equals", identity.nodeBufferEqualsMemberId, identity.nodeBufferEqualsSignatureId, [nativeParameter(0, buffer)], nativeBool, false),
    classCall("compare", identity.nodeBufferCompareInstanceMemberId, identity.nodeBufferCompareInstanceSignatureId,
      [nativeParameter(0, buffer), nativeDefault(nullableInt), nativeDefault(nullableInt), nativeDefault(nullableInt), nativeDefault(nullableInt)], nativeInt, false),
    classCall("copy", identity.nodeBufferCopyMemberId, identity.nodeBufferCopySignatureId,
      [nativeParameter(0, buffer), nativeParameter(1, nativeInt), nativeParameter(2, nullableInt), nativeParameter(3, nullableInt)], nativeInt, false),
    classCall("includes", identity.nodeBufferIncludesMemberId, identity.nodeBufferIncludesSignatureId,
      [nativeParameter(0, nativeObject), nativeParameter(1, nativeInt), nativeParameter(2, nativeString)], nativeBool, false),
    classCall("indexOf", identity.nodeBufferIndexOfMemberId, identity.nodeBufferIndexOfSignatureId,
      [nativeParameter(0, nativeObject), nativeParameter(1, nativeInt), nativeParameter(2, nativeString)], nativeInt, false),
    classCall("lastIndexOf", identity.nodeBufferLastIndexOfMemberId, identity.nodeBufferLastIndexOfSignatureId,
      [nativeParameter(0, nativeObject), nativeParameter(1, nullableInt), nativeParameter(2, nativeString)], nativeInt, false),
    classCall("slice", identity.nodeBufferSliceMemberId, identity.nodeBufferSliceSignatureId,
      [nativeParameter(0, nullableInt), nativeParameter(1, nullableInt)], buffer, false),
    classCall("subarray", identity.nodeBufferSubarrayMemberId, identity.nodeBufferSubarraySignatureId,
      [nativeParameter(0, nullableInt), nativeParameter(1, nullableInt)], buffer, false),
    classCall("toString", identity.nodeBufferToStringMemberId, identity.nodeBufferToStringSignatureId,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeInt), nativeParameter(2, nullableInt)], nativeString, false),
    classCall("write", identity.nodeBufferWriteMemberId, identity.nodeBufferWriteSignatureId,
      [nativeParameter(0, nativeString), nativeParameter(1, nativeInt), nativeParameter(2, nullableInt), nativeParameter(3, nativeString)], nativeInt, false),
    classCall("readUInt8", identity.nodeBufferReadUInt8MemberId, identity.nodeBufferReadUInt8SignatureId, [nativeParameter(0, nativeInt)], nativeByte, false),
    classCall("writeUInt8", identity.nodeBufferWriteUInt8MemberId, identity.nodeBufferWriteUInt8SignatureId,
      [nativeParameter(0, nativeByte), nativeParameter(1, nativeInt)], nativeInt, false),
    ...nodeBufferNumericClassCallTargetMembers(),
  ];
}

export function nodeBufferClassPropertyTargetMembers(): readonly NodejsClassPropertyTargetMember[] {
  return [
    { exportName: "Buffer", memberName: "length", memberId: identity.nodeBufferLengthMemberId, native: nativeSlot(owner, "length", nativeInt) },
    { exportName: "Buffer", memberName: "poolSize", memberId: identity.nodeBufferPoolSizeMemberId, static: true, native: nativeSlot(owner, "poolSize", nativeInt, true) },
  ];
}

function classCall(
  memberName: string, memberId: string, signatureId: string, parameters: readonly NodejsNativeParameterMapping[],
  returnType: NodejsNativeTypeLocator, isStatic: boolean,
): NodejsClassCallTargetMember {
  return { exportName: "Buffer", memberName, memberId, signatureId, ...isStatic ? { static: true } : {},
    native: method(memberName, parameters, returnType, isStatic) };
}

function method(
  targetName: string, parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator, isStatic: boolean,
): NodejsNativeCallMapping {
  return nativeCall({ owner, kind: "method", targetName, static: isStatic, parameters, returnType,
    receiver: { kind: isStatic ? "none" : "instance" } });
}

function moduleCall(
  exportName: string, signatureId: string, parameters: readonly NodejsNativeParameterMapping[], returnType: NodejsNativeTypeLocator,
): NodejsModuleCallTargetMember {
  return { exportName, signatureId, native: nativeCall({ owner: moduleOwner, kind: "method", targetName: exportName,
    static: true, parameters, returnType, receiver: { kind: "none" } }) };
}

