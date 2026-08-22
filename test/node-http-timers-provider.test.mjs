import assert from "node:assert/strict";
import test from "node:test";

import {
  assertCsharpCompilationSucceeded,
  compileCsharpSource,
  checkCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import {
  createTsonicPlugin,
} from "../dist/index.js";
import {
  createCsharpNodejsProviderPackageBindingProvider,
} from "../dist/provider/provider.js";
import {
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";

test("every canonical Node provider target type has one exact C# render binding", () => {
  const relations = nodejsProviderTargetRelations().filter((relation) =>
    relation.kind === "type" &&
    relation.source.moduleSpecifier === relation.source.providerModuleId
  );

  assert.equal(relations.length, 19);
  assert.equal(
    new Set(relations.map((relation) =>
      `${relation.source.providerModuleId}:${relation.source.exportName}`
    )).size,
    relations.length,
  );
  for (const relation of relations) {
    assert.equal(relation.targetBinding.csharpType.kind, "target-named");
    assert.equal(
      relation.targetBinding.csharpType.id,
      relation.targetBinding.id,
    );
    assert.equal(
      relation.targetBinding.csharpType.csharpRender?.kind,
      "named",
    );
  }
  const legacyUrlBindings = relations
    .filter((relation) =>
      relation.targetBinding.id === "Tsonic.CSharp.Node.LegacyUrlObject"
    )
    .map((relation) => JSON.stringify(relation.targetBinding));
  assert.equal(legacyUrlBindings.length, 3);
  assert.equal(new Set(legacyUrlBindings).size, 1);
});

test("Node HTTP and timer modules expose exact provider-owned declarations", () => {
  const provider = createCsharpNodejsProviderPackageBindingProvider();
  const httpModel = declarationModel(provider, "node:http");
  const timersModel = declarationModel(provider, "node:timers");

  assert.deepEqual(
    httpModel.exports
      .filter((entry) => entry.exportKind !== "default")
      .map((entry) => entry.name),
    [
      "IncomingMessage",
      "ServerResponse",
      "Server",
      "createServer",
    ],
  );
  assert.deepEqual(
    timersModel.exports
      .filter((entry) => entry.exportKind !== "default")
      .map((entry) => entry.name),
    ["Timeout", "setTimeout", "setInterval"],
  );
  assert.equal(
    httpModel.exports.find((entry) => entry.exportKind === "default")?.name,
    "NodeHttpModule",
  );
  assert.equal(
    timersModel.exports.find((entry) => entry.exportKind === "default")?.name,
    "NodeTimersModule",
  );
  assert.equal(
    classMember(httpModel, "IncomingMessage", "readAll").signatures[0].id,
    "node:http.IncomingMessage.readAll()",
  );
  assert.deepEqual(
    classMember(httpModel, "ServerResponse", "end").signatures.map(
      (signature) => signature.id,
    ),
    [
      "node:http.ServerResponse.end()",
      "node:http.ServerResponse.end(System.String)",
      "node:http.ServerResponse.end(Tsonic.CSharp.Node.Buffer)",
    ],
  );
  assert.equal(
    exportedFunction(timersModel, "setInterval").signatures[0].id,
    "node:timers.setInterval(Function,System.Int32)",
  );
});

test("Node HTTP and timer source operations compile through exact provider relations", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import * as http from "node:http";
      import type { IncomingMessage, ServerResponse } from "node:http";
      import * as timers from "node:timers";

      export function start(port: number): void {
        const server = http.createServer(
          (request: IncomingMessage, response: ServerResponse) => {
            response.statusCode = 200;
            response.setHeader("Content-Type", "text/plain");
            response.end(request.url ?? "/");
          },
        );
        server.listen(port, () => {});
        timers.setInterval(() => {}, 60000);
      }

      export async function read(
        request: IncomingMessage,
      ): Promise<string> {
        return await request.readAll();
      }
    `,
  });

  assertCsharpCompilationSucceeded(compiled);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(
    source,
    /Tsonic\.CSharp\.Node\.Http\.http\.createServer/u,
  );
  assert.match(source, /server\.listen\(port/u);
  assert.match(
    source,
    /Tsonic\.CSharp\.Node\.timers\.setInterval/u,
  );
  assert.match(source, /return await request\.readAll\(\);/u);
});

test("Node HTTP modules remain unavailable without the installed capability", () => {
  const checked = checkCsharpSource({
    surface: "js",
    sourceText: `
      import { createServer } from "node:http";
      export const server = createServer();
    `,
  });

  assert.match(
    checked.sourceDiagnosticsText,
    /Cannot find (?:module|name) 'node:http'/u,
  );
  assert.deepEqual(checked.extensionDiagnostics, []);
});

function declarationModel(provider, moduleSpecifier) {
  const resolution = provider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  const model = provider.getDeclarationModel(resolution, {
    context: {},
    materialization: { kind: "complete" },
  });
  assert.equal(model.moduleSpecifier, moduleSpecifier);
  return model;
}

function classMember(model, exportName, memberName) {
  const declaration = model.exports.find((entry) =>
    entry.name === exportName
  );
  assert.equal(declaration?.kind, "class");
  const member = declaration.members.find((entry) =>
    entry.name === memberName
  );
  assert.ok(member, `missing ${exportName}.${memberName}`);
  return member;
}

function exportedFunction(model, exportName) {
  const declaration = model.exports.find((entry) =>
    entry.name === exportName
  );
  assert.equal(declaration?.kind, "function");
  return declaration;
}
