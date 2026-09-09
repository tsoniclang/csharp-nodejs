import type {
  CsharpTargetMember,
} from "@tsonic/target-csharp/provider";
import {
  nodeAssertCallTargetMembers,
  nodeAssertModuleSpecifier,
  nodeAssertUnsupportedTargetIdentities,
} from "../modules/assert.js";
import {
  nodeBufferClassCallTargetMembers,
  nodeBufferClassPropertyTargetMembers,
  nodeBufferModuleCallTargetMembers,
  nodeBufferModuleSpecifier,
  nodeBufferUnsupportedTargetIdentities,
} from "../modules/buffer/index.js";
import {
  nodeCryptoClassCallTargetMembers,
  nodeCryptoCallTargetMembers,
  nodeCryptoModuleSpecifier,
  nodeCryptoUnsupportedTargetIdentities,
} from "../modules/crypto.js";
import {
  nodeChildProcessCallTargetMembers,
  nodeChildProcessClassPropertyTargetMembers,
  nodeChildProcessModuleSpecifier,
} from "../modules/child-process.js";
import {
  nodeFsCallTargetMembers,
  nodeFsClassCallTargetMembers,
  nodeFsClassPropertyTargetMembers,
  nodeFsOptionClassPropertyTargetMembers,
  nodeFsModuleSpecifier,
  nodeFsPromisesCallTargetMembers,
  nodeFsPromisesModuleSpecifier,
  nodeFsUnsupportedTargetIdentities,
  nodeFsStreamCallTargetMembers,
  nodeFsStreamClassCallTargetMembers,
  nodeFsStreamClassPropertyTargetMembers,
} from "../modules/filesystem/index.js";
import {
  nodeOsCallTargetMembers,
  nodeOsModuleSpecifier,
  nodeOsPropertyTargetMembers,
  nodeOsUnsupportedTargetIdentities,
} from "../modules/os.js";
import {
  nodeHttpCallTargetMembers,
  nodeHttpClassCallTargetMembers,
  nodeHttpClassPropertyTargetMembers,
  nodeHttpModuleSpecifier,
} from "../modules/http/index.js";
import {
  nodeHttpsCallTargetMembers,
  nodeHttpsClassPropertyTargetMembers,
  nodeHttpsModuleSpecifier,
} from "../modules/https.js";
import {
  nodePathCallTargetMembers,
  nodePathClassPropertyTargetMembers,
  nodePathModuleSpecifier,
  nodePathPathModuleClassCallTargetMembers,
  nodePathPathModulePropertyTargetMembers,
  nodePathPropertyTargetMembers,
} from "../modules/path/index.js";
import {
  nodeProcessClassPropertyTargetMembers,
  nodeProcessCallTargetMembers,
  nodeProcessModuleSpecifier,
  nodeProcessPropertyTargetMembers,
  nodeProcessUnsupportedTargetIdentities,
} from "../modules/process.js";
import {
  nodeTimersCallTargetMembers,
  nodeTimersModuleSpecifier,
} from "../modules/timers.js";
import {
  nodeUtilCallTargetMembers,
  nodeUtilModuleSpecifier,
  nodeUtilUnsupportedTargetIdentities,
} from "../modules/util/declarations.js";
import {
  nodeUtilTextDecoderClassCallTargetMembers,
  nodeUtilTextDecoderClassPropertyTargetMembers,
} from "../modules/util/text-decoder.js";
import {
  nodeUrlCallTargetMembers,
  nodeUrlClassCallTargetMembers,
  nodeUrlClassPropertyTargetMembers,
  nodeUrlModuleSpecifier,
  nodeUrlUnsupportedTargetIdentities,
} from "../modules/url/index.js";
import {
  nodeEventsClassCallTargetMembers,
  nodeEventsModuleSpecifier,
} from "../modules/events.js";
import {
  nodeStreamClassCallTargetMembers,
  nodeStreamClassPropertyTargetMembers,
  nodeStreamModuleSpecifier,
} from "../modules/stream.js";
import {
  nodeZlibCallTargetMembers,
  nodeZlibClassPropertyTargetMembers,
  nodeZlibModuleSpecifier,
} from "../modules/zlib.js";
import {
  nodeDnsCallTargetMembers,
  nodeDnsClassPropertyTargetMembers,
  nodeDnsModuleSpecifier,
  nodeDnsPromisesCallTargetMembers,
  nodeDnsPromisesModuleSpecifier,
  nodeDnsPropertyTargetMembers,
} from "../modules/dns.js";
import {
  nodeNetCallTargetMembers,
  nodeNetClassCallTargetMembers,
  nodeNetClassPropertyTargetMembers,
  nodeNetModuleSpecifier,
} from "../modules/net.js";
import {
  nodeTlsCallTargetMembers,
  nodeTlsClassPropertyTargetMembers,
  nodeTlsModuleSpecifier,
} from "../modules/tls.js";
import {
  nodeReadlineCallTargetMembers,
  nodeReadlineClassCallTargetMembers,
  nodeReadlineClassPropertyTargetMembers,
  nodeReadlineModuleSpecifier,
} from "../modules/readline.js";
import {
  nodeWorkerThreadsClassCallTargetMembers,
  nodeWorkerThreadsClassPropertyTargetMembers,
  nodeWorkerThreadsModuleCallTargetMembers,
  nodeWorkerThreadsModulePropertyTargetMembers,
  nodeWorkerThreadsModuleSpecifier,
} from "../modules/worker-threads.js";
import {
  nodejsExportDeclarationIdentity,
  nodejsExportMemberDeclarationIdentity,
  nodejsExportMemberSignatureDeclarationIdentity,
  nodejsExportSignatureDeclarationIdentity,
} from "../identity.js";
import {
  nodejsDefaultModuleMemberDeclarationIdentities,
} from "../modules/defaults.js";
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
    ...classCallRecords(nodeEventsModuleSpecifier, nodeEventsClassCallTargetMembers()),
    ...classCallRecords(nodeStreamModuleSpecifier, nodeStreamClassCallTargetMembers()),
    ...classPropertyRecords(nodeStreamModuleSpecifier, nodeStreamClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeZlibModuleSpecifier, nodeZlibCallTargetMembers()),
    ...classPropertyRecords(nodeZlibModuleSpecifier, nodeZlibClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeDnsModuleSpecifier, nodeDnsCallTargetMembers()),
    ...modulePropertyRecords(nodeDnsModuleSpecifier, nodeDnsPropertyTargetMembers()),
    ...classPropertyRecords(nodeDnsModuleSpecifier, nodeDnsClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeDnsPromisesModuleSpecifier, nodeDnsPromisesCallTargetMembers()),
    ...moduleCallRecords(nodeNetModuleSpecifier, nodeNetCallTargetMembers()),
    ...classCallRecords(nodeNetModuleSpecifier, nodeNetClassCallTargetMembers()),
    ...classPropertyRecords(nodeNetModuleSpecifier, nodeNetClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeTlsModuleSpecifier, nodeTlsCallTargetMembers()),
    ...classPropertyRecords(nodeTlsModuleSpecifier, nodeTlsClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeReadlineModuleSpecifier, nodeReadlineCallTargetMembers()),
    ...classCallRecords(nodeReadlineModuleSpecifier, nodeReadlineClassCallTargetMembers()),
    ...classPropertyRecords(nodeReadlineModuleSpecifier, nodeReadlineClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeWorkerThreadsModuleSpecifier, nodeWorkerThreadsModuleCallTargetMembers()),
    ...modulePropertyRecords(nodeWorkerThreadsModuleSpecifier, nodeWorkerThreadsModulePropertyTargetMembers()),
    ...classCallRecords(nodeWorkerThreadsModuleSpecifier, nodeWorkerThreadsClassCallTargetMembers()),
    ...classPropertyRecords(nodeWorkerThreadsModuleSpecifier, nodeWorkerThreadsClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeBufferModuleSpecifier, nodeBufferModuleCallTargetMembers()),
    ...moduleCallRecords(nodeChildProcessModuleSpecifier, nodeChildProcessCallTargetMembers()),
    ...classPropertyRecords(nodeChildProcessModuleSpecifier, nodeChildProcessClassPropertyTargetMembers()),
    ...classCallRecords(nodeBufferModuleSpecifier, nodeBufferClassCallTargetMembers()),
    ...classPropertyRecords(nodeBufferModuleSpecifier, nodeBufferClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeAssertModuleSpecifier, nodeAssertCallTargetMembers()),
    ...moduleCallRecords(nodePathModuleSpecifier, nodePathCallTargetMembers()),
    ...modulePropertyRecords(nodePathModuleSpecifier, nodePathPropertyTargetMembers()),
    ...classCallRecords(nodePathModuleSpecifier, nodePathPathModuleClassCallTargetMembers()),
    ...classPropertyRecords(nodePathModuleSpecifier, nodePathPathModulePropertyTargetMembers()),
    ...classPropertyRecords(nodePathModuleSpecifier, nodePathClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeFsModuleSpecifier, nodeFsCallTargetMembers()),
    ...moduleCallRecords(nodeFsModuleSpecifier, nodeFsStreamCallTargetMembers()),
    ...classCallRecords(nodeFsModuleSpecifier, nodeFsClassCallTargetMembers()),
    ...classCallRecords(nodeFsModuleSpecifier, nodeFsStreamClassCallTargetMembers()),
    ...classPropertyRecords(nodeFsModuleSpecifier, nodeFsClassPropertyTargetMembers()),
    ...classPropertyRecords(nodeFsModuleSpecifier, nodeFsStreamClassPropertyTargetMembers()),
    ...classPropertyRecords(nodeFsModuleSpecifier, nodeFsOptionClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeFsPromisesModuleSpecifier, nodeFsPromisesCallTargetMembers()),
    ...moduleCallRecords(nodeHttpModuleSpecifier, nodeHttpCallTargetMembers()),
    ...classCallRecords(nodeHttpModuleSpecifier, nodeHttpClassCallTargetMembers()),
    ...classPropertyRecords(nodeHttpModuleSpecifier, nodeHttpClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeHttpsModuleSpecifier, nodeHttpsCallTargetMembers()),
    ...classPropertyRecords(nodeHttpsModuleSpecifier, nodeHttpsClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeCryptoModuleSpecifier, nodeCryptoCallTargetMembers()),
    ...classCallRecords(nodeCryptoModuleSpecifier, nodeCryptoClassCallTargetMembers()),
    ...moduleCallRecords(nodeOsModuleSpecifier, nodeOsCallTargetMembers()),
    ...modulePropertyRecords(nodeOsModuleSpecifier, nodeOsPropertyTargetMembers()),
    ...moduleCallRecords(nodeProcessModuleSpecifier, nodeProcessCallTargetMembers()),
    ...modulePropertyRecords(nodeProcessModuleSpecifier, nodeProcessPropertyTargetMembers()),
    ...classPropertyRecords(nodeProcessModuleSpecifier, nodeProcessClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeTimersModuleSpecifier, nodeTimersCallTargetMembers()),
    ...moduleCallRecords(nodeUtilModuleSpecifier, nodeUtilCallTargetMembers()),
    ...classCallRecords(nodeUtilModuleSpecifier, nodeUtilTextDecoderClassCallTargetMembers()),
    ...classPropertyRecords(nodeUtilModuleSpecifier, nodeUtilTextDecoderClassPropertyTargetMembers()),
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
