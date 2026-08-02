import assert from "node:assert/strict";
import test from "node:test";
import {
  createCsharpProviderRelationCatalog,
} from "../../tsonic-csharp/dist/provider/target-relations/index.js";
import {
  compileCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import {
  createTsonicPlugin,
} from "../dist/index.js";
import {
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";
import {
  nodejsCanonicalProviderExports,
} from "../dist/provider/provider.js";

test("Node provider relations form one contradiction-free exact catalog", () => {
  const relations = nodejsProviderTargetRelations();
  const catalog = createCsharpProviderRelationCatalog([relations]);

  assert.equal(relations.length, 894);
  assert.equal(catalog.relations.length, relations.length);
});

test("Node default imports are provider-owned static containers", () => {
  const declarations = nodejsCanonicalProviderExports("node:path");
  const defaultDeclarations = declarations.filter(
    (declaration) => declaration.exportKind === "default",
  );

  assert.equal(defaultDeclarations.length, 1);
  const declaration = defaultDeclarations[0];
  assert.equal(declaration.kind, "class");
  assert.equal(declaration.id, "node:path.default");
  assert.equal(declaration.name, "NodePathModule");
  assert.ok(declaration.members.length > 0);
  assert.ok(declaration.members.every((member) => member.static === true));
  assert.equal(
    declarations.some((candidate) => candidate.kind === "interface" &&
      candidate.name === "NodePathModule"),
    false,
  );
});

test("Node class staticness comes from the exact provider declaration", () => {
  const relations = nodejsProviderTargetRelations().filter(
    (relation) => relation.kind === "signature" &&
      relation.source.signatureId ===
        "node:buffer.Buffer.from(System.String,System.String)",
  );

  assert.equal(relations.length, 2);
  for (const relation of relations) {
    assert.equal(relation.source.memberStatic, true);
    assert.equal(relation.targetMember.static, true);
    assert.deepEqual(relation.receiver, { kind: "none" });
  }
});

test("named, namespace, default, property, and class-static Node operations compile", () => {
  const compiled = compileCsharpSource({
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import path, { join } from "node:path";
      import * as pathNamespace from "node:path";
      import { Buffer } from "node:buffer";

      export function named(left: string, right: string): string {
        return join(left, right);
      }

      export function namespaced(left: string, right: string): string {
        return pathNamespace.join(left, right);
      }

      export function defaulted(left: string, right: string): string {
        return path.join(left, right) + path.sep;
      }

      export function buffered(value: string): Buffer {
        return Buffer.from(value, "utf8");
      }
    `,
  });

  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.extensionDiagnostics, []);
  assert.deepEqual(compiled.result.diagnostics, []);
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /return Tsonic\.CSharp\.Node\.path\.join\(left, right\);/u,
  );
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /return Tsonic\.CSharp\.Node\.path\.join\(left, right\) \+ Tsonic\.CSharp\.Node\.path\.sep;/u,
  );
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /return Tsonic\.CSharp\.Node\.Buffer\.from\(value, "utf8"\);/u,
  );
});
