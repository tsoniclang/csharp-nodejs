import { test } from "node:test";
import assert from "node:assert/strict";
import {
  createCompilerSessionFromFiles,
  formatDiagnostics,
  providerVirtualDeclarationFactKey,
  selectedTargetSignatureFactKey,
} from "../../tsonic/packages/tsts/dist/src/index.js";
import { createTsonicCoreSourceExtension } from "../../tsonic/packages/source-core/dist/index.js";
import { csharpTargetOperationFactKey } from "../../tsonic-csharp/dist/index.js";
import {
  createCsharpTargetPack,
  createCsharpJsSurfaceExtension,
  createCsharpSourceSemanticsExtension,
  createCsharpTargetSemanticsExtension,
} from "../../tsonic-csharp/dist/index.js";
import {
  normalizeTargetSourceProfileSegment,
  tsonicSourceProfileVirtualDirectory,
} from "../../tsonic/packages/target-api/dist/index.js";
import {
  createCsharpNodejsProviderPackageBindingProvider,
  createCsharpNodejsProviderPackageExtension,
  createCsharpNodejsProviderOperationsContribution,
  createCsharpNodejsTargetContributions,
} from "../dist/provider/index.js";
import {
  nodeFsCallTargetMembers,
  nodeFsModuleSpecifier,
  nodeFsPromisesCallTargetMembers,
  nodeFsPromisesModuleSpecifier,
  nodeFsUnsupportedTargetIdentities,
} from "../dist/provider/filesystem/index.js";
import {
  nodeCryptoCallTargetMembers,
  nodeCryptoClassCallTargetMembers,
  nodeCryptoModuleSpecifier,
  nodeCryptoUnsupportedTargetIdentities,
} from "../dist/provider/crypto.js";
import {
  nodeOsCallTargetMembers,
  nodeOsModuleSpecifier,
  nodeOsPropertyTargetMembers,
  nodeOsUnsupportedTargetIdentities,
} from "../dist/provider/os.js";

function createCsharpNodejsOperationsTestContribution() {
  return createCsharpNodejsProviderOperationsContribution("tsonic.csharp.provider-package.nodejs.test");
}

export { test, assert, createCompilerSessionFromFiles, formatDiagnostics, providerVirtualDeclarationFactKey, selectedTargetSignatureFactKey, createTsonicCoreSourceExtension, csharpTargetOperationFactKey, createCsharpTargetPack, createCsharpJsSurfaceExtension, createCsharpSourceSemanticsExtension, createCsharpTargetSemanticsExtension, createCsharpNodejsProviderPackageBindingProvider, createCsharpNodejsProviderPackageExtension, createCsharpNodejsTargetContributions, createCsharpNodejsOperationsTestContribution, nodeFsCallTargetMembers, nodeFsModuleSpecifier, nodeFsPromisesCallTargetMembers, nodeFsPromisesModuleSpecifier, nodeFsUnsupportedTargetIdentities, nodeCryptoCallTargetMembers, nodeCryptoClassCallTargetMembers, nodeCryptoModuleSpecifier, nodeCryptoUnsupportedTargetIdentities, nodeOsCallTargetMembers, nodeOsModuleSpecifier, nodeOsPropertyTargetMembers, nodeOsUnsupportedTargetIdentities };













export function assertModuleExport(bindingProvider, moduleSpecifier, exportName, signatureId, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  assert.ok(declaration?.signatures?.some((signature) => signature.id === signatureId));
  assertCallMapping(
    nodejsVirtualDeclaration(moduleSpecifier, exportName, signatureId),
    targetIdentityId,
  );
}

export function assertClassMember(bindingProvider, moduleSpecifier, exportName, memberName, signatureId, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  const member = declaration?.members?.find((entry) => entry.name === memberName);
  assert.ok(member?.signatures?.some((signature) => signature.id === signatureId));
  assertCallMapping(
    nodejsVirtualMemberDeclaration(
      moduleSpecifier,
      exportName,
      memberName,
      member.id,
      signatureId,
    ),
    targetIdentityId,
  );
}

