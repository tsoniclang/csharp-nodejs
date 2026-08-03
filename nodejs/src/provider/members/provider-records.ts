import type {
  CsharpTargetMember,
} from "@tsonic/target-csharp";
import {
  nodeAssertCallTargetMembers,
  nodeAssertModuleSpecifier,
  nodeAssertUnsupportedTargetIdentities,
} from "../assert.js";
import {
  nodeBufferClassCallTargetMembers,
  nodeBufferClassPropertyTargetMembers,
  nodeBufferModuleCallTargetMembers,
  nodeBufferModuleSpecifier,
  nodeBufferUnsupportedTargetIdentities,
} from "../buffer.js";
import {
  nodeCryptoClassCallTargetMembers,
  nodeCryptoCallTargetMembers,
  nodeCryptoModuleSpecifier,
  nodeCryptoUnsupportedTargetIdentities,
} from "../crypto.js";
import {
  nodeFsCallTargetMembers,
  nodeFsClassCallTargetMembers,
  nodeFsClassPropertyTargetMembers,
  nodeFsModuleSpecifier,
  nodeFsPromisesCallTargetMembers,
  nodeFsPromisesModuleSpecifier,
  nodeFsUnsupportedTargetIdentities,
} from "../filesystem/index.js";
import {
  nodeOsCallTargetMembers,
  nodeOsModuleSpecifier,
  nodeOsPropertyTargetMembers,
  nodeOsUnsupportedTargetIdentities,
} from "../os.js";
import {
  nodeHttpCallTargetMembers,
  nodeHttpClassCallTargetMembers,
  nodeHttpClassPropertyTargetMembers,
  nodeHttpModuleSpecifier,
} from "../http.js";
import {
  nodePathCallTargetMembers,
  nodePathClassPropertyTargetMembers,
  nodePathModuleSpecifier,
  nodePathPathModuleClassCallTargetMembers,
  nodePathPathModulePropertyTargetMembers,
  nodePathPropertyTargetMembers,
} from "../path.js";
import {
  nodeProcessClassPropertyTargetMembers,
  nodeProcessCallTargetMembers,
  nodeProcessModuleSpecifier,
  nodeProcessPropertyTargetMembers,
  nodeProcessUnsupportedTargetIdentities,
} from "../process.js";
import {
  nodeTimersCallTargetMembers,
  nodeTimersModuleSpecifier,
} from "../timers.js";
import {
  nodeUtilCallTargetMembers,
  nodeUtilModuleSpecifier,
  nodeUtilUnsupportedTargetIdentities,
} from "../util.js";
import {
  nodeUrlCallTargetMembers,
  nodeUrlClassCallTargetMembers,
  nodeUrlClassPropertyTargetMembers,
  nodeUrlModuleSpecifier,
  nodeUrlUnsupportedTargetIdentities,
} from "../url.js";
import {
  nodejsExportDeclarationIdentity,
  nodejsExportMemberDeclarationIdentity,
  nodejsExportMemberSignatureDeclarationIdentity,
  nodejsExportSignatureDeclarationIdentity,
} from "../identity.js";
import {
  nodejsDefaultModuleMemberDeclarationIdentities,
} from "../module-defaults.js";
import type {
  NodejsProviderDeclarationIdentity,
} from "../identity.js";
import type {
  NodejsClassCallTargetMember,
  NodejsClassPropertyTargetMember,
  NodejsModuleCallTargetMember,
  NodejsModulePropertyTargetMember,
  NodejsUnsupportedTargetIdentity,
} from "./types.js";

export interface NodejsTargetMemberMetadataRecord {
  readonly declarationIdentities: readonly NodejsProviderDeclarationIdentity[];
  readonly member: CsharpTargetMember;
}

export interface NodejsUnsupportedTargetMetadataRecord {
  readonly declarationIdentities: readonly NodejsProviderDeclarationIdentity[];
  readonly identity: NodejsUnsupportedTargetIdentity;
}

