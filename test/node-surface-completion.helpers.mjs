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
  const signature = declaration?.signatures?.find((candidate) => candidate.id === signatureId);
  assert.ok(signature);
  assertCallMapping(
    nodejsVirtualDeclaration(moduleSpecifier, exportName, signatureId),
    targetIdentityId,
    signature,
  );
}

export function assertClassMember(bindingProvider, moduleSpecifier, exportName, memberName, signatureId, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  const member = declaration?.members?.find((entry) => entry.name === memberName);
  const signature = member?.signatures?.find((candidate) => candidate.id === signatureId);
  assert.ok(signature);
  assertCallMapping(
    nodejsVirtualMemberDeclaration(
      moduleSpecifier,
      exportName,
      memberName,
      member.id,
      signatureId,
    ),
    targetIdentityId,
    signature,
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
    memberName,
  );
}

export function assertClassElement(bindingProvider, moduleSpecifier, exportName, memberName, signatureId, targetIdentityId) {
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const declaration = model.exports.find((entry) => entry.name === exportName);
  const member = declaration?.members?.find((entry) => entry.name === memberName);
  const signature = member?.signatures?.find((candidate) => candidate.id === signatureId);
  assert.ok(signature);
  assertElementMapping(
    nodejsVirtualMemberDeclaration(moduleSpecifier, exportName, memberName, member.id, signatureId),
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
    exportName,
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
  const signature = member?.signatures?.[0];
  assert.equal(signature?.id, signatureId);
  assertCallMapping(
    nodejsVirtualMemberDeclaration(
      moduleSpecifier,
      interfaceName,
      memberName,
      member.id,
      signatureId,
    ),
    targetIdentityId,
    signature,
  );
}

export function assertDefaultModuleSignature(bindingProvider, moduleSpecifier, interfaceName, memberName, signatureId, targetIdentityId) {
  const member = assertDefaultModuleMember(bindingProvider, moduleSpecifier, interfaceName, memberName);
  const signature = member?.signatures?.find((candidate) => candidate.id === signatureId);
  assert.ok(signature);
  assertCallMapping(
    nodejsVirtualMemberDeclaration(
      moduleSpecifier,
      interfaceName,
      memberName,
      member.id,
      signatureId,
    ),
    targetIdentityId,
    signature,
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
    memberName,
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

function assertCallMapping(declaration, targetIdentityId, signature) {
  const provider = createCsharpNodejsOperationsTestContribution();
  const facts = new TestFactStore();
  const selectedSignature = {};
  facts.set(selectedSignature, providerVirtualDeclarationFactKey, declaration);
  const result = provider.mapCheckedCall(
    nodejsCallRequest({}, selectedSignature, signature),
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

function assertPropertyMapping(declaration, targetIdentityId, propertyName) {
  const provider = createCsharpNodejsOperationsTestContribution();
  const facts = new TestFactStore();
  const selectedDeclaration = {};
  facts.set(selectedDeclaration, providerVirtualDeclarationFactKey, declaration);
  const result = provider.mapCheckedPropertyAccess(
    nodejsPropertyRequest({}, selectedDeclaration, propertyName),
    fakeContext(facts),
  );
  assert.equal(result.kind, "accept");
  assert.equal(result.value.operation.operationId, targetIdentityId);
}

function assertElementMapping(declaration, targetIdentityId) {
  const provider = createCsharpNodejsOperationsTestContribution();
  const facts = new TestFactStore();
  const selectedDeclaration = {};
  facts.set(selectedDeclaration, providerVirtualDeclarationFactKey, declaration);
  const result = provider.mapCheckedElementAccess(
    nodejsElementRequest({}, selectedDeclaration),
    fakeContext(facts),
  );
  assert.equal(result.kind, "accept");
  assert.equal(result.value.operation.operationId, targetIdentityId);
}

export function assertUnsupportedCall(provider, facts, selectedSignature, targetIdentityId) {
  const result = mapNodejsCheckedCall(provider, facts, {}, selectedSignature);
  assert.equal(result.kind, "reject");
  assert.equal(result.diagnostic.extensionCode, "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED");
  assert.equal(result.diagnostic.evidence?.[0]?.details?.targetIdentityId, targetIdentityId);
}

export function assertUnsupportedProperty(provider, facts, selectedDeclaration, targetIdentityId) {
  const declaration = facts.get(selectedDeclaration, providerVirtualDeclarationFactKey);
  const propertyName = declaration?.memberName ?? declaration?.exportName;
  assert.equal(typeof propertyName, "string");
  const result = provider.mapCheckedPropertyAccess(
    nodejsPropertyRequest({}, selectedDeclaration, propertyName),
    fakeContext(facts),
  );
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

export function mapNodejsCheckedCall(provider, facts, call, sourceSelectedSignature, sourceArgumentCount) {
  const declaration = facts.get(sourceSelectedSignature, providerVirtualDeclarationFactKey);
  assert.ok(declaration);
  const signature = getNodejsProviderSignature(declaration);
  assert.ok(signature);
  return provider.mapCheckedCall(
    nodejsCallRequest(call, sourceSelectedSignature, signature, sourceArgumentCount),
    fakeContext(facts),
  );
}

export function nodejsCallRequest(call, sourceSelectedSignature, signature, sourceArgumentCount = minimumSourceArgumentCount(signature)) {
  const callee = {};
  const sourceArguments = Array.from({ length: sourceArgumentCount }, () => ({
    expression: {},
    type: {},
  }));
  const parameters = signature.parameters.map((parameter, parameterIndex) => ({
    parameterIndex,
    parameterName: parameter.name,
    parameterSymbol: {},
    selectedType: {},
    acceptsOmission: parameter.optional === true || parameter.rest === true,
    rest: parameter.rest === true,
  }));
  assert.ok(sourceArgumentCount <= parameters.length || parameters.at(-1)?.rest === true);
  return {
    target: "csharp",
    sourceOperationKind: "call",
    call,
    callee,
    arguments: sourceArguments.map((argument) => argument.expression),
    callKind: "call",
    sourceSelection: {
      kind: "applicable",
      signature: sourceSelectedSignature,
      methodTypeArguments: [],
      parameters,
      argumentBindings: sourceArguments.map((argument, sourceArgumentIndex) => {
        const sourceParameterIndex = Math.min(sourceArgumentIndex, parameters.length - 1);
        const parameter = parameters[sourceParameterIndex];
        assert.ok(parameter);
        return {
          sourceArgumentIndex,
          effectiveArgumentIndex: sourceArgumentIndex,
          sourceForm: "value",
          sourceParameterIndex,
          sourceParameterForm: parameter.rest ? "rest-element" : "parameter",
          selectedArgumentType: argument.type,
          selectedParameterType: parameter.selectedType,
        };
      }),
    },
    sourceCallee: {
      expression: callee,
      type: {},
      selectedDeclaration: sourceSelectedSignature,
    },
    sourceArguments,
    sourceResult: {
      expression: call,
      type: {},
    },
    chainRole: {
      kind: "ordinary",
      participant: "call",
    },
  };
}

export function nodejsCallRequestWithoutSignature(call, sourceSelectedDeclaration, sourceArgumentCount = 0) {
  const callee = {};
  const sourceArguments = Array.from({ length: sourceArgumentCount }, () => ({
    expression: {},
    type: {},
  }));
  return {
    target: "csharp",
    sourceOperationKind: "call",
    call,
    callee,
    arguments: sourceArguments.map((argument) => argument.expression),
    callKind: "call",
    sourceSelection: { kind: "untyped" },
    sourceCallee: {
      expression: callee,
      type: {},
      selectedDeclaration: sourceSelectedDeclaration,
    },
    sourceArguments,
    sourceResult: {
      expression: call,
      type: {},
    },
    chainRole: {
      kind: "ordinary",
      participant: "call",
    },
  };
}

export function nodejsPropertyRequest(expression, sourceSelectedDeclaration, propertyName, use = "value") {
  const receiver = {};
  return {
    target: "csharp",
    sourceOperationKind: "property-access",
    expression,
    receiver,
    propertyName,
    sourceReceiver: {
      expression: receiver,
      type: {},
    },
    accessMode: "read",
    use,
    sourceReadResult: {
      expression,
      type: {},
      selectedDeclaration: sourceSelectedDeclaration,
    },
    chainRole: {
      kind: "ordinary",
      participant: "property-access",
    },
  };
}

export function nodejsElementRequest(expression, sourceSelectedDeclaration) {
  const receiver = {};
  const argument = {};
  return {
    target: "csharp",
    sourceOperationKind: "element-access",
    expression,
    receiver,
    argument,
    sourceArgument: {
      expression: argument,
      type: {},
    },
    sourceReceiver: {
      expression: receiver,
      type: {},
    },
    accessMode: "read",
    use: "value",
    sourceReadResult: {
      expression,
      type: {},
      selectedDeclaration: sourceSelectedDeclaration,
    },
    chainRole: {
      kind: "ordinary",
      participant: "element-access",
    },
  };
}

function minimumSourceArgumentCount(signature) {
  let count = 0;
  for (const [index, parameter] of signature.parameters.entries()) {
    if (parameter.optional !== true && parameter.rest !== true) {
      count = index + 1;
    }
  }
  return count;
}

function getNodejsProviderSignature(declaration) {
  const bindingProvider = createCsharpNodejsProviderPackageBindingProvider();
  const resolution = bindingProvider.resolveModule(declaration.moduleSpecifier, {});
  if (resolution.kind !== "virtual") {
    return undefined;
  }
  const model = bindingProvider.getDeclarationModel(resolution);
  const exported = model.exports.find((entry) => entry.name === declaration.exportName);
  const callable = declaration.memberName === undefined
    ? exported
    : exported?.members?.find((entry) => entry.name === declaration.memberName);
  return callable?.signatures?.find((entry) => entry.id === declaration.signatureId);
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
