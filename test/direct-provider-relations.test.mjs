import assert from "node:assert/strict";
import test from "node:test";
import {
  assertCsharpProviderPolicyIsNonContradictory,
  createCsharpProviderRejectionCatalog,
  createCsharpProviderRelationCatalog,
} from "../../tsonic-csharp/dist/public/provider.js";
import {
  getCsharpNullableElementTargetType,
} from "../../tsonic-csharp/dist/public/provider.js";
import {
  assertCsharpCompilationSucceeded,
  checkCsharpSource,
  compileCsharpSource,
} from "../../tsonic-csharp/test/helpers/direct-csharp-session.mjs";
import {
  createTsonicPlugin,
} from "../dist/index.js";
import {
  nodejsProviderTargetRejections,
  nodejsProviderTargetRelations,
} from "../dist/provider/target-relations.js";
import {
  nodejsCanonicalProviderExports,
} from "../dist/provider/modules/catalog.js";

test("Node provider relations form one contradiction-free exact catalog", () => {
  const relations = nodejsProviderTargetRelations();
  const rejections = nodejsProviderTargetRejections();
  const relationCatalog = createCsharpProviderRelationCatalog([relations]);
  const rejectionCatalog = createCsharpProviderRejectionCatalog([rejections]);

  assert.equal(relations.length, 1748);
  assert.equal(rejections.length, 166);
  assert.equal(relationCatalog.relations.length, relations.length);
  assert.equal(rejectionCatalog.rejections.length, rejections.length);
  assert.doesNotThrow(() =>
    assertCsharpProviderPolicyIsNonContradictory(
      relationCatalog,
      rejectionCatalog,
    ));

  const readFile = rejections.filter((rejection) =>
    rejection.source.kind === "signature" &&
    rejection.source.providerModuleId === "node:fs" &&
    rejection.source.moduleSpecifier === "node:fs" &&
    rejection.source.exportName === "readFile" &&
    rejection.source.signatureId ===
      "node:fs.readFile(System.String,System.Object,Function)"
  );
  assert.equal(readFile.length, 1);
  assert.equal(
    readFile[0].diagnostic.extensionCode,
    "CSHARP_NODEJS_PROVIDER_PACKAGE_OPERATION_UNSUPPORTED",
  );
});

test("mkdtempSync has exact named and default relations for both public fs specifiers", () => {
  const targetMemberId =
    "Tsonic.CSharp.Node.fs.mkdtempSync(System.String)";
  const relations = nodejsProviderTargetRelations()
    .filter((relation) =>
      relation.kind === "signature" &&
      relation.source.signatureId === "node:fs.mkdtempSync(System.String)"
    )
    .map((relation) => ({
      moduleSpecifier: relation.source.moduleSpecifier,
      exportName: relation.source.exportName,
      memberId: relation.source.memberId ?? null,
      memberStatic: relation.source.memberStatic ?? null,
      targetMemberId: relation.targetMember.id,
    }))
    .sort((left, right) =>
      `${left.moduleSpecifier}|${left.exportName}`.localeCompare(
        `${right.moduleSpecifier}|${right.exportName}`,
      )
    );

  assert.deepEqual(relations, [
    {
      moduleSpecifier: "fs",
      exportName: "default",
      memberId: "node:fs.default.mkdtempSync",
      memberStatic: true,
      targetMemberId,
    },
    {
      moduleSpecifier: "fs",
      exportName: "mkdtempSync",
      memberId: null,
      memberStatic: null,
      targetMemberId,
    },
    {
      moduleSpecifier: "node:fs",
      exportName: "default",
      memberId: "node:fs.default.mkdtempSync",
      memberStatic: true,
      targetMemberId,
    },
    {
      moduleSpecifier: "node:fs",
      exportName: "mkdtempSync",
      memberId: null,
      memberStatic: null,
      targetMemberId,
    },
  ]);
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
  assert.deepEqual(
    declaration.members.map((member) => ({
      id: member.id,
      kind: member.kind,
      name: member.name,
      static: member.static,
    })),
    [
      ["basename", "method"],
      ["dirname", "method"],
      ["extname", "method"],
      ["isAbsolute", "method"],
      ["join", "method"],
      ["matchesGlob", "method"],
      ["normalize", "method"],
      ["parse", "method"],
      ["relative", "method"],
      ["resolve", "method"],
      ["toNamespacedPath", "method"],
      ["format", "method"],
      ["sep", "property"],
      ["delimiter", "property"],
      ["posix", "property"],
      ["win32", "property"],
    ].map(([name, kind]) => ({
      id: `node:path.default.${name}`,
      kind,
      name,
      static: true,
    })),
  );
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
    targetOptions: { outputType: "Exe" },
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

  assertCsharpCompilationSucceeded(compiled);
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

test("standard filesystem option objects lower through exact provider construction relations", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import { mkdirSync, rmSync, watch } from "node:fs";

      export function recreate(
        path: string,
        mode: number,
        maxRetries: number,
        retryDelay: number,
      ): void {
        mkdirSync(path, { recursive: true, mode });
        rmSync(path, { recursive: true, force: true, maxRetries, retryDelay });
        const watcher = watch(
          path,
          { persistent: false, recursive: true },
          (_eventType, _filename) => {},
        );
        watcher.close();
      }
    `,
  });

  assertCsharpCompilationSucceeded(compiled);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(
    source,
    /new Tsonic\.CSharp\.Node\.MakeDirectoryOptions\s*\{[\s\S]*recursive = true[\s\S]*mode = mode[\s\S]*\}/u,
  );
  assert.match(
    source,
    /new Tsonic\.CSharp\.Node\.RmOptions\s*\{[\s\S]*recursive = true[\s\S]*force = true[\s\S]*maxRetries = maxRetries[\s\S]*retryDelay = retryDelay[\s\S]*\}/u,
  );
  assert.match(
    source,
    /new Tsonic\.CSharp\.Node\.WatchOptions\s*\{[\s\S]*persistent = false[\s\S]*recursive = true[\s\S]*\}/u,
  );
});

test("provider-private boolean filesystem overloads are absent", () => {
  const checked = checkCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import { mkdirSync, rmSync } from "node:fs";

      export function invalid(path: string): void {
        mkdirSync(path, true);
        rmSync(path, true);
      }
    `,
  });

  assert.equal(
    [...checked.sourceDiagnosticsText.matchAll(/TS2559/gu)].length,
    2,
  );
  assert.deepEqual(checked.extensionDiagnostics, []);
});

