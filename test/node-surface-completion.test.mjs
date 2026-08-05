import assert from "node:assert/strict";
import test from "node:test";

import {
  compileCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import {
  createTsonicPlugin,
} from "../dist/index.js";
import {
  nodejsProviderPackageOwnedModuleSpecifiers,
} from "../dist/provider/module-specifiers.js";
import {
  nodejsCanonicalProviderExports,
} from "../dist/provider/provider.js";
import {
  nodejsProviderTargetRejections,
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";

test("every canonical Node source operation has exact target policy", () => {
  const sourceSignatures = new Set();
  const sourceProperties = new Set();
  const canonicalModules = nodejsProviderPackageOwnedModuleSpecifiers()
    .filter((specifier) => specifier.startsWith("node:"));

  for (const moduleSpecifier of canonicalModules) {
    for (
      const declaration of
        nodejsCanonicalProviderExports(moduleSpecifier) ?? []
    ) {
      for (const signature of declaration.signatures ?? []) {
        sourceSignatures.add(signature.id);
      }
      if (declaration.kind === "value") {
        sourceProperties.add(declaration.id);
      }
      for (const member of declaration.members ?? []) {
        for (const signature of member.signatures ?? []) {
          sourceSignatures.add(signature.id);
        }
        if (member.kind === "property") {
          sourceProperties.add(member.id);
        }
      }
    }
  }

  const policySignatures = new Set();
  const policyProperties = new Set();
  for (
    const policy of [
      ...nodejsProviderTargetRelations(),
      ...nodejsProviderTargetRejections(),
    ]
  ) {
    const source = policy.source;
    if (source.kind === "signature") {
      policySignatures.add(source.signatureId);
    } else if (source.kind === "member") {
      policyProperties.add(source.memberId);
    } else if (source.kind === "value") {
      policyProperties.add(source.exportId);
    }
  }

  assert.equal(sourceSignatures.size, 261);
  assert.equal(sourceProperties.size, 87);
  assert.deepEqual([...policySignatures].sort(), [...sourceSignatures].sort());
  assert.deepEqual([...policyProperties].sort(), [...sourceProperties].sort());
});

test("Node Promise declarations and targets preserve separate source and target shapes", () => {
  const declarations = nodejsCanonicalProviderExports("node:http") ?? [];
  const incoming = declarations.find((declaration) =>
    declaration.name === "IncomingMessage"
  );
  const readAll = incoming?.members
    ?.find((member) => member.name === "readAll")
    ?.signatures?.find((signature) =>
      signature.id === "node:http.IncomingMessage.readAll()"
    );
  assert.deepEqual(readAll?.returnType, {
    kind: "source-global",
    name: "Promise",
    typeArguments: [{ kind: "string" }],
  });

  const target = nodejsProviderTargetRelations().filter((relation) =>
    relation.kind === "signature" &&
    relation.source.moduleSpecifier === "node:http" &&
    relation.source.signatureId ===
      "node:http.IncomingMessage.readAll()"
  );
  assert.equal(target.length, 1);
  assert.equal(
    target[0].targetMember.returnType.id,
    "System.Threading.Tasks.Task`1",
  );
  assert.equal(
    target[0].targetMember.returnType.typeArguments[0].id,
    "System.String",
  );
});

test("Node process declarations expose exact undefined source semantics", () => {
  const declarations = nodejsCanonicalProviderExports("node:process") ?? [];
  const processEnv = declarations.find((declaration) =>
    declaration.name === "ProcessEnv"
  );
  const indexer = processEnv?.members?.find((member) =>
    member.kind === "indexer"
  );
  assert.deepEqual(indexer?.signatures?.[0]?.returnType, {
    kind: "union",
    types: [{ kind: "string" }, { kind: "undefined" }],
  });
});

test("Node provider families compile together through selected source evidence", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import fsPromises from "node:fs/promises";
      import { mkdtempSync } from "node:fs";
      import { Buffer } from "node:buffer";
      import process from "node:process";
      import { URLSearchParams } from "node:url";

      export async function run(
        path: string,
        key: string,
      ): Promise<string> {
        const bytes = Buffer.from("x", "utf8");
        const tempDir = mkdtempSync(path + "-");
        await fsPromises.writeFile(path, bytes);
        const text = await fsPromises.readFile(path, "utf8");
        const params = new URLSearchParams("a=1");
        params.append("b", "2");
        return (process.env[key] ?? text)
          + ":" + params.size + ":" + bytes.length + ":" + tempDir;
      }
    `,
  });

  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.extensionDiagnostics, []);
  assert.deepEqual(compiled.result.diagnostics, []);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(source, /Tsonic\.CSharp\.Node\.Buffer\.from/u);
  assert.match(
    source,
    /await Tsonic\.CSharp\.Node\.fs_promises\.readFile/u,
  );
  assert.match(
    source,
    /Tsonic\.CSharp\.Node\.process\.env\[key\] \?\? text/u,
  );
  assert.match(source, /Tsonic\.CSharp\.Node\.fs\.mkdtempSync/u);
  assert.match(source, /new Tsonic\.CSharp\.Node\.URLSearchParams/u);
});

test("unsupported selected Node operations fail closed without artifacts", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import { format } from "node:util";
      export function invalid(): string {
        return format({ value: 1 });
      }
    `,
  });

  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.extensionDiagnostics, []);
  assert.deepEqual(
    compiled.result.diagnostics.map((diagnostic) => diagnostic.code),
    ["TS9100203"],
  );
  assert.match(
    compiled.result.diagnostics[0].message,
    /hard-rejected selected call 'node:util' export 'format'/u,
  );
  assert.equal(compiled.artifacts.size, 0);
});
