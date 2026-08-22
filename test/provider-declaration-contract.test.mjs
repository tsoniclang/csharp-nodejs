import assert from "node:assert/strict";
import test from "node:test";
import {
  nodejsProviderPackageOwnedModuleSpecifiers,
} from "../dist/provider/module-specifiers.js";
import {
  nodejsCanonicalProviderExports,
} from "../dist/provider/provider.js";
import {
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";

test("every Node provider signature has legal parameter omission order", () => {
  const violations = [];

  for (const moduleSpecifier of nodejsProviderPackageOwnedModuleSpecifiers()) {
    for (const declaration of nodejsCanonicalProviderExports(moduleSpecifier) ?? []) {
      const declarationPath = `${moduleSpecifier}:${declaration.name}`;
      visitSignatures(declaration.signatures, declarationPath, violations);
      for (const member of declaration.members ?? []) {
        const memberPath = `${declarationPath}.${member.name}`;
        visitSignatures(member.signatures, memberPath, violations);
        visitType(member.type, `${memberPath}.type`, violations, new WeakSet());
      }
      visitType(declaration.type, `${declarationPath}.type`, violations, new WeakSet());
    }
  }

  assert.deepEqual(violations, []);
});

test("Node callback APIs model omitted middle options as exact overloads", () => {
  const declarations = nodejsCanonicalProviderExports("node:fs") ?? [];

  assert.deepEqual(
    signatureIds(declarations, "readFile"),
    [
      "node:fs.readFile(System.String,Function)",
      "node:fs.readFile(System.String,System.Object,Function)",
    ],
  );
  assert.deepEqual(
    signatureIds(declarations, "writeFile"),
    [
      "node:fs.writeFile(System.String,System.Object,Function)",
      "node:fs.writeFile(System.String,System.Object,System.Object,Function)",
    ],
  );
  assert.deepEqual(
    signatureIds(declarations, "watch"),
    [
      "node:fs.watch(System.String,Function)",
      "node:fs.watch(System.String,System.Object,Function)",
    ],
  );
});

test("Tsumo portability APIs expose closed provider declarations", () => {
  const childProcess = nodejsCanonicalProviderExports("node:child_process") ?? [];
  const spawnSync = childProcess.find((entry) =>
    entry.name === "spawnSync" && entry.exportKind !== "default"
  );
  assert.deepEqual(spawnSync?.signatures, [{
    id: "node:child_process.spawnSync(System.String,System.String[])",
    parameters: [
      { name: "command", type: { kind: "string" } },
      { name: "args", type: { kind: "array", elementType: { kind: "string" } } },
    ],
    returnType: {
      kind: "provider-ref",
      moduleSpecifier: "node:child_process",
      exportName: "SpawnSyncReturns",
      typeArguments: [{
        kind: "provider-ref",
        moduleSpecifier: "node:buffer",
        exportName: "Buffer",
      }],
    },
  }]);
  const spawnSyncReturns = childProcess.find((entry) =>
    entry.name === "SpawnSyncReturns" && entry.exportKind !== "default"
  );
  assert.deepEqual(spawnSyncReturns?.typeParameters, [{ name: "T" }]);
  assert.deepEqual(
    spawnSyncReturns?.members?.map((member) => [member.name, member.type]),
    [
      ["stdout", { kind: "type-parameter", name: "T" }],
      ["stderr", { kind: "type-parameter", name: "T" }],
      ["status", {
        kind: "union",
        types: [{ kind: "number" }, { kind: "literal", value: null }],
      }],
    ],
  );
  const spawnRelations = nodejsProviderTargetRelations().filter((relation) =>
    relation.kind === "signature" &&
    relation.source.signatureId ===
      "node:child_process.spawnSync(System.String,System.String[])"
  );
  assert.equal(spawnRelations.length, 4);
  for (const moduleSpecifier of ["child_process", "node:child_process"]) {
    const targetArgumentTypes = spawnRelations
      .filter((relation) => relation.source.moduleSpecifier === moduleSpecifier)
      .map((relation) => relation.targetMember.parameters[1].type)
      .sort((left, right) => left.kind.localeCompare(right.kind));
    assert.equal(targetArgumentTypes.length, 2);
    assert.equal(targetArgumentTypes[0].kind, "array");
    assert.equal(targetArgumentTypes[1].kind, "target-named");
    assert.equal(targetArgumentTypes[1].id, "Tsonic.CSharp.Js.JSArray`1");
  }

  const fs = nodejsCanonicalProviderExports("node:fs") ?? [];
  assert.deepEqual(signatureIds(fs, "lstatSync"), ["node:fs.lstatSync(System.String)"]);
  const stats = fs.find((entry) => entry.name === "Stats");
  assert.ok(stats?.members.some((member) =>
    member.id === "node:fs.Stats.isSymbolicLink" &&
    member.signatures?.[0]?.id === "node:fs.Stats.isSymbolicLink()"
  ));

  const util = nodejsCanonicalProviderExports("node:util") ?? [];
  const textDecoder = util.find((entry) => entry.name === "TextDecoder");
  const textDecoderConstructor = textDecoder?.members?.find((member) => member.name === "constructor");
  assert.deepEqual(
    textDecoderConstructor?.signatures?.map((signature) => signature.id),
    ["node:util.TextDecoder.constructor()"],
  );
  assert.ok(textDecoder?.members.some((member) => member.id === "node:util.TextDecoder.decode"));

  const url = nodejsCanonicalProviderExports("node:url") ?? [];
  const urlObject = url.find((entry) => entry.name === "UrlObject");
  assert.deepEqual(
    urlObject?.members.map((member) => member.name),
    [
      "href",
      "protocol",
      "auth",
      "host",
      "hostname",
      "port",
      "pathname",
      "search",
      "query",
      "hash",
      "slashes",
    ],
  );
  assert.ok(urlObject?.members.every((member) => member.optional === true));
  for (const member of urlObject?.members ?? []) {
    assert.equal(member.readonly, undefined);
    assert.deepEqual(
      member.type,
      member.name === "slashes"
        ? {
          kind: "union",
          types: [
            { kind: "boolean" },
            { kind: "literal", value: null },
            { kind: "undefined" },
          ],
        }
        : {
          kind: "union",
          types: [
            { kind: "string" },
            { kind: "literal", value: null },
            { kind: "undefined" },
          ],
        },
    );
  }
  const legacyUrl = url.find((entry) => entry.name === "Url");
  assert.equal(legacyUrl?.heritage, undefined);
  assert.deepEqual(legacyUrl?.members.map((member) => member.name), [
    "href",
    "protocol",
    "auth",
    "host",
    "hostname",
    "port",
    "pathname",
    "search",
    "query",
    "hash",
    "path",
    "slashes",
  ]);
  assert.deepEqual(
    legacyUrl?.members.find((member) => member.name === "href")?.type,
    { kind: "string" },
  );
  assert.deepEqual(
    legacyUrl?.members.find((member) => member.name === "pathname")?.type,
    {
      kind: "union",
      types: [{ kind: "string" }, { kind: "literal", value: null }],
    },
  );
  const stringQueryUrl = url.find((entry) => entry.name === "UrlWithStringQuery");
  assert.deepEqual(stringQueryUrl?.heritage, [{ kind: "extends", type: {
    kind: "provider-ref",
    moduleSpecifier: "node:url",
    exportName: "Url",
  } }]);
  assert.deepEqual(stringQueryUrl?.members.map((member) => member.name), ["query"]);
});

function signatureIds(declarations, exportName) {
  const declaration = declarations.find((candidate) =>
    candidate.name === exportName && candidate.exportKind !== "default"
  );
  assert.ok(declaration, `missing provider export ${exportName}`);
  return declaration.signatures.map((signature) => signature.id);
}

function visitSignatures(signatures, path, violations) {
  for (const [index, signature] of (signatures ?? []).entries()) {
    const signaturePath = `${path}.signature[${index}]`;
    let optionalSeen = false;
    for (const parameter of signature.parameters) {
      if (optionalSeen && parameter.optional !== true && parameter.rest !== true) {
        violations.push(`${signaturePath}: required ${parameter.name} follows an optional parameter`);
      }
      optionalSeen ||= parameter.optional === true || parameter.rest === true;
      visitType(parameter.type, `${signaturePath}.${parameter.name}`, violations, new WeakSet());
    }
    visitType(signature.returnType, `${signaturePath}.return`, violations, new WeakSet());
  }
}

function visitType(type, path, violations, seen) {
  if (type === undefined || typeof type !== "object" || seen.has(type)) return;
  seen.add(type);
  if (type.kind === "function") visitSignatures([type], path, violations);
  for (const nested of type.types ?? type.typeArguments ?? type.elementTypes ?? []) {
    visitType(nested, path, violations, seen);
  }
  visitType(type.elementType, path, violations, seen);
  visitType(type.sourceShape, path, violations, seen);
}
