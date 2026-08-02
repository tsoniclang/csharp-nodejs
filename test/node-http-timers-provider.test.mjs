import {
  assert,
  collectFactValues,
  createCsharpNodejsProviderPackageBindingProvider,
  createCsharpNodejsTargetContributions,
  createCsharpSession,
  csharpTargetOperationFactKey,
  formatDiagnostics,
  selectedTargetSignatureFactKey,
  test,
} from "./node-surface-completion.helpers.mjs";
import {
  csharpTargetBindingsContributionKind,
} from "@tsonic/target-csharp";
import {
  nodejsProviderPackageOwnedModuleSpecifiers,
} from "../dist/provider/module-specifiers.js";

test("every Node provider target identity has one exact C# render binding", () => {
  const provider = createCsharpNodejsProviderPackageBindingProvider();
  const targetIdentities = new Map();
  for (const moduleSpecifier of nodejsProviderPackageOwnedModuleSpecifiers().filter((specifier) => specifier.startsWith("node:"))) {
    const model = declarationModel(provider, moduleSpecifier);
    for (const declaration of model.exports) {
      if (declaration.targetIdentity === undefined) {
        continue;
      }
      const existing = targetIdentities.get(declaration.targetIdentity.id);
      const identity = {
        id: declaration.targetIdentity.id,
        sourceName: declaration.name,
        kind: declaration.kind,
      };
      existing === undefined
        ? targetIdentities.set(identity.id, identity)
        : assert.deepEqual(identity, existing);
    }
  }

  const contribution = createCsharpNodejsTargetContributions({})
    .find((candidate) => candidate.kind === csharpTargetBindingsContributionKind);
  assert.ok(contribution);
  const bindings = new Map(contribution.bindings.map((binding) => [binding.id, binding]));
  assert.deepEqual([...bindings.keys()].sort(), [...targetIdentities.keys()].sort());
  for (const identity of targetIdentities.values()) {
    const binding = bindings.get(identity.id);
    assert.ok(binding, `missing C# target binding ${identity.id}`);
    assert.equal(binding.sourceName, identity.sourceName);
    assert.equal(binding.kind, identity.kind);
    assert.equal(binding.csharpType.kind, "target-named");
    assert.equal(binding.csharpType.id, identity.id);
    assert.equal(binding.csharpType.csharpRender.kind, "named");
  }
});

test("Node HTTP and timer modules expose exact provider-owned declarations", () => {
  const provider = createCsharpNodejsProviderPackageBindingProvider();
  const httpModel = declarationModel(provider, "node:http");
  const timersModel = declarationModel(provider, "node:timers");

  assert.deepEqual(
    httpModel.exports.filter((entry) => entry.exportKind !== "default").map((entry) => entry.name),
    ["IncomingMessage", "ServerResponse", "Server", "createServer", "NodeHttpModule"],
  );
  assert.deepEqual(
    timersModel.exports.filter((entry) => entry.exportKind !== "default").map((entry) => entry.name),
    ["Timeout", "setTimeout", "setInterval", "NodeTimersModule"],
  );

  assert.equal(
    classMember(httpModel, "IncomingMessage", "readAll").signatures[0].id,
    "node:http.IncomingMessage.readAll()",
  );
  assert.deepEqual(
    classMember(httpModel, "ServerResponse", "end").signatures.map((signature) => signature.id),
    ["node:http.ServerResponse.end()", "node:http.ServerResponse.end(System.String)"],
  );
  assert.equal(
    classMember(httpModel, "IncomingMessage", "method").type.kind,
    "union",
  );
  assert.equal(
    exportedFunction(timersModel, "setInterval").signatures[0].id,
    "node:timers.setInterval(Function,System.Int32)",
  );
});

test("Node HTTP and timer source operations select exact provider target facts", () => {
  const session = createCsharpSession(`
    import * as http from "node:http";
    import type { IncomingMessage, ServerResponse } from "node:http";
    import * as timers from "node:timers";

    export function start(): void {
      const port: number = 8765;
      const server = http.createServer((request: IncomingMessage, response: ServerResponse) => {
        const method = request.method ?? "GET";
        const path = request.url ?? "/";
        response.statusCode = method === "GET" ? 200 : 405;
        response.setHeader("Content-Type", "text/plain");
        response.writeHead(200, "OK");
        response.end(path);
      });
      server.listen(port, () => {});
      timers.setInterval(() => {}, 60000);
    }

    export async function read(request: IncomingMessage): Promise<string> {
      return await request.readAll();
    }
  `, {
    selectedSurfaces: [{ id: "js" }],
    selectedCapabilities: [{ id: "@tsonic/csharp-nodejs" }],
  });
  const sourceFile = session.getSourceFile("/src/index.ts");

  assert.equal(formatDiagnostics(session.ensureChecked(sourceFile)), "");
  const extensionHost = session.finalizeExtensions();
  assert.equal(extensionHost.diagnostics.all().map((diagnostic) => diagnostic.extensionCode).join("\n"), "");

  const selectedIds = collectFactValues(sourceFile, session, extensionHost, selectedTargetSignatureFactKey)
    .map((fact) => fact.member.id);
  const operationIds = collectFactValues(sourceFile, session, extensionHost, csharpTargetOperationFactKey)
    .map((fact) => fact.operationId);

  for (const expected of [
    "Tsonic.CSharp.Node.Http.http.createServer(System.Action`2)",
    "Tsonic.CSharp.Node.Http.IncomingMessage.readAll()",
    "Tsonic.CSharp.Node.Http.ServerResponse.setHeader(System.String,System.String)",
    "Tsonic.CSharp.Node.Http.ServerResponse.writeHead(System.Int32,System.String)",
    "Tsonic.CSharp.Node.Http.ServerResponse.end(System.String)",
    "Tsonic.CSharp.Node.Http.Server.listen(System.Double,System.Action)",
    "Tsonic.CSharp.Node.timers.setInterval(System.Action,System.Int32)",
  ]) {
    assert.ok(selectedIds.includes(expected), `missing selected target signature ${expected}`);
  }
  assert.ok(operationIds.includes("Tsonic.CSharp.Node.Http.IncomingMessage.method"));
  assert.ok(operationIds.includes("Tsonic.CSharp.Node.Http.IncomingMessage.url"));
  assert.ok(operationIds.includes("Tsonic.CSharp.Node.Http.ServerResponse.statusCode"));
});

test("Node HTTP modules remain unavailable without the installed capability", () => {
  const session = createCsharpSession(`
    import { createServer } from "node:http";
    export const server = createServer();
  `, { selectedSurfaces: [{ id: "js" }] });
  const sourceFile = session.getSourceFile("/src/index.ts");
  const diagnostics = formatDiagnostics(session.ensureChecked(sourceFile));

  assert.match(diagnostics, /Cannot find (?:module|name) 'node:http'/);
});

function declarationModel(provider, moduleSpecifier) {
  const resolution = provider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = provider.getDeclarationModel(resolution);
  assert.equal(model.moduleSpecifier, moduleSpecifier);
  return model;
}

function classMember(model, exportName, memberName) {
  const declaration = model.exports.find((entry) => entry.name === exportName);
  assert.equal(declaration?.kind, "class");
  const member = declaration.members.find((entry) => entry.name === memberName);
  assert.ok(member, `missing ${exportName}.${memberName}`);
  return member;
}

function exportedFunction(model, exportName) {
  const declaration = model.exports.find((entry) => entry.name === exportName);
  assert.equal(declaration?.kind, "function");
  return declaration;
}
