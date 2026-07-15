import { test, assert, createCompilerSessionFromFiles, formatDiagnostics, providerVirtualDeclarationFactKey, selectedTargetSignatureFactKey, createTsonicCoreSourceExtension, csharpTargetOperationFactKey, createCsharpJsSurfaceExtension, createCsharpSourceSemanticsExtension, createCsharpTargetSemanticsExtension, createCsharpNodejsProviderPackageBindingProvider, createCsharpNodejsProviderPackageExtension, createCsharpNodejsProviderPackageOperationsMappers, createCsharpNodejsProviderPackageOperationsProvider, nodeFsCallTargetMembers, nodeFsModuleSpecifier, nodeFsPromisesCallTargetMembers, nodeFsPromisesModuleSpecifier, nodeFsUnsupportedTargetIdentities, nodeCryptoCallTargetMembers, nodeCryptoClassCallTargetMembers, nodeCryptoModuleSpecifier, nodeCryptoUnsupportedTargetIdentities, nodeOsCallTargetMembers, nodeOsModuleSpecifier, nodeOsPropertyTargetMembers, nodeOsUnsupportedTargetIdentities, assertModuleExport, assertClassMember, assertClassProperty, assertModuleValue, assertProviderUnionType, providerTypeKey, assertDefaultModuleCall, assertDefaultModuleSignature, assertDefaultModuleProperty, assertDefaultModuleMember, assertSelectedMember, assertUnsupportedCall, assertUnsupportedProperty, fakeContext, createCsharpSession, selectedProviderPackages, nodejsTestProviderPackage, nodejsCallRequest, nodejsCallRequestWithoutSignature, nodejsPropertyRequest, nodejsVirtualDeclaration, nodejsVirtualMemberDeclaration, collectFactValues, collectAllNodes, TestFactStore } from "./node-surface-completion.helpers.mjs";

