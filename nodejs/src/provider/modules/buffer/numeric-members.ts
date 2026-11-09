import type { ProviderMemberDeclaration, ProviderTypeExpression } from "@tsonic/tsts";
import type { NodejsClassCallTargetMember } from "../../model/members.js";
import type { NodejsNativeTypeLocator } from "../../native/locators.js";
import { nativeCall, nativeOwner, nativeParameter } from "../../native/authoring.js";

type NumericKind = "int8" | "uint16" | "int16" | "uint32" | "int32" | "float32" | "float64";
const widths: readonly (readonly [suffix: string, kind: NumericKind, sourceIdentityName: string])[] = [
  ["Int8", "int8", "System.SByte"], ["UInt16LE", "uint16", "System.UInt16"], ["UInt16BE", "uint16", "System.UInt16"],
  ["Int16LE", "int16", "System.Int16"], ["Int16BE", "int16", "System.Int16"],
  ["UInt32LE", "uint32", "System.UInt32"], ["UInt32BE", "uint32", "System.UInt32"],
  ["Int32LE", "int32", "System.Int32"], ["Int32BE", "int32", "System.Int32"],
  ["FloatLE", "float32", "System.Single"], ["FloatBE", "float32", "System.Single"],
  ["DoubleLE", "float64", "System.Double"], ["DoubleBE", "float64", "System.Double"],
];
const owner = nativeOwner("Tsonic.CSharp.Node.Buffer");
const nativeInt: NodejsNativeTypeLocator = { kind: "source-primitive", name: "int32" };
const sourceInt: ProviderTypeExpression = { kind: "source-primitive", name: "int32" };

function numericMembers() {
  return [false, true].flatMap(write => widths.map(([suffix, kind, identityName]) => {
    const name = `${write ? "write" : "read"}${suffix}`;
    const memberId = `node:buffer.Buffer.${name}`;
    const sourceValue: ProviderTypeExpression = { kind: "source-primitive", name: kind };
    const physicalValue: NodejsNativeTypeLocator = { kind: "source-primitive", name: kind };
    return { name, memberId, signatureId: `${memberId}(${write ? identityName + "," : ""}System.Int32)`,
      sourceParameters: [...write ? [{ name: "value", type: sourceValue }] : [], { name: "offset", type: sourceInt, optional: true }],
      sourceReturn: write ? sourceInt : sourceValue,
      native: nativeCall({ owner, kind: "method", targetName: name, static: false,
        parameters: write ? [nativeParameter(0, physicalValue), nativeParameter(1, nativeInt)] : [nativeParameter(0, nativeInt)],
        returnType: write ? nativeInt : physicalValue, receiver: { kind: "instance" } }),
    };
  }));
}

export function nodeBufferNumericInstanceMemberDeclarations(): readonly ProviderMemberDeclaration[] {
  return numericMembers().map(entry => ({ id: entry.memberId, name: entry.name, kind: "method",
    signatures: [{ id: entry.signatureId, parameters: entry.sourceParameters, returnType: entry.sourceReturn }] }));
}

export function nodeBufferNumericClassCallTargetMembers(): readonly NodejsClassCallTargetMember[] {
  return numericMembers().map(entry => ({ exportName: "Buffer", memberName: entry.name, memberId: entry.memberId,
    signatureId: entry.signatureId, native: entry.native }));
}

