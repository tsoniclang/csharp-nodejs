import assert from "node:assert/strict";
import test from "node:test";

import {
  assertCsharpCompilationSucceeded,
  compileCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import {
  createTsonicPlugin,
} from "../dist/index.js";
import {
  nodejsSourceProvider,
} from "./helpers/provider-package.mjs";
import {
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";

test("ServerResponse.end declares the exact Buffer overload", () => {
  const provider = nodejsSourceProvider(["js"]);
  const model = declarationModel(provider, "node:http");
  const end = classMember(model, "ServerResponse", "end");
  assert.deepEqual(
    end.signatures.map((signature) => signature.id),
    [
      "node:http.ServerResponse.end()",
      "node:http.ServerResponse.end(System.String)",
      "node:http.ServerResponse.end(Tsonic.CSharp.Node.Buffer)",
    ],
  );
  const bufferSignature = end.signatures[2];
  assert.deepEqual(bufferSignature.parameters, [{
    name: "chunk",
    type: {
      kind: "provider-ref",
      moduleSpecifier: "node:buffer",
      exportName: "Buffer",
    },
  }]);
  assert.deepEqual(
    model.imports,
    [
      {
        moduleSpecifier: "node:buffer",
        namedImports: [{ exportedName: "Buffer", kind: "type" }],
        typeOnly: true,
      },
      {
        moduleSpecifier: "node:stream",
        namedImports: [{ exportedName: "Writable", kind: "value" }],
        typeOnly: false,
      },
    ],
  );
});

test("Server.listen declares the exact hostname overload", () => {
  const provider = nodejsSourceProvider(["js"]);
  const model = declarationModel(provider, "node:http");
  const listen = classMember(model, "Server", "listen");
  assert.deepEqual(
    listen.signatures.map((signature) => signature.id),
    [
      "node:http.Server.listen(System.Double,System.Action)",
      "node:http.Server.listen(System.Double,System.String,System.Action)",
    ],
  );
  const hostnameSignature = listen.signatures[1];
  assert.deepEqual(
    hostnameSignature.parameters.map((parameter) => [parameter.name, parameter.type.kind, parameter.optional === true]),
    [
      ["port", "number", false],
      ["hostname", "string", false],
      ["callback", "function", true],
    ],
  );
});

test("only the default node:process object exitCode property is writable", () => {
  const provider = nodejsSourceProvider(["js"]);
  const model = declarationModel(provider, "node:process");
  const defaultObject = model.exports.find((entry) => entry.exportKind === "default");
  assert.equal(defaultObject?.name, "NodeProcessModule");
  const exitCode = defaultObject.members.find((member) => member.name === "exitCode");
  assert.ok(exitCode, "missing default exitCode member");
  assert.equal(exitCode.readonly, undefined);
  assert.equal(exitCode.static, true);
  const argv = defaultObject.members.find((member) => member.name === "argv");
  assert.equal(argv?.readonly, true);
  const namedExitCode = model.exports.find((entry) =>
    entry.exportKind !== "default" && entry.name === "exitCode"
  );
  assert.equal(namedExitCode?.kind, "value");
});

test("the new operations map through exact selected relations", () => {
  const relations = nodejsProviderTargetRelations();
  const endBuffer = relations.filter((relation) =>
    relation.kind === "signature" &&
    relation.source.signatureId === "node:http.ServerResponse.end(Tsonic.CSharp.Node.Buffer)"
  );
  assert.equal(endBuffer.length, 2);
  for (const relation of endBuffer) {
    assert.equal(
      relation.targetMember.id,
      "Tsonic.CSharp.Node.Http.ServerResponse.end(Tsonic.CSharp.Node.Buffer)",
    );
    assert.deepEqual(relation.receiver, { kind: "instance" });
  }
  const listenHostname = relations.filter((relation) =>
    relation.kind === "signature" &&
    relation.source.signatureId === "node:http.Server.listen(System.Double,System.String,System.Action)"
  );
  assert.equal(listenHostname.length, 2);
  for (const relation of listenHostname) {
    assert.equal(
      relation.targetMember.id,
      "Tsonic.CSharp.Node.Http.Server.listen(System.Double,System.String,System.Action)",
    );
  }
  const defaultExitCode = relations.filter((relation) =>
    relation.kind === "member" &&
    relation.source.memberId === "node:process.default.exitCode"
  );
  assert.equal(defaultExitCode.length, 2);
  for (const relation of defaultExitCode) {
    assert.equal(relation.targetMember.id, "Tsonic.CSharp.Node.process.exitCode");
    assert.equal(relation.targetMember.static, true);
  }
});

test("binary response, hostname binding, and exit code compile to exact C#", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import process from "node:process";
      import { readFileSync } from "node:fs";
      import { createServer } from "node:http";
      import type { IncomingMessage, ServerResponse } from "node:http";

      export function serve(filePath: string, port: number, host: string, callback: () => void): void {
        const server = createServer(
          (request: IncomingMessage, response: ServerResponse) => {
            response.end(readFileSync(filePath));
          },
        );
        server.listen(port, host, callback);
        process.exitCode = 2;
      }
      export function commandArguments(): string[] {
        return process.argv.slice(2);
      }
    `,
  });

  assertCsharpCompilationSucceeded(compiled);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(
    source,
    /response\.end\(Tsonic\.CSharp\.Node\.fs\.readFileSync\(filePath\)\)/u,
  );
  assert.match(source, /server\.listen\(port, host, callback\);/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.process\.exitCode = 2/u);
  assert.match(
    source,
    /Tsonic\.CSharp\.Js\.Array\.slice\(Tsonic\.CSharp\.Node\.process\.argv, 2\)/u,
  );
});

test("named process imports and writes to readonly default members stay rejected", () => {
  const namedImport = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import { process } from "node:process";
      export const code = process;
    `,
  });
  assert.match(
    namedImport.sourceDiagnosticsText,
    /TS2614: Module '"node:process"' has no exported member 'process'\. Did you mean to use 'import process from "node:process"' instead\?/u,
  );

  const readonlyWrite = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import process from "node:process";
      process.argv = [];
    `,
  });
  assert.match(
    readonlyWrite.sourceDiagnosticsText,
    /Cannot assign to 'argv' because it is a read-only property/u,
  );
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
    entry.exportKind !== "default" && entry.name === exportName
  );
  assert.equal(declaration?.kind, "class");
  const member = declaration.members.find((entry) => entry.name === memberName);
  assert.ok(member, `missing ${exportName}.${memberName}`);
  return member;
}