test("NodeJS provider package exposes completion metadata for assigned modules", () => {
  const bindingProvider = createCsharpNodejsProviderPackageBindingProvider();

  assertModuleExport(bindingProvider, "node:fs", "watchFile", "node:fs.watchFile(System.String,Function)");
  assertModuleExport(bindingProvider, "node:fs", "readdirSync", "node:fs.readdirSync(System.String)");
  assertModuleExport(bindingProvider, "node:fs/promises", "readFile", "node:fs/promises.readFile(System.String,System.String)");
  assertModuleExport(bindingProvider, "node:fs/promises", "readFile", "node:fs/promises.readFile(System.String)");
  assertModuleExport(bindingProvider, "node:fs/promises", "writeFile", "node:fs/promises.writeFile(System.String,Tsonic.CSharp.Node.Buffer)");
  assertModuleExport(bindingProvider, "node:fs/promises", "appendFile", "node:fs/promises.appendFile(System.String,Tsonic.CSharp.Node.Buffer)");
  assertModuleExport(bindingProvider, "node:fs/promises", "readdir", "node:fs/promises.readdir(System.String)");
  assertModuleExport(bindingProvider, "node:path", "format", "node:path.format(Tsonic.CSharp.Node.ParsedPath)");
  assertClassMember(bindingProvider, "node:buffer", "Buffer", "compare", "node:buffer.Buffer.compare(Tsonic.CSharp.Node.Buffer,Tsonic.CSharp.Node.Buffer)");
  assertClassMember(bindingProvider, "node:buffer", "Buffer", "includes", "node:buffer.Buffer.includes(System.Object,System.Int32,System.String)");
  assertClassMember(bindingProvider, "node:buffer", "Buffer", "readUInt8", "node:buffer.Buffer.readUInt8(System.Int32)");
  assertClassMember(bindingProvider, "node:buffer", "Buffer", "writeUInt8", "node:buffer.Buffer.writeUInt8(System.Byte,System.Int32)");
  assertClassMember(bindingProvider, "node:buffer", "Buffer", "isBuffer", "node:buffer.Buffer.isBuffer(System.Object)");
  assertClassProperty(bindingProvider, "node:buffer", "Buffer", "poolSize", "node:buffer.Buffer.poolSize", "Tsonic.CSharp.Node.Buffer.poolSize");
  assertModuleExport(bindingProvider, "node:buffer", "transcode", "node:buffer.transcode(Tsonic.CSharp.Node.Buffer,System.String,System.String)");
  assertModuleExport(bindingProvider, "node:crypto", "createCipheriv", "node:crypto.createCipheriv(System.String,System.Object,System.Object)");
  assertModuleExport(bindingProvider, "node:os", "cpus", "node:os.cpus()");
  assertModuleExport(bindingProvider, "node:process", "hrtime", "node:process.hrtime(System.Double[])");
  assertModuleExport(bindingProvider, "node:process", "memoryUsage", "node:process.memoryUsage()");
  assertClassProperty(bindingProvider, "node:process", "MemoryUsage", "rss", "Tsonic.CSharp.Node.MemoryUsage.rss");
  assertModuleExport(bindingProvider, "node:util", "styleText", "node:util.styleText(System.String,System.String)");
  assertModuleExport(bindingProvider, "node:util", "format", "node:util.format(System.Object,System.Object[])");
  assertClassMember(bindingProvider, "node:url", "URLSearchParams", "append", "node:url.URLSearchParams.append(System.String,System.String)");
  assertClassProperty(bindingProvider, "node:url", "URLSearchParams", "size", "node:url.URLSearchParams.size", "Tsonic.CSharp.Node.URLSearchParams.size");
  assertDefaultModuleCall(bindingProvider, "node:fs", "NodeFsModule", "existsSync", "node:fs.existsSync(System.String)", "Tsonic.CSharp.Node.fs.existsSync(System.String)");
  assertDefaultModuleCall(bindingProvider, "node:fs/promises", "NodeFsPromisesModule", "readFile", "node:fs/promises.readFile(System.String,System.String)", "Tsonic.CSharp.Node.fs_promises.readFile(System.String,System.String)");
  assertDefaultModuleSignature(bindingProvider, "node:fs/promises", "NodeFsPromisesModule", "readFile", "node:fs/promises.readFile(System.String)", "Tsonic.CSharp.Node.fs_promises.readFile(System.String)");
  assertDefaultModuleCall(bindingProvider, "node:path", "NodePathModule", "join", "node:path.join(System.String[])", "Tsonic.CSharp.Node.path.join(System.String[])");
  assertDefaultModuleCall(bindingProvider, "node:process", "NodeProcessModule", "cwd", "node:process.cwd()", "Tsonic.CSharp.Node.process.cwd()");
  assertDefaultModuleProperty(bindingProvider, "node:process", "NodeProcessModule", "platform", "node:process.NodeProcessModule.platform", "Tsonic.CSharp.Node.process.platform");
  assertDefaultModuleCall(bindingProvider, "node:util", "NodeUtilModule", "toUSVString", "node:util.toUSVString(System.String)", "Tsonic.CSharp.Node.util.toUSVString(System.String)");
  assertDefaultModuleCall(bindingProvider, "node:url", "NodeUrlModule", "pathToFileURL", "node:url.pathToFileURL(System.String)", "Tsonic.CSharp.Node.url.pathToFileURL(System.String)");
});
test("NodeJS promise declarations preserve the target Task carrier and exact source-global shape", () => {
  const bindingProvider = createCsharpNodejsProviderPackageBindingProvider();
  const resolution = bindingProvider.resolveModule("node:fs/promises", {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const readFile = model.exports.find((entry) => entry.name === "readFile");
  const signature = readFile?.signatures?.find((entry) => entry.id === "node:fs/promises.readFile(System.String)");

  assert.equal(signature?.returnType?.kind, "target-named");
  assert.equal(signature.returnType.target, "csharp");
  assert.equal(signature.returnType.id, "System.Threading.Tasks.Task`1");
  assert.deepEqual(signature.returnType.typeArguments, [{
    kind: "provider-ref",
    moduleSpecifier: "node:buffer",
    exportName: "Buffer",
  }]);
  assert.deepEqual(signature.returnType.sourceShape, {
    kind: "source-global",
    name: "Promise",
    typeArguments: [{
      kind: "provider-ref",
      moduleSpecifier: "node:buffer",
      exportName: "Buffer",
    }],
  });
});
test("NodeJS process provider metadata exposes provider-owned nullish union shapes", () => {
  const bindingProvider = createCsharpNodejsProviderPackageBindingProvider();
  const resolution = bindingProvider.resolveModule("node:process", {});
  assert.equal(resolution.kind, "virtual");
  const model = bindingProvider.getDeclarationModel(resolution);
  const exitCode = model.exports.find((entry) => entry.name === "exitCode");
  const processEnv = model.exports.find((entry) => entry.name === "ProcessEnv");
  const envIndexer = processEnv?.members?.find((entry) => entry.name === "Item");
  const envIndexerSignature = envIndexer?.signatures?.find((signature) => signature.id === "Tsonic.CSharp.Node.ProcessEnv.Item(System.String)");

  assertProviderUnionType(exitCode?.type, ["number", "literal:null"]);
  assert.equal(envIndexer?.kind, "indexer");
  assertProviderUnionType(envIndexerSignature?.returnType, ["string", "void"]);
  assertClassProperty(bindingProvider, "node:process", "ProcessEnv", "Item", "Tsonic.CSharp.Node.ProcessEnv.Item(System.String)");
});
test("NodeJS fs provider metadata exposes every supported operation row by provider signature identity", () => {
  const bindingProvider = createCsharpNodejsProviderPackageBindingProvider();

  for (const row of nodeFsCallTargetMembers()) {
    assertModuleExport(bindingProvider, nodeFsModuleSpecifier, row.exportName, row.signatureId, row.targetMemberId);
  }

  for (const row of nodeFsPromisesCallTargetMembers()) {
    assertModuleExport(bindingProvider, nodeFsPromisesModuleSpecifier, row.exportName, row.signatureId, row.targetMemberId);
  }
});
test("NodeJS Buffer, crypto, and os provider metadata exposes operation rows by provider identity", () => {
  const bindingProvider = createCsharpNodejsProviderPackageBindingProvider();

  for (const row of nodeCryptoCallTargetMembers()) {
    assertModuleExport(bindingProvider, nodeCryptoModuleSpecifier, row.exportName, row.signatureId, row.targetMemberId);
  }

  for (const row of nodeCryptoClassCallTargetMembers()) {
    assertClassMember(bindingProvider, nodeCryptoModuleSpecifier, row.exportName, row.memberName, row.signatureId, row.targetMemberId);
  }

  for (const row of nodeOsCallTargetMembers()) {
    assertModuleExport(bindingProvider, nodeOsModuleSpecifier, row.exportName, row.signatureId, row.member.id);
  }

  for (const row of nodeOsPropertyTargetMembers()) {
    assertModuleValue(bindingProvider, nodeOsModuleSpecifier, row.exportName, row.member.id);
  }
});
test("NodeJS provider package maps closed operations from selected provider identities", () => {
  const facts = new TestFactStore();
  const provider = createCsharpNodejsProviderPackageOperationsProvider();
  const readFileCall = {};
  const readFileSignature = {};
  const readdirSyncCall = {};
  const readdirSyncSignature = {};
  const pathParseCall = {};
  const pathParseSignature = {};
  const fsPromisesReadCall = {};
  const fsPromisesReadSignature = {};
  const fsPromisesReadBytesCall = {};
  const fsPromisesReadBytesSignature = {};
  const fsPromisesReaddirCall = {};
  const fsPromisesReaddirSignature = {};
  const parsedBaseExpression = {};
  const parsedBaseDeclaration = {};
  const processCwdCall = {};
  const processCwdSignature = {};
  const bufferCompareCall = {};
  const bufferCompareSignature = {};
  const bufferIncludesCall = {};
  const bufferIncludesSignature = {};
  const bufferReadUInt8Call = {};
  const bufferReadUInt8Signature = {};
  const bufferWriteUInt8Call = {};
  const bufferWriteUInt8Signature = {};
  const bufferIsBufferSignature = {};
  const bufferPoolSizeExpression = {};
  const bufferPoolSizeDeclaration = {};
  const bufferTranscodeCall = {};
  const bufferTranscodeSignature = {};
  const cryptoHmacCall = {};
  const cryptoHmacSignature = {};
  const osHomedirCall = {};
  const osHomedirSignature = {};
  const processHrtimeCall = {};
  const processHrtimeSignature = {};
  const processMemoryCall = {};
  const processMemorySignature = {};
  const processMemoryRssExpression = {};
  const processMemoryRssDeclaration = {};
  const processUptimeCall = {};
  const processUptimeSignature = {};
  const utilCall = {};
  const utilSignature = {};
  const utilStyleTextCall = {};
  const utilStyleTextSignature = {};
  const defaultFsExistsCall = {};
  const defaultFsExistsSignature = {};
  const defaultProcessPlatformExpression = {};
  const defaultProcessPlatformDeclaration = {};
  const defaultUtilFormatSignature = {};
  const urlCanParseCall = {};
  const urlCanParseSignature = {};
  const urlSearchParamsAppendCall = {};
  const urlSearchParamsAppendSignature = {};
  const urlSearchParamsSizeExpression = {};
  const urlSearchParamsSizeDeclaration = {};
  facts.set(readFileSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:fs", "readFileSync", "node:fs.readFileSync(System.String,System.String)"));
  facts.set(readdirSyncSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:fs", "readdirSync", "node:fs.readdirSync(System.String)"));
  facts.set(pathParseSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:path", "parse", "node:path.parse(System.String)"));
  facts.set(fsPromisesReadSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:fs/promises", "readFile", "node:fs/promises.readFile(System.String,System.String)"));
  facts.set(fsPromisesReadBytesSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:fs/promises", "readFile", "node:fs/promises.readFile(System.String)"));
  facts.set(fsPromisesReaddirSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:fs/promises", "readdir", "node:fs/promises.readdir(System.String)"));
  facts.set(parsedBaseDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:path", "ParsedPath", "base", "node:path.ParsedPath.base"));
  facts.set(processCwdSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("process", "cwd", "node:process.cwd()"));
  facts.set(bufferCompareSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("buffer", "Buffer", "compare", "node:buffer.Buffer.compare", "node:buffer.Buffer.compare(Tsonic.CSharp.Node.Buffer,Tsonic.CSharp.Node.Buffer)"));
  facts.set(bufferIncludesSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:buffer", "Buffer", "includes", "node:buffer.Buffer.includes", "node:buffer.Buffer.includes(System.Object,System.Int32,System.String)"));
  facts.set(bufferReadUInt8Signature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:buffer", "Buffer", "readUInt8", "node:buffer.Buffer.readUInt8", "node:buffer.Buffer.readUInt8(System.Int32)"));
  facts.set(bufferWriteUInt8Signature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:buffer", "Buffer", "writeUInt8", "node:buffer.Buffer.writeUInt8", "node:buffer.Buffer.writeUInt8(System.Byte,System.Int32)"));
  facts.set(bufferIsBufferSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:buffer", "Buffer", "isBuffer", "node:buffer.Buffer.isBuffer", "node:buffer.Buffer.isBuffer(System.Object)"));
  facts.set(bufferPoolSizeDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:buffer", "Buffer", "poolSize", "node:buffer.Buffer.poolSize"));
  facts.set(bufferTranscodeSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:buffer", "transcode", "node:buffer.transcode(Tsonic.CSharp.Node.Buffer,System.String,System.String)"));
  facts.set(cryptoHmacSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:crypto", "createHmac", "node:crypto.createHmac(System.String,Tsonic.CSharp.Node.Buffer)"));
  facts.set(osHomedirSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("os", "homedir", "node:os.homedir()"));
  facts.set(processHrtimeSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:process", "hrtime", "node:process.hrtime(System.Double[])"));
  facts.set(processMemorySignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:process", "memoryUsage", "node:process.memoryUsage()"));
  facts.set(processMemoryRssDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:process", "MemoryUsage", "rss", "Tsonic.CSharp.Node.MemoryUsage.rss"));
  facts.set(processUptimeSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:process", "uptime", "node:process.uptime()"));
  facts.set(utilSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:util", "toUSVString", "node:util.toUSVString(System.String)"));
  facts.set(utilStyleTextSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:util", "styleText", "node:util.styleText(System.String,System.String)"));
  facts.set(defaultFsExistsSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:fs", "NodeFsModule", "existsSync", "node:fs.NodeFsModule.existsSync", "node:fs.existsSync(System.String)"));
  facts.set(defaultProcessPlatformDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:process", "NodeProcessModule", "platform", "node:process.NodeProcessModule.platform"));
  facts.set(defaultUtilFormatSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:util", "NodeUtilModule", "format", "node:util.NodeUtilModule.format", "node:util.format(System.Object,System.Object[])"));
  facts.set(urlCanParseSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("url", "URL", "canParse", "node:url.URL.canParse", "node:url.URL.canParse(System.String,Tsonic.CSharp.Node.URL)"));
  facts.set(urlSearchParamsAppendSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:url", "URLSearchParams", "append", "node:url.URLSearchParams.append", "node:url.URLSearchParams.append(System.String,System.String)"));
  facts.set(urlSearchParamsSizeDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:url", "URLSearchParams", "size", "node:url.URLSearchParams.size"));

  const readFileResult = provider.mapCheckedCall(nodejsCallRequest(readFileCall, readFileSignature), fakeContext(facts));
  const readdirSyncResult = provider.mapCheckedCall(nodejsCallRequest(readdirSyncCall, readdirSyncSignature), fakeContext(facts));
  const pathParseResult = provider.mapCheckedCall(nodejsCallRequest(pathParseCall, pathParseSignature), fakeContext(facts));
  const fsPromisesReadResult = provider.mapCheckedCall(nodejsCallRequest(fsPromisesReadCall, fsPromisesReadSignature), fakeContext(facts));
  const fsPromisesReadBytesResult = provider.mapCheckedCall(nodejsCallRequest(fsPromisesReadBytesCall, fsPromisesReadBytesSignature), fakeContext(facts));
  const fsPromisesReaddirResult = provider.mapCheckedCall(nodejsCallRequest(fsPromisesReaddirCall, fsPromisesReaddirSignature), fakeContext(facts));
  const parsedBaseResult = provider.mapCheckedPropertyAccess(nodejsPropertyRequest(parsedBaseExpression, parsedBaseDeclaration), fakeContext(facts));
  const processCwdResult = provider.mapCheckedCall(nodejsCallRequest(processCwdCall, processCwdSignature), fakeContext(facts));
  const bufferCompareResult = provider.mapCheckedCall(nodejsCallRequest(bufferCompareCall, bufferCompareSignature), fakeContext(facts));
  const bufferIncludesResult = provider.mapCheckedCall(nodejsCallRequest(bufferIncludesCall, bufferIncludesSignature), fakeContext(facts));
  const bufferReadUInt8Result = provider.mapCheckedCall(nodejsCallRequest(bufferReadUInt8Call, bufferReadUInt8Signature), fakeContext(facts));
  const bufferWriteUInt8Result = provider.mapCheckedCall(nodejsCallRequest(bufferWriteUInt8Call, bufferWriteUInt8Signature), fakeContext(facts));
  const bufferIsBufferResult = provider.mapCheckedCall(nodejsCallRequest({}, bufferIsBufferSignature), fakeContext(facts));
  const bufferPoolSizeResult = provider.mapCheckedPropertyAccess(nodejsPropertyRequest(bufferPoolSizeExpression, bufferPoolSizeDeclaration), fakeContext(facts));
  const bufferTranscodeResult = provider.mapCheckedCall(nodejsCallRequest(bufferTranscodeCall, bufferTranscodeSignature), fakeContext(facts));
  const cryptoHmacResult = provider.mapCheckedCall(nodejsCallRequest(cryptoHmacCall, cryptoHmacSignature), fakeContext(facts));
  const osHomedirResult = provider.mapCheckedCall(nodejsCallRequest(osHomedirCall, osHomedirSignature), fakeContext(facts));
  const processHrtimeResult = provider.mapCheckedCall(nodejsCallRequest(processHrtimeCall, processHrtimeSignature), fakeContext(facts));
  const processMemoryResult = provider.mapCheckedCall(nodejsCallRequest(processMemoryCall, processMemorySignature), fakeContext(facts));
  const processMemoryRssResult = provider.mapCheckedPropertyAccess(nodejsPropertyRequest(processMemoryRssExpression, processMemoryRssDeclaration), fakeContext(facts));
  const processUptimeResult = provider.mapCheckedCall(nodejsCallRequest(processUptimeCall, processUptimeSignature), fakeContext(facts));
  const utilResult = provider.mapCheckedCall(nodejsCallRequest(utilCall, utilSignature), fakeContext(facts));
  const utilStyleTextResult = provider.mapCheckedCall(nodejsCallRequest(utilStyleTextCall, utilStyleTextSignature), fakeContext(facts));
  const defaultFsExistsResult = provider.mapCheckedCall(nodejsCallRequest(defaultFsExistsCall, defaultFsExistsSignature), fakeContext(facts));
  const defaultProcessPlatformResult = provider.mapCheckedPropertyAccess(nodejsPropertyRequest(defaultProcessPlatformExpression, defaultProcessPlatformDeclaration), fakeContext(facts));
  const urlCanParseResult = provider.mapCheckedCall(nodejsCallRequest(urlCanParseCall, urlCanParseSignature), fakeContext(facts));
  const urlSearchParamsAppendResult = provider.mapCheckedCall(nodejsCallRequest(urlSearchParamsAppendCall, urlSearchParamsAppendSignature), fakeContext(facts));
  const urlSearchParamsSizeResult = provider.mapCheckedPropertyAccess(nodejsPropertyRequest(urlSearchParamsSizeExpression, urlSearchParamsSizeDeclaration), fakeContext(facts));

  assertSelectedMember(readFileResult, "Tsonic.CSharp.Node.fs.readFileSync(System.String,System.String)");
  assertSelectedMember(readdirSyncResult, "Tsonic.CSharp.Node.fs.readdirSync(System.String)");
  assertSelectedMember(pathParseResult, "Tsonic.CSharp.Node.path.parse(System.String)");
  assertSelectedMember(fsPromisesReadResult, "Tsonic.CSharp.Node.fs_promises.readFile(System.String,System.String)");
  assertSelectedMember(fsPromisesReadBytesResult, "Tsonic.CSharp.Node.fs_promises.readFile(System.String)");
  assert.equal(fsPromisesReadResult.value.selectedSignature.member.returnType.csharpTaskResultType.id, "System.String");
  assert.equal(fsPromisesReadBytesResult.value.selectedSignature.member.returnType.csharpTaskResultType.id, "Tsonic.CSharp.Node.Buffer");
  assertSelectedMember(fsPromisesReaddirResult, "Tsonic.CSharp.Node.fs_promises.readdir(System.String)");
  assert.equal(parsedBaseResult.kind, "accept");
  assert.equal(parsedBaseResult.value.operation.operationId, "Tsonic.CSharp.Node.ParsedPath.@base");
  assert.equal(facts.get(parsedBaseExpression, csharpTargetOperationFactKey)?.operationId, "Tsonic.CSharp.Node.ParsedPath.@base");
  assertSelectedMember(processCwdResult, "Tsonic.CSharp.Node.process.cwd()");
  assertSelectedMember(bufferCompareResult, "Tsonic.CSharp.Node.Buffer.compare(Tsonic.CSharp.Node.Buffer,Tsonic.CSharp.Node.Buffer)");
  assert.equal(bufferCompareResult.value.selectedSignature.member.static, true);
  assertSelectedMember(bufferIncludesResult, "Tsonic.CSharp.Node.Buffer.includes(System.Object,System.Int32,System.String)");
  assertSelectedMember(bufferReadUInt8Result, "Tsonic.CSharp.Node.Buffer.readUInt8(System.Int32)");
  assertSelectedMember(bufferWriteUInt8Result, "Tsonic.CSharp.Node.Buffer.writeUInt8(System.Byte,System.Int32)");
  assertSelectedMember(bufferIsBufferResult, "Tsonic.CSharp.Node.Buffer.isBuffer(System.Object)");
  assert.equal(bufferPoolSizeResult.kind, "accept");
  assert.equal(bufferPoolSizeResult.value.operation.operationId, "Tsonic.CSharp.Node.Buffer.poolSize");
  assert.equal(facts.get(bufferPoolSizeExpression, csharpTargetOperationFactKey)?.operationId, "Tsonic.CSharp.Node.Buffer.poolSize");
  assertSelectedMember(bufferTranscodeResult, "Tsonic.CSharp.Node.buffer.transcode(Tsonic.CSharp.Node.Buffer,System.String,System.String)");
  assertSelectedMember(cryptoHmacResult, "Tsonic.CSharp.Node.crypto.createHmac(System.String,Tsonic.CSharp.Node.Buffer)");
  assertSelectedMember(osHomedirResult, "Tsonic.CSharp.Node.os.homedir()");
  assertSelectedMember(processHrtimeResult, "Tsonic.CSharp.Node.process.hrtime(System.Double[])");
  assertSelectedMember(processMemoryResult, "Tsonic.CSharp.Node.process.memoryUsage()");
  assert.equal(processMemoryResult.value.selectedSignature.member.returnType.id, "Tsonic.CSharp.Node.MemoryUsage");
  assert.equal(processMemoryRssResult.kind, "accept");
  assert.equal(processMemoryRssResult.value.operation.operationId, "Tsonic.CSharp.Node.MemoryUsage.rss");
  assert.equal(facts.get(processMemoryRssExpression, csharpTargetOperationFactKey)?.operationId, "Tsonic.CSharp.Node.MemoryUsage.rss");
  assertSelectedMember(processUptimeResult, "Tsonic.CSharp.Node.process.uptime()");
  assertSelectedMember(utilResult, "Tsonic.CSharp.Node.util.toUSVString(System.String)");
  assertSelectedMember(utilStyleTextResult, "Tsonic.CSharp.Node.util.styleText(System.String,System.String)");
  assertSelectedMember(defaultFsExistsResult, "Tsonic.CSharp.Node.fs.existsSync(System.String)");
  assert.equal(defaultProcessPlatformResult.kind, "accept");
  assert.equal(defaultProcessPlatformResult.value.operation.operationId, "Tsonic.CSharp.Node.process.platform");
  assert.equal(facts.get(defaultProcessPlatformExpression, csharpTargetOperationFactKey)?.operationId, "Tsonic.CSharp.Node.process.platform");
  assertUnsupportedCall(provider, facts, defaultUtilFormatSignature, "unsupported:Tsonic.CSharp.Node.util.format(System.Object,System.Object[])");
  assertSelectedMember(urlCanParseResult, "Tsonic.CSharp.Node.URL.canParse(System.String,Tsonic.CSharp.Node.URL)");
  assertSelectedMember(urlSearchParamsAppendResult, "Tsonic.CSharp.Node.URLSearchParams.append(System.String,System.String)");
  assert.equal(urlSearchParamsSizeResult.kind, "accept");
  assert.equal(urlSearchParamsSizeResult.value.operation.operationId, "Tsonic.CSharp.Node.URLSearchParams.size");
  assert.equal(facts.get(urlSearchParamsSizeExpression, csharpTargetOperationFactKey)?.operationId, "Tsonic.CSharp.Node.URLSearchParams.size");
});
test("NodeJS provider package hard-rejects selected unsupported provider identities", () => {
  const facts = new TestFactStore();
  const provider = createCsharpNodejsProviderPackageOperationsProvider();
  const cryptoCipherSignature = {};
  const processStdinDeclaration = {};
  const utilFormatSignature = {};
  const urlPatternTestSignature = {};
  const processNextTickDeclaration = {};
  const processNextTickSignature = {};
  const staleReaddirSignature = {};
  const staleReaddirCall = {};
  facts.set(cryptoCipherSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("crypto", "createCipheriv", "node:crypto.createCipheriv(System.String,System.Object,System.Object)"));
  facts.set(processStdinDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("process", "stdin"));
  facts.set(utilFormatSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:util", "format", "node:util.format(System.Object,System.Object[])"));
  facts.set(urlPatternTestSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:url", "URLPattern", "test", "node:url.URLPattern.test", "node:url.URLPattern.test(System.String)"));
  facts.set(processNextTickDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:process", "NodeProcessModule", "nextTick", "node:process.NodeProcessModule.nextTick"));
  facts.set(processNextTickSignature, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration("node:process", "NodeProcessModule", "nextTick", "node:process.NodeProcessModule.nextTick", "node:process.nextTick(Function,System.Object[])"));
  facts.set(staleReaddirSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration("node:fs", "readdirSync", "node:fs.readdirSync(System.String,System.Boolean)"));

  for (const unsupported of nodeFsUnsupportedTargetIdentities()) {
    const selectedSignature = {};
    facts.set(selectedSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration(
      nodeFsModuleSpecifier,
      unsupported.exportName,
      unsupported.signatureId,
    ));
    assertUnsupportedCall(provider, facts, selectedSignature, unsupported.targetIdentityId);
  }
  for (const unsupported of nodeCryptoUnsupportedTargetIdentities()) {
    const selectedSignature = {};
    facts.set(selectedSignature, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration(
      nodeCryptoModuleSpecifier,
      unsupported.exportName,
      unsupported.signatureId,
    ));
    assertUnsupportedCall(provider, facts, selectedSignature, unsupported.targetIdentityId);
  }
  for (const unsupported of nodeOsUnsupportedTargetIdentities()) {
    const selectedDeclaration = {};
    facts.set(selectedDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualDeclaration(
      nodeOsModuleSpecifier,
      unsupported.exportName,
      unsupported.signatureId,
    ));
    if (unsupported.signatureId === undefined) {
      assertUnsupportedProperty(provider, facts, selectedDeclaration, unsupported.targetIdentityId);
    } else {
      assertUnsupportedCall(provider, facts, selectedDeclaration, unsupported.targetIdentityId);
    }
  }
  assertUnsupportedCall(provider, facts, cryptoCipherSignature, "unsupported:Tsonic.CSharp.Node.crypto.createCipheriv(System.String,System.Object,System.Object)");
  assertUnsupportedProperty(provider, facts, processStdinDeclaration, "unsupported:Tsonic.CSharp.Node.process.stdin");
  assertUnsupportedCall(provider, facts, utilFormatSignature, "unsupported:Tsonic.CSharp.Node.util.format(System.Object,System.Object[])");
  assertUnsupportedCall(provider, facts, urlPatternTestSignature, "unsupported:Tsonic.CSharp.Node.URLPattern.test(System.String)");
  const processNextTickCalleeResult = provider.mapCheckedPropertyAccess(nodejsPropertyRequest({}, processNextTickDeclaration), fakeContext(facts));
  assert.equal(processNextTickCalleeResult.kind, "accept");
  assert.equal(processNextTickCalleeResult.value.operation.operationKind, "method");
  assertUnsupportedCall(provider, facts, processNextTickSignature, "unsupported:Tsonic.CSharp.Node.process.nextTick(Function,System.Object[])");
  const staleReaddirResult = provider.mapCheckedCall(nodejsCallRequest(staleReaddirCall, staleReaddirSignature), fakeContext(facts));
  assert.equal(staleReaddirResult.kind, "reject");
  assert.equal(staleReaddirResult.diagnostic.extensionCode, "CSHARP_NODEJS_CALL_NOT_MAPPED");
  assert.equal(facts.get(staleReaddirCall, csharpTargetOperationFactKey), undefined);
});
test("NodeJS provider package requires selected signatures before target member selection", () => {
  const call = {};
  const selectedDeclaration = {};
  const facts = new TestFactStore();
  const provider = createCsharpNodejsProviderPackageOperationsProvider();
  facts.set(selectedDeclaration, providerVirtualDeclarationFactKey, nodejsVirtualMemberDeclaration(
    "node:buffer",
    "Buffer",
    "compare",
    "node:buffer.Buffer.compare",
  ));

  const result = provider.mapCheckedCall(nodejsCallRequestWithoutSignature(call, selectedDeclaration), fakeContext(facts));

  assert.equal(result.kind, "reject");
  assert.equal(result.diagnostic.extensionCode, "CSHARP_NODEJS_CALL_REQUIRES_SELECTED_SIGNATURE");
  assert.equal(result.diagnostic.evidence?.[0]?.details?.requiredFacts[1], "selected NodeJS provider signature identity");
  assert.equal(facts.get(call, csharpTargetOperationFactKey), undefined);
});
test("selected NodeJS Buffer source type-checks compare provider declarations", () => {
  const session = createCsharpSession(`
    import { Buffer, transcode } from "buffer";

    export function compareBuffers(text: string): number {
      const left = Buffer.from(text, "utf8");
      const right = Buffer.from("expected", "utf8");
      left.writeUInt8(72, 0);
      return Buffer.compare(left, right) + left.indexOf("e") + left.readUInt8(0) + (left.includes("x") ? 1 : 0) + transcode(left, "utf8", "utf8").length + (Buffer.isBuffer(left) ? Buffer.poolSize : 0);
    }
  `, { selectedSurfaces: [{ id: "js" }], selectedCapabilities: [{ id: "@tsonic/csharp-nodejs" }] });
  const sourceFile = session.getSourceFile("/src/index.ts");
  assert.equal(formatDiagnostics(session.ensureChecked(sourceFile)), "");
});
test("selected NodeJS fs promises source type-checks and maps through provider-package declarations", () => {
  const session = createCsharpSession(`
    import { Buffer } from "node:buffer";
    import { appendFile, chmod, cp, readFile, readlink, realpath, rmdir, stat, symlink, writeFile } from "node:fs/promises";

    export function load(path: string): Promise<string> {
      return readFile(path, "utf8");
    }

    export function loadBytes(path: string): Promise<Buffer> {
      return readFile(path);
    }

    export async function saveAndSize(path: string): Promise<number> {
      await writeFile(path, "hello", "utf8");
      const stats = await stat(path);
      return stats.size;
    }

    export async function saveBytes(path: string): Promise<void> {
      await writeFile(path, Buffer.from("a", "utf8"));
      await appendFile(path, Buffer.from("b", "utf8"));
    }

    export async function prepareLink(target: string, path: string): Promise<string> {
      await symlink(target, path);
      await chmod(path, 420);
      await cp(target, path + ".copy", true);
      await rmdir(path + ".dir", true);
      return (await readlink(path)) + (await realpath(path));
    }
  `, { selectedSurfaces: [{ id: "js" }], selectedCapabilities: [{ id: "@tsonic/csharp-nodejs" }] });
  const sourceFile = session.getSourceFile("/src/index.ts");
  assert.equal(formatDiagnostics(session.ensureChecked(sourceFile)), "");

  const extensionHost = session.finalizeExtensions();
  const selectedMemberIds = collectFactValues(sourceFile, session, extensionHost, selectedTargetSignatureFactKey)
    .map((fact) => fact.member.id);

  assert.equal(extensionHost.diagnostics.all().map((diagnostic) => diagnostic.extensionCode).join("\n"), "");
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.readFile(System.String,System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.readFile(System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.writeFile(System.String,System.String,System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.writeFile(System.String,Tsonic.CSharp.Node.Buffer)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.appendFile(System.String,Tsonic.CSharp.Node.Buffer)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.stat(System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.symlink(System.String,System.String,System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.chmod(System.String,System.Int32)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.cp(System.String,System.String,System.Boolean)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.rmdir(System.String,System.Boolean)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.readlink(System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.realpath(System.String)"));
});
test("selected NodeJS URLSearchParams source type-checks through closed provider declarations", () => {
  const session = createCsharpSession(`
    import { URL, URLSearchParams } from "node:url";

    export function query(input: string): string {
      const params = new URLSearchParams("a=1");
      params.append("b", input);
      params.set("a", "2");
      return (params.get("a") ?? "") + params.getAll("a")[0] + params.toString();
    }

    export function querySize(): number {
      return new URLSearchParams("a=1").size;
    }

    export function liveQuery(input: string): string {
      const url = new URL("https://example.com/?q=" + input);
      url.searchParams.append("page", "1");
      return url.searchParams.get("page") ?? "";
    }
  `, { selectedSurfaces: [{ id: "js" }], selectedCapabilities: [{ id: "@tsonic/csharp-nodejs" }] });
  const sourceFile = session.getSourceFile("/src/index.ts");
  assert.equal(formatDiagnostics(session.ensureChecked(sourceFile)), "");
});
test("selected NodeJS default module imports type-check through provider-package declarations", () => {
  const session = createCsharpSession(`
    import { Buffer } from "node:buffer";
    import fs from "node:fs";
    import fsPromises from "node:fs/promises";
    import path from "node:path";
    import process from "node:process";
    import util from "node:util";
    import url from "node:url";

    export function defaultModulePath(input: string): string {
      return path.join(process.cwd(), util.toUSVString(input));
    }

    export function defaultModuleExists(input: string): boolean {
      return fs.existsSync(input);
    }

    export function defaultModuleReadText(input: string): Promise<string> {
      return fsPromises.readFile(input, "utf8");
    }

    export function defaultModuleReadBytes(input: string): Promise<Buffer> {
      return fsPromises.readFile(input);
    }

    export function defaultModuleFileUrl(input: string): string {
      return url.pathToFileURL(input).href;
    }
  `, { selectedSurfaces: [{ id: "js" }], selectedCapabilities: [{ id: "@tsonic/csharp-nodejs" }] });
  const sourceFile = session.getSourceFile("/src/index.ts");
  assert.equal(formatDiagnostics(session.ensureChecked(sourceFile)), "");

  const extensionHost = session.finalizeExtensions();
  const selectedMemberIds = collectFactValues(sourceFile, session, extensionHost, selectedTargetSignatureFactKey)
    .map((fact) => fact.member.id);

  assert.equal(extensionHost.diagnostics.all().map((diagnostic) => diagnostic.extensionCode).join("\n"), "");
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs.existsSync(System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.readFile(System.String,System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.fs_promises.readFile(System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.path.join(System.String[])"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.process.cwd()"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.util.toUSVString(System.String)"));
  assert.ok(selectedMemberIds.includes("Tsonic.CSharp.Node.url.pathToFileURL(System.String)"));
});
test("selected NodeJS process nullish unions finalize provider-owned target operation facts", () => {
  const session = createCsharpSession(`
    import process from "node:process";

    export function pathOrEmpty(): string {
      return process.env["PATH"] ?? "";
    }

    export function exitOrZero(): number {
      return process.exitCode ?? 0;
    }
  `, { selectedSurfaces: [{ id: "js" }], selectedCapabilities: [{ id: "@tsonic/csharp-nodejs" }] });
  const sourceFile = session.getSourceFile("/src/index.ts");
  assert.equal(formatDiagnostics(session.ensureChecked(sourceFile)), "");

  const extensionHost = session.finalizeExtensions();
  const operationFacts = collectFactValues(sourceFile, session, extensionHost, csharpTargetOperationFactKey);
  const processEnvOperation = operationFacts.find((fact) => fact.operationId === "Tsonic.CSharp.Node.process.env");
  const envIndexerOperation = operationFacts.find((fact) => fact.operationId === "Tsonic.CSharp.Node.ProcessEnv.Item(System.String)");
  const exitCodeOperation = operationFacts.find((fact) => fact.operationId === "Tsonic.CSharp.Node.process.exitCode");

  assert.equal(extensionHost.diagnostics.all().map((diagnostic) => diagnostic.extensionCode).join("\n"), "");
  assert.equal(processEnvOperation?.resultType?.id, "Tsonic.CSharp.Node.ProcessEnv");
  assert.equal(envIndexerOperation?.resultType?.kind, "target-named");
  assert.equal(envIndexerOperation?.resultType?.id, "System.String");
  assert.equal(envIndexerOperation?.resultType?.csharpNullableReference, true);
  assert.equal(exitCodeOperation?.resultType?.kind, "target-named");
  assert.equal(exitCodeOperation?.resultType?.id, "System.Nullable`1");
  assert.deepEqual(exitCodeOperation?.resultType?.typeArguments?.[0], { kind: "source-primitive", name: "int32" });
});
