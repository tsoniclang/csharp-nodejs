import { test } from "node:test";
import assert from "node:assert/strict";
import { getNodejsProviderSignature, nodejsVirtualMemberDeclaration } from "./node-surface-completion.helpers.mjs";
import { createCsharpNodejsProviderPackageBindingProvider } from "../dist/provider/index.js";

function providerModel(moduleSpecifier) {
  const bindingProvider = createCsharpNodejsProviderPackageBindingProvider();
  const resolution = bindingProvider.resolveModule(moduleSpecifier, {});
  assert.equal(resolution.kind, "virtual");
  return bindingProvider.getDeclarationModel(resolution);
}

function findCallableMember(moduleSpecifier) {
  const model = providerModel(moduleSpecifier);
  for (const exported of model.exports) {
    for (const member of exported.members ?? []) {
      const signature = member.signatures?.[0];
      if (signature !== undefined) {
        return { exported, member, signature };
      }
    }
  }
  return undefined;
}

test("provider test records resolve through exact member and signature identity", () => {
  const found = findCallableMember("node:fs");
  assert.ok(found, "expected at least one callable provider member");
  const declaration = nodejsVirtualMemberDeclaration(
    "node:fs",
    found.exported.name,
    found.member.name,
    found.member.id,
    found.signature.id,
  );
  assert.equal(getNodejsProviderSignature(declaration)?.id, found.signature.id);
});

test("provider test records cannot select a same-spelling sibling by name", () => {
  const found = findCallableMember("node:fs");
  assert.ok(found);
  // Exact spelling, wrong identity: the member name still matches the real
  // member, but the member id names something that does not exist. Name-based
  // reconstruction would resolve this; exact identity must not.
  const impostor = nodejsVirtualMemberDeclaration(
    "node:fs",
    found.exported.name,
    found.member.name,
    `${found.member.id}$notAMember`,
    found.signature.id,
  );
  assert.equal(getNodejsProviderSignature(impostor), undefined);
});

test("provider test records reject contradictory memberStatic evidence", () => {
  const found = findCallableMember("node:fs");
  assert.ok(found);
  const declaration = {
    ...nodejsVirtualMemberDeclaration(
      "node:fs",
      found.exported.name,
      found.member.name,
      found.member.id,
      found.signature.id,
    ),
    memberStatic: found.member.static !== true,
  };
  assert.equal(getNodejsProviderSignature(declaration), undefined);
});

test("provider test records reject a contradictory member key", () => {
  const found = findCallableMember("node:fs");
  assert.ok(found);
  const declaration = {
    ...nodejsVirtualMemberDeclaration(
      "node:fs",
      found.exported.name,
      found.member.name,
      found.member.id,
      found.signature.id,
    ),
    memberKey: { kind: "property-key", name: `${found.member.name}$other` },
  };
  assert.equal(getNodejsProviderSignature(declaration), undefined);
});