test("filesystem links, child processes, legacy URLs, and text decoding use exact provider relations", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import { Buffer } from "node:buffer";
      import { spawnSync } from "node:child_process";
      import { lstatSync } from "node:fs";
      import { TextDecoder } from "node:util";
      import { format, parse } from "node:url";

      export function isLink(path: string): boolean {
        return lstatSync(path).isSymbolicLink();
      }

      export function run(command: string, args: string[]): number | null {
        return spawnSync(command, args).status;
      }

      export function clearStatus(command: string, args: string[]): number | null {
        const result = spawnSync(command, args);
        result.status = null;
        return result.status;
      }

      export function pathname(value: string): string | null {
        return parse(value).pathname;
      }

      export function pathnameOrEmpty(value: string): string {
        return parse(value).pathname ?? "";
      }

      export function roundTrip(value: string): string {
        return format(parse(value));
      }

      export function rewrite(value: string): string {
        const parsed = parse(value);
        parsed.href = value;
        parsed.pathname = "/checked";
        parsed.query = null;
        return format(parsed);
      }

      export function hasAuthority(value: string): boolean | null {
        return parse(value).slashes;
      }

      export function query(value: string): string | null {
        return parse(value).query;
      }

      export function decode(value: string): string {
        return new TextDecoder().decode(Buffer.from(value, "utf8"));
      }
    `,
  });

  assertCsharpCompilationSucceeded(compiled);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(source, /Tsonic\.CSharp\.Node\.fs\.lstatSync\(path\)\.IsSymbolicLink\(\)/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.child_process\.spawnSyncResult\(command, args\)\.status/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.url\.parse\(value\)\.pathname/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.url\.parse\(value\)\.pathname \?\? ""/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.url\.format\(Tsonic\.CSharp\.Node\.url\.parse\(value\)\)/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.url\.parse\(value\)\.slashes/u);
  assert.match(source, /Tsonic\.CSharp\.Node\.url\.parse\(value\)\.queryText/u);
  assert.match(source, /new Tsonic\.CSharp\.Node\.TextDecoder\(\)\.decode/u);
});

test("legacy URL declarations preserve nullable selected source results", () => {
  const checked = checkCsharpSource({
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import { parse } from "node:url";

      export function invalid(value: string): string {
        return parse(value).pathname;
      }
    `,
  });

  assert.match(checked.sourceDiagnosticsText, /TS2322/u);
  assert.match(checked.sourceDiagnosticsText, /string \| null/u);
  assert.deepEqual(checked.extensionDiagnostics, []);
});