export function nodejsTargetMemberMetadataRecords(): readonly NodejsTargetMemberMetadataRecord[] {
  return [
    ...moduleCallRecords(nodeBufferModuleSpecifier, nodeBufferModuleCallTargetMembers()),
    ...classCallRecords(nodeBufferModuleSpecifier, nodeBufferClassCallTargetMembers()),
    ...classPropertyRecords(nodeBufferModuleSpecifier, nodeBufferClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeAssertModuleSpecifier, nodeAssertCallTargetMembers()),
    ...moduleCallRecords(nodePathModuleSpecifier, nodePathCallTargetMembers()),
    ...modulePropertyRecords(nodePathModuleSpecifier, nodePathPropertyTargetMembers()),
    ...classCallRecords(nodePathModuleSpecifier, nodePathPathModuleClassCallTargetMembers()),
    ...classPropertyRecords(nodePathModuleSpecifier, nodePathPathModulePropertyTargetMembers()),
    ...classPropertyRecords(nodePathModuleSpecifier, nodePathClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeFsModuleSpecifier, nodeFsCallTargetMembers()),
    ...classCallRecords(nodeFsModuleSpecifier, nodeFsClassCallTargetMembers()),
    ...classPropertyRecords(nodeFsModuleSpecifier, nodeFsClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeFsPromisesModuleSpecifier, nodeFsPromisesCallTargetMembers()),
    ...moduleCallRecords(nodeHttpModuleSpecifier, nodeHttpCallTargetMembers()),
    ...classCallRecords(nodeHttpModuleSpecifier, nodeHttpClassCallTargetMembers()),
    ...classPropertyRecords(nodeHttpModuleSpecifier, nodeHttpClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeCryptoModuleSpecifier, nodeCryptoCallTargetMembers()),
    ...classCallRecords(nodeCryptoModuleSpecifier, nodeCryptoClassCallTargetMembers()),
    ...moduleCallRecords(nodeOsModuleSpecifier, nodeOsCallTargetMembers()),
    ...modulePropertyRecords(nodeOsModuleSpecifier, nodeOsPropertyTargetMembers()),
    ...moduleCallRecords(nodeProcessModuleSpecifier, nodeProcessCallTargetMembers()),
    ...modulePropertyRecords(nodeProcessModuleSpecifier, nodeProcessPropertyTargetMembers()),
    ...classPropertyRecords(nodeProcessModuleSpecifier, nodeProcessClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeTimersModuleSpecifier, nodeTimersCallTargetMembers()),
    ...moduleCallRecords(nodeUtilModuleSpecifier, nodeUtilCallTargetMembers()),
    ...moduleCallRecords(nodeUrlModuleSpecifier, nodeUrlCallTargetMembers()),
    ...classCallRecords(nodeUrlModuleSpecifier, nodeUrlClassCallTargetMembers()),
    ...classPropertyRecords(nodeUrlModuleSpecifier, nodeUrlClassPropertyTargetMembers()),
  ];
}

export function nodejsUnsupportedTargetMetadataRecords(): readonly NodejsUnsupportedTargetMetadataRecord[] {
  return [
    ...unsupportedRecords(nodeAssertModuleSpecifier, nodeAssertUnsupportedTargetIdentities()),
    ...unsupportedRecords(nodeBufferModuleSpecifier, nodeBufferUnsupportedTargetIdentities()),
    ...unsupportedRecords(nodeCryptoModuleSpecifier, nodeCryptoUnsupportedTargetIdentities()),
    ...unsupportedRecords(nodeFsModuleSpecifier, nodeFsUnsupportedTargetIdentities()),
    ...unsupportedRecords(nodeOsModuleSpecifier, nodeOsUnsupportedTargetIdentities()),
    ...unsupportedRecords(nodeUtilModuleSpecifier, nodeUtilUnsupportedTargetIdentities()),
    ...unsupportedRecords(nodeProcessModuleSpecifier, nodeProcessUnsupportedTargetIdentities()),
    ...unsupportedRecords(nodeUrlModuleSpecifier, nodeUrlUnsupportedTargetIdentities()),
  ];
}