export function assertClassProperty(bindingProvider, moduleSpecifier, exportName, memberName, memberId, targetIdentityId = memberId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  const member = declaration?.members?.find((entry) => entry.name === memberName);
  assert.equal(member?.id, memberId);
  assertPropertyMapping(
    nodejsVirtualMemberDeclaration(moduleSpecifier, exportName, memberName, memberId),
    targetIdentityId,
  );
}

export function assertModuleValue(bindingProvider, moduleSpecifier, exportName, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  assert.equal(declaration?.kind, "value");
  assertPropertyMapping(
    nodejsVirtualDeclaration(moduleSpecifier, exportName),
    targetIdentityId,
  );
}

export function assertProviderUnionType(type, expectedKinds) {
  assert.equal(type?.kind, "union");
  assert.deepEqual(type.types.map(providerTypeKey), expectedKinds);
}

export function providerTypeKey(type) {
  return type.kind === "literal"
    ? `literal:${String(type.value)}`
    : type.kind;
}

export function assertDefaultModuleCall(bindingProvider, moduleSpecifier, interfaceName, memberName, signatureId, targetIdentityId) {
  const member = assertDefaultModuleMember(bindingProvider, moduleSpecifier, interfaceName, memberName);
  assert.equal(member?.signatures?.[0]?.id, signatureId);
  assertCallMapping(
    nodejsVirtualMemberDeclaration(
      moduleSpecifier,
      interfaceName,
      memberName,
      member.id,
      signatureId,
    ),
    targetIdentityId,
  );
}

export function assertDefaultModuleSignature(bindingProvider, moduleSpecifier, interfaceName, memberName, signatureId, targetIdentityId) {
  const member = assertDefaultModuleMember(bindingProvider, moduleSpecifier, interfaceName, memberName);
  assert.ok(member?.signatures?.some((signature) => signature.id === signatureId));
  assertCallMapping(
    nodejsVirtualMemberDeclaration(
      moduleSpecifier,
      interfaceName,
      memberName,
      member.id,
      signatureId,
    ),
    targetIdentityId,
  );
}

export function assertDefaultModuleProperty(bindingProvider, moduleSpecifier, interfaceName, memberName, memberId, targetIdentityId) {
  const member = assertDefaultModuleMember(bindingProvider, moduleSpecifier, interfaceName, memberName);
  assert.equal(member?.id, memberId);
  assertPropertyMapping(
    nodejsVirtualMemberDeclaration(
      moduleSpecifier,
      interfaceName,
      memberName,
      memberId,
    ),
    targetIdentityId,
  );
}

export function assertDefaultModuleMember(bindingProvider, moduleSpecifier, interfaceName, memberName) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const defaultDeclaration = model.exports.find((entry) => entry.exportKind === "default");
  assert.equal(defaultDeclaration?.type?.kind, "provider-ref");
  assert.equal(defaultDeclaration?.type?.exportName, interfaceName);
  const moduleDeclaration = model.exports.find((entry) => entry.name === interfaceName);
  assert.equal(moduleDeclaration?.kind, "interface");
  return moduleDeclaration.members?.find((entry) => entry.name === memberName);
}

export function assertSelectedMember(result, memberId) {
  assert.equal(result.kind, "accept");
  assert.equal(result.value.selectedSignature.member.id, memberId);
}

function assertCallMapping(declaration, targetIdentityId) {
  const provider = createCsharpNodejsOperationsTestContribution();
  const facts = new TestFactStore();
  const selectedSignature = {};
  facts.set(selectedSignature, providerVirtualDeclarationFactKey, declaration);
  const result = provider.mapCheckedCall(
    nodejsCallRequest({}, selectedSignature),
    fakeContext(facts),
  );
  assert.notEqual(result.kind, "defer");
  if (result.kind === "accept") {
    if (targetIdentityId === undefined) {
      assert.ok(result.value.selectedSignature.member.id);
    } else {
      assert.equal(result.value.selectedSignature.member.id, targetIdentityId);
    }
    return;
  }
  assert.equal(targetIdentityId, undefined);
  assert.equal(result.diagnostic.extensionCode, "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED");
  assert.ok(result.diagnostic.evidence?.[0]?.details?.targetIdentityId);
}

