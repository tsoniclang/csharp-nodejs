import assert from "node:assert/strict";
import test from "node:test";
import {
  nodejsProviderPackageOwnedModuleSpecifiers,
} from "../dist/provider/module-specifiers.js";
import {
  nodejsCanonicalProviderExports,
} from "../dist/provider/provider.js";

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