function moduleCallRecords(
  moduleSpecifier: string,
  entries: readonly NodejsModuleCallTargetMember[],
): readonly NodejsTargetMemberMetadataRecord[] {
  return entries.map((entry) => ({
    declarationIdentities: [
      nodejsExportSignatureDeclarationIdentity(moduleSpecifier, entry.exportName, entry.signatureId),
      ...nodejsDefaultModuleMemberDeclarationIdentities(
        moduleSpecifier,
        entry.exportName,
        entry.signatureId,
      ).filter((identity) => identity.signatureId !== undefined),
    ],
    member: entry.member,
  }));
}

function modulePropertyRecords(
  moduleSpecifier: string,
  entries: readonly NodejsModulePropertyTargetMember[],
): readonly NodejsTargetMemberMetadataRecord[] {
  return entries.map((entry) => ({
    declarationIdentities: [
      nodejsExportDeclarationIdentity(moduleSpecifier, entry.exportName),
      ...nodejsDefaultModuleMemberDeclarationIdentities(moduleSpecifier, entry.exportName, undefined),
    ],
    member: entry.member,
  }));
}

function classCallRecords(
  moduleSpecifier: string,
  entries: readonly NodejsClassCallTargetMember[],
): readonly NodejsTargetMemberMetadataRecord[] {
  return entries.map((entry) => ({
    declarationIdentities: [
      nodejsExportMemberSignatureDeclarationIdentity(
        moduleSpecifier,
        entry.exportName,
        entry.memberName,
        entry.memberId,
        entry.signatureId,
        entry.static === true,
      ),
    ],
    member: entry.member,
  }));
}

function classPropertyRecords(
  moduleSpecifier: string,
  entries: readonly NodejsClassPropertyTargetMember[],
): readonly NodejsTargetMemberMetadataRecord[] {
  return entries.map((entry) => ({
    declarationIdentities: [
      ...(entry.signatureId === undefined
        ? [nodejsExportMemberDeclarationIdentity(
            moduleSpecifier,
            entry.exportName,
            entry.memberName,
            entry.memberId,
            entry.member.static === true,
          )]
        : [nodejsExportMemberSignatureDeclarationIdentity(
            moduleSpecifier,
            entry.exportName,
            entry.memberName,
            entry.memberId,
            entry.signatureId,
            entry.member.static === true,
          )]),
    ],
    member: entry.member,
  }));
}

function unsupportedRecords(
  moduleSpecifier: string,
  entries: readonly NodejsUnsupportedTargetIdentity[],
): readonly NodejsUnsupportedTargetMetadataRecord[] {
  return entries.map((identity) => ({
    declarationIdentities: unsupportedDeclarationIdentities(moduleSpecifier, identity),
    identity,
  }));
}

function unsupportedDeclarationIdentities(
  moduleSpecifier: string,
  identity: NodejsUnsupportedTargetIdentity,
): readonly NodejsProviderDeclarationIdentity[] {
  if (identity.memberName === undefined) {
    return [
      identity.signatureId === undefined
        ? nodejsExportDeclarationIdentity(moduleSpecifier, identity.exportName)
        : nodejsExportSignatureDeclarationIdentity(moduleSpecifier, identity.exportName, identity.signatureId),
      ...nodejsDefaultModuleMemberDeclarationIdentities(
        moduleSpecifier,
        identity.exportName,
        identity.signatureId,
      ).filter((declaration) =>
        identity.signatureId === undefined || declaration.signatureId !== undefined
      ),
    ];
  }
  if (identity.memberId === undefined) {
    throw new Error(
      `Unsupported NodeJS provider member '${moduleSpecifier}.${identity.exportName}.${identity.memberName}' requires an exact member id.`,
    );
  }
  return [
    identity.signatureId === undefined
      ? nodejsExportMemberDeclarationIdentity(
          moduleSpecifier,
          identity.exportName,
          identity.memberName,
          identity.memberId,
        )
      : nodejsExportMemberSignatureDeclarationIdentity(
          moduleSpecifier,
          identity.exportName,
          identity.memberName,
          identity.memberId,
          identity.signatureId,
        ),
  ];
}