function assertPropertyMapping(declaration, targetIdentityId) {
  const provider = createCsharpNodejsOperationsTestContribution();
  const facts = new TestFactStore();
  const selectedDeclaration = {};
  facts.set(selectedDeclaration, providerVirtualDeclarationFactKey, declaration);
  const result = provider.mapCheckedPropertyAccess(
    nodejsPropertyRequest({}, selectedDeclaration),
    fakeContext(facts),
  );
  assert.equal(result.kind, "accept");
  assert.equal(result.value.operation.operationId, targetIdentityId);
}

export function assertUnsupportedCall(provider, facts, selectedSignature, targetIdentityId) {
  const result = provider.mapCheckedCall(nodejsCallRequest({}, selectedSignature), fakeContext(facts));
  assert.equal(result.kind, "reject");
  assert.equal(result.diagnostic.extensionCode, "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED");
  assert.equal(result.diagnostic.evidence?.[0]?.details?.targetIdentityId, targetIdentityId);
}

export function assertUnsupportedProperty(provider, facts, selectedDeclaration, targetIdentityId) {
  const result = provider.mapCheckedPropertyAccess(nodejsPropertyRequest({}, selectedDeclaration), fakeContext(facts));
  assert.equal(result.kind, "reject");
  assert.equal(result.diagnostic.extensionCode, "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED");
  assert.equal(result.diagnostic.evidence?.[0]?.details?.targetIdentityId, targetIdentityId);
}

export function fakeContext(facts) {
  return {
    facts,
    factResolver: {
      resolve: (subject, key) => facts.get(subject, key),
    },
  };
}

export function createCsharpSession(sourceText, options = {}) {
  const targetPack = createCsharpTargetPack();
  const target = { id: "csharp" };
  const selectedSurfaces = selectedTargetSurfaces(targetPack, options.selectedSurfaces ?? []);
  const selectedCapabilities = selectedProviderPackages(options.selectedCapabilities ?? []);
  const context = {
    project: {
      entryPoint: "index.ts",
      targets: [target],
    },
    target,
    targetPack,
    selectedSurfaces,
    selectedCapabilities,
  };
  const sourceProfileFiles = csharpTestSourceProfileFiles(context);
  return createCompilerSessionFromFiles({
    currentDirectory: "/src",
    files: new Map([
      ...sourceProfileFiles.map((file) => [file.path, file.text]),
      ["/src/index.ts", sourceText],
    ]),
    rootFiles: [
      ...sourceProfileFiles.map((file) => file.path),
      "/src/index.ts",
    ],
    compilerOptions: {
      module: "esnext",
      moduleResolution: "bundler",
      noLib: true,
      strictNullChecks: true,
      target: "es2022",
    },
    extensionHostOptions: {
      activeTarget: "csharp",
      extensions: [
        createTsonicCoreSourceExtension(),
        createCsharpSourceSemanticsExtension(context),
        createCsharpTargetSemanticsExtension(context),
        ...context.selectedSurfaces.flatMap((surface) =>
          surface.id === "js"
            ? [createCsharpJsSurfaceExtension({ ...context, surface })]
            : []
        ),
        ...context.selectedCapabilities.flatMap((providerPackage) =>
          providerPackage.createExtensions?.({ ...context, capability: providerPackage }) ?? []
        ),
      ],
    },
  });
}

function selectedTargetSurfaces(targetPack, requestedSurfaces) {
  return requestedSurfaces.map((surface) =>
    targetPack.surfaces?.find((candidate) => candidate.id === surface.id) ?? surface
  );
}