test("combined portability provider selection is independent of source ordering", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import { format, parse } from "node:url";
      import { TextDecoder } from "node:util";
      import { lstatSync } from "node:fs";
      import { spawnSync } from "node:child_process";
      import { Buffer } from "node:buffer";

      export function decode(value: string): string {
        return new TextDecoder().decode(Buffer.from(value, "utf8"));
      }

      export function roundTrip(value: string): string {
        return format(parse(value));
      }

      export function run(command: string, args: string[]): number | null {
        return spawnSync(command, args).status;
      }

      export function isLink(path: string): boolean {
        return lstatSync(path).isSymbolicLink();
      }
    `,
  });

  assertCsharpCompilationSucceeded(compiled);
  assert.deepEqual([...compiled.artifacts.keys()].sort(), [
    "TsonicGenerated.csproj",
    "generated/TsonicEntrypoint.cs",
    "src/Index.cs",
  ]);
});

test("Node numeric API parameters preserve the source number carrier", () => {
  const compiled = compileCsharpSource({
    capabilities: [createTsonicPlugin()],
    targetOptions: { outputType: "Exe" },
    sourceText: `
      import * as http from "node:http";

      export function start(port: number): void {
        const server = http.createServer((_request, _response) => {});
        server.listen(port, () => {});
      }
    `,
  });

  assertCsharpCompilationSucceeded(compiled);
  assert.match(
    compiled.artifacts.get("src/Index.cs"),
    /server\.listen\(port, \(\) =>/u,
  );

  const relations = nodejsProviderTargetRelations().filter(
    (relation) => relation.kind === "signature" &&
      relation.source.signatureId ===
        "node:http.Server.listen(System.Double,System.Action)",
  );
  assert.deepEqual(
    relations.map((relation) => ({
      moduleSpecifier: relation.source.moduleSpecifier,
      targetId: relation.targetMember.id,
      parameterType: relation.targetMember.parameters[0]?.type,
    })),
    ["http", "node:http"].map((moduleSpecifier) => ({
      moduleSpecifier,
      targetId: "Tsonic.CSharp.Node.Http.Server.listen(System.Double,System.Action)",
      parameterType: { kind: "source-primitive", name: "float64" },
    })),
  );
});

test("Node provider relations declare every source-number target adapter exactly", () => {
  const expectedTargetNames = new Map([
    ["bool", "ToBoolean"],
    ["int8", "ToSByte"],
    ["uint8", "ToByte"],
    ["int16", "ToInt16"],
    ["uint16", "ToUInt16"],
    ["int32", "ToInt32"],
    ["native-int", "ToInt32"],
    ["uint32", "ToUInt32"],
    ["native-uint", "ToUInt32"],
    ["int64", "ToInt64"],
    ["uint64", "ToUInt64"],
    ["float16", "ToSingle"],
    ["float32", "ToSingle"],
    ["decimal", "ToDecimal"],
  ]);
  let adapterCount = 0;

  for (const relation of nodejsProviderTargetRelations()) {
    if (relation.kind !== "signature") continue;
    const sourceSignature = findSourceSignature(relation);
    for (const parameter of relation.parameters) {
      const sourceParameter = sourceSignature.parameters[
        parameter.sourceParameterIndex
      ];
      const targetParameter = relation.targetMember.parameters[
        parameter.targetParameterIndex
      ];
      assert.ok(sourceParameter);
      assert.ok(targetParameter);
      const resultType = getCsharpNullableElementTargetType(
        targetParameter.type,
      ) ?? targetParameter.type;
      const expectedTargetName = sourceParameter.type.kind === "number" &&
          resultType.kind === "source-primitive"
        ? expectedTargetNames.get(resultType.name)
        : undefined;
      const identity = `${relation.source.moduleSpecifier}:${relation.source.signatureId}:parameter[${parameter.sourceParameterIndex}]`;

      assert.equal(
        parameter.argumentAdapter?.targetName,
        expectedTargetName,
        identity,
      );
      if (expectedTargetName === undefined) continue;
      adapterCount += 1;
      assert.deepEqual(parameter.argumentAdapter, {
        kind: "static-method",
        id: `System.Convert.${expectedTargetName}(System.Double)`,
        declaringType: {
          kind: "target-named",
          id: "System.Convert",
          csharpRender: {
            kind: "named",
            namespace: ["System"],
            name: "Convert",
          },
        },
        targetName: expectedTargetName,
        inputType: { kind: "source-primitive", name: "float64" },
        resultType,
      }, identity);
    }
  }

  assert.equal(adapterCount, 256);
});

function findSourceSignature(relation) {
  const declarations = nodejsCanonicalProviderExports(
    relation.source.providerModuleId,
  ) ?? [];
  const declaration = declarations.find((candidate) =>
    candidate.id === relation.source.exportId
  );
  assert.ok(declaration, `missing source export ${relation.source.exportId}`);
  const signatures = relation.source.memberId === undefined
    ? declaration.signatures
    : declaration.members?.find((member) =>
        member.id === relation.source.memberId
      )?.signatures;
  const signature = signatures?.find((candidate) =>
    candidate.id === relation.source.signatureId
  );
  assert.ok(signature, `missing source signature ${relation.source.signatureId}`);
  return signature;
}
