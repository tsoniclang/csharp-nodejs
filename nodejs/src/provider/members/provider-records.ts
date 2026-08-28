import type {
  CsharpTargetMember,
} from "@tsonic/target-csharp/provider";
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
  nodeChildProcessCallTargetMembers,
  nodeChildProcessClassPropertyTargetMembers,
  nodeChildProcessModuleSpecifier,
} from "../child-process.js";
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
  nodeHttpsCallTargetMembers,
  nodeHttpsClassPropertyTargetMembers,
  nodeHttpsModuleSpecifier,
} from "../https.js";
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
  nodeUtilTextDecoderClassCallTargetMembers,
  nodeUtilTextDecoderClassPropertyTargetMembers,
} from "../text-decoder.js";
import {
  nodeUrlCallTargetMembers,
  nodeUrlClassCallTargetMembers,
  nodeUrlClassPropertyTargetMembers,
  nodeUrlModuleSpecifier,
  nodeUrlUnsupportedTargetIdentities,
} from "../url.js";
import {
  nodeEventsClassCallTargetMembers,
  nodeEventsModuleSpecifier,
} from "../events.js";
import {
  nodeStreamClassCallTargetMembers,
  nodeStreamClassPropertyTargetMembers,
  nodeStreamModuleSpecifier,
} from "../stream.js";
import {
  nodeZlibCallTargetMembers,
  nodeZlibClassCallTargetMembers,
  nodeZlibClassPropertyTargetMembers,
  nodeZlibModuleSpecifier,
} from "../zlib.js";
import {
  nodeDnsCallTargetMembers,
  nodeDnsClassPropertyTargetMembers,
  nodeDnsModuleSpecifier,
  nodeDnsPromisesCallTargetMembers,
  nodeDnsPromisesModuleSpecifier,
  nodeDnsPropertyTargetMembers,
} from "../dns.js";
import {
  nodeNetCallTargetMembers,
  nodeNetClassCallTargetMembers,
  nodeNetClassPropertyTargetMembers,
  nodeNetModuleSpecifier,
} from "../net.js";
import {
  nodeTlsCallTargetMembers,
  nodeTlsClassPropertyTargetMembers,
  nodeTlsModuleSpecifier,
} from "../tls.js";
import {
  nodeReadlineCallTargetMembers,
  nodeReadlineClassCallTargetMembers,
  nodeReadlineClassPropertyTargetMembers,
  nodeReadlineModuleSpecifier,
} from "../readline.js";
import {
  nodeWorkerThreadsClassCallTargetMembers,
  nodeWorkerThreadsClassPropertyTargetMembers,
  nodeWorkerThreadsModuleCallTargetMembers,
  nodeWorkerThreadsModulePropertyTargetMembers,
  nodeWorkerThreadsModuleSpecifier,
} from "../worker-threads.js";
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
    ...classCallRecords(nodeEventsModuleSpecifier, nodeEventsClassCallTargetMembers()),
    ...classCallRecords(nodeStreamModuleSpecifier, nodeStreamClassCallTargetMembers()),
    ...classPropertyRecords(nodeStreamModuleSpecifier, nodeStreamClassPropertyTargetMembers()),
    ...moduleCallRecords(nodeZlibModuleSpecifier, nodeZlibCallTargetMembers()),
    ...classCallRecords(nodeZlibModuleSpecifier, nodeZlibClassCallTargetMembers()),
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