function csharpTestSourceProfileFiles(context) {
  const files = [];
  appendSourceProfileDeclarations(files, context.targetPack.provider?.id, context.targetPack.provider?.sourceProfileContributions?.(context)?.declarations ?? []);
  for (const surface of context.selectedSurfaces) {
    appendSourceProfileDeclarations(files, surface.id, surface.sourceProfileContributions?.({ ...context, surface })?.declarations ?? []);
  }
  for (const capability of context.selectedCapabilities) {
    appendSourceProfileDeclarations(files, capability.id, capability.sourceProfileContributions?.({ ...context, capability })?.declarations ?? []);
  }
  return files;
}

function appendSourceProfileDeclarations(files, ownerId, declarations) {
  if (ownerId === undefined) {
    return;
  }
  for (const declaration of declarations) {
    files.push({
      path: `/src/${tsonicSourceProfileVirtualDirectory}/${normalizeTargetSourceProfileSegment(ownerId)}/${declaration.fileName}`,
      text: declaration.text,
    });
  }
}

export function selectedProviderPackages(requestedPackages) {
  return requestedPackages.map((providerPackage) =>
    providerPackage.id === nodejsTestProviderPackage.id
      ? nodejsTestProviderPackage
      : providerPackage
  );
}

export const nodejsTestProviderPackage = {
  id: "@tsonic/csharp-nodejs",
  kind: "target-capability",
  targetId: "csharp",
  displayName: "Node.js provider package",
  requiredSurfaces: ["js"],
  moduleOwnership: [],
  createTargetContributions: createCsharpNodejsTargetContributions,
  createExtensions(context) {
    return [createCsharpNodejsProviderPackageExtension(context)];
  },
};

export function nodejsCallRequest(call, sourceSelectedSignature) {
  return {
    target: "csharp",
    call,
    callee: {},
    arguments: [],
    sourceSelectedSignature,
  };
}

export function nodejsCallRequestWithoutSignature(call, sourceSelectedDeclaration) {
  return {
    target: "csharp",
    call,
    callee: {},
    arguments: [],
    sourceSelectedDeclaration,
  };
}

export function nodejsPropertyRequest(expression, sourceSelectedSymbol) {
  return {
    target: "csharp",
    expression,
    receiver: {},
    receiverType: {},
    propertyName: "selectedByProviderIdentity",
    sourceSelectedSymbol,
  };
}

export function nodejsVirtualDeclaration(moduleSpecifier, exportName, signatureId) {
  return {
    providerId: "tsonic.csharp.provider-package.nodejs",
    providerVersion: "0.0.1",
    providerModuleId: moduleSpecifier,
    moduleSpecifier,
    artifactFileName: `tsts-provider://csharp-nodejs/${encodeURIComponent(moduleSpecifier)}.d.ts`,
    exportName,
    ...(signatureId !== undefined ? { signatureId } : {}),
  };
}

export function nodejsVirtualMemberDeclaration(moduleSpecifier, exportName, memberName, memberId, signatureId) {
  return {
    ...nodejsVirtualDeclaration(moduleSpecifier, exportName),
    memberName,
    memberKey: { kind: "property-key", name: memberName },
    memberId,
    ...(signatureId !== undefined ? { signatureId } : {}),
  };
}

export function collectFactValues(sourceFile, session, extensionHost, factKey) {
  return collectAllNodes(sourceFile, session.ast)
    .map((node) => extensionHost.facts.get(node, factKey))
    .filter((fact) => fact !== undefined);
}

export function collectAllNodes(node, ast, result = []) {
  if (node === undefined) {
    return result;
  }
  result.push(node);
  for (const child of ast.children(node) ?? []) {
    collectAllNodes(child, ast, result);
  }
  return result;
}

export class TestFactStore {
  #facts = new Map();

  get(subject, key) {
    return this.#facts.get(subject)?.get(key);
  }

  set(subject, key, value) {
    let subjectFacts = this.#facts.get(subject);
    if (subjectFacts === undefined) {
      subjectFacts = new Map();
      this.#facts.set(subject, subjectFacts);
    }
    subjectFacts.set(key, value);
  }
}
