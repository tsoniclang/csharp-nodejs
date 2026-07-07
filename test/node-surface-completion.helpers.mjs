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
  createCsharpJsSurfaceExtension,
  createCsharpSourceSemanticsExtension,
  createCsharpTargetSemanticsExtension,
} from "../../tsonic-csharp/dist/index.js";
import {
  createCsharpNodejsProviderPackageBindingProvider,
  createCsharpNodejsProviderPackageExtension,
  createCsharpNodejsProviderPackageOperationsMappers,
  createCsharpNodejsProviderPackageOperationsProvider,
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
export { test, assert, createCompilerSessionFromFiles, formatDiagnostics, providerVirtualDeclarationFactKey, selectedTargetSignatureFactKey, createTsonicCoreSourceExtension, csharpTargetOperationFactKey, createCsharpJsSurfaceExtension, createCsharpSourceSemanticsExtension, createCsharpTargetSemanticsExtension, createCsharpNodejsProviderPackageBindingProvider, createCsharpNodejsProviderPackageExtension, createCsharpNodejsProviderPackageOperationsMappers, createCsharpNodejsProviderPackageOperationsProvider, nodeFsCallTargetMembers, nodeFsModuleSpecifier, nodeFsPromisesCallTargetMembers, nodeFsPromisesModuleSpecifier, nodeFsUnsupportedTargetIdentities, nodeCryptoCallTargetMembers, nodeCryptoClassCallTargetMembers, nodeCryptoModuleSpecifier, nodeCryptoUnsupportedTargetIdentities, nodeOsCallTargetMembers, nodeOsModuleSpecifier, nodeOsPropertyTargetMembers, nodeOsUnsupportedTargetIdentities };













export function assertModuleExport(bindingProvider, moduleSpecifier, exportName, signatureId, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  assert.ok(declaration?.signatures?.some((signature) => signature.id === signatureId));
  const identity = bindingProvider.getTargetIdentity({
    moduleSpecifier,
    exportName,
    signatureId,
  });
  if (targetIdentityId === undefined) {
    assert.ok(identity?.id);
  } else {
    assert.equal(identity?.id, targetIdentityId);
  }
}

export function assertClassMember(bindingProvider, moduleSpecifier, exportName, memberName, signatureId, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  const member = declaration?.members?.find((entry) => entry.name === memberName);
  assert.ok(member?.signatures?.some((signature) => signature.id === signatureId));
  const identity = bindingProvider.getTargetIdentity({
    moduleSpecifier,
    exportName,
    memberName,
    signatureId,
  });
  if (targetIdentityId === undefined) {
    assert.ok(identity?.id);
  } else {
    assert.equal(identity?.id, targetIdentityId);
  }
}

export function assertClassProperty(bindingProvider, moduleSpecifier, exportName, memberName, memberId, targetIdentityId = memberId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  const member = declaration?.members?.find((entry) => entry.name === memberName);
  assert.equal(member?.id, memberId);
  const identity = bindingProvider.getTargetIdentity({
    moduleSpecifier,
    exportName,
    memberName,
  });
  assert.equal(identity?.id, targetIdentityId);
}

export function assertModuleValue(bindingProvider, moduleSpecifier, exportName, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  assert.equal(declaration?.kind, "value");
  const identity = bindingProvider.getTargetIdentity({
    moduleSpecifier,
    exportName,
  });
  assert.equal(identity?.id, targetIdentityId);
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
  const identity = bindingProvider.getTargetIdentity({
    moduleSpecifier,
    exportName: interfaceName,
    memberName,
    signatureId,
  });
  assert.equal(identity?.id, targetIdentityId);
}

export function assertDefaultModuleSignature(bindingProvider, moduleSpecifier, interfaceName, memberName, signatureId, targetIdentityId) {
  const member = assertDefaultModuleMember(bindingProvider, moduleSpecifier, interfaceName, memberName);
  assert.ok(member?.signatures?.some((signature) => signature.id === signatureId));
  const identity = bindingProvider.getTargetIdentity({
    moduleSpecifier,
    exportName: interfaceName,
    memberName,
    signatureId,
  });
  assert.equal(identity?.id, targetIdentityId);
}

export function assertDefaultModuleProperty(bindingProvider, moduleSpecifier, interfaceName, memberName, memberId, targetIdentityId) {
  const member = assertDefaultModuleMember(bindingProvider, moduleSpecifier, interfaceName, memberName);
  assert.equal(member?.id, memberId);
  const identity = bindingProvider.getTargetIdentity({
    moduleSpecifier,
    exportName: interfaceName,
    memberName,
  });
  assert.equal(identity?.id, targetIdentityId);
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
  const target = { id: "csharp" };
  const selectedCapabilities = selectedProviderPackages(options.selectedCapabilities ?? []);
  const context = {
    project: {
      entryPoint: "index.ts",
      targets: [target],
    },
    target,
    selectedSurfaces: options.selectedSurfaces ?? [],
    selectedCapabilities,
  };
  return createCompilerSessionFromFiles({
    currentDirectory: "/src",
    files: new Map([
      ["/src/index.ts", sourceText],
    ]),
    compilerOptions: {
      module: "esnext",
      moduleResolution: "bundler",
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
            ? [createCsharpJsSurfaceExtension({ ...context, surface, targetPack: fakeTargetPack })]
            : []
        ),
        ...context.selectedCapabilities.flatMap((providerPackage) =>
          providerPackage.createExtensions?.({ ...context, capability: providerPackage, targetPack: fakeTargetPack }) ?? []
        ),
      ],
    },
  });
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
  createOperationMappers: createCsharpNodejsProviderPackageOperationsMappers,
  createExtensions(context) {
    return [createCsharpNodejsProviderPackageExtension(context)];
  },
};

export const fakeTargetPack = {
  id: "csharp",
  displayName: "C#",
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
    virtualFileName: `tsts-provider://csharp-nodejs/${encodeURIComponent(moduleSpecifier)}.d.ts`,
    exportName,
    ...(signatureId !== undefined ? { signatureId } : {}),
  };
}

export function nodejsVirtualMemberDeclaration(moduleSpecifier, exportName, memberName, memberId, signatureId) {
  return {
    ...nodejsVirtualDeclaration(moduleSpecifier, exportName),
    memberName,
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
