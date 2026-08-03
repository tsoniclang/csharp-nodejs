import assert from "node:assert/strict";
import test from "node:test";

import {
  compileCsharpSource,
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

test("Node fs Stats Date declarations use the selected source global", () => {
  const provider = createCsharpNodejsProviderPackageBindingProvider();
  const resolution = provider.resolveModule("node:fs", {});
  assert.equal(resolution.kind, "virtual");
  const model = provider.getDeclarationModel(resolution);
  const stats = model.exports.find((declaration) =>
    declaration.name === "Stats"
  );
  const mtime = stats?.members.find((member) => member.name === "mtime");
  const mtimeMs = stats?.members.find((member) => member.name === "mtimeMs");

  assert.deepEqual(mtime?.type, { kind: "source-global", name: "Date" });
  assert.deepEqual(mtimeMs?.type, { kind: "number" });

  const relation = nodejsProviderTargetRelations().filter((candidate) =>
    candidate.kind === "member" &&
    candidate.source.moduleSpecifier === "node:fs" &&
    candidate.source.memberId === "node:fs.Stats.mtime"
  );
  assert.equal(relation.length, 1);
  assert.equal(
    relation[0].targetMember.returnType.id,
    "Tsonic.CSharp.Js.Date",
  );
});

test("Node Stats Date facts compose with JS Date and nullish operations", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    capabilities: [createTsonicPlugin()],
    sourceText: `
      import { statSync } from "node:fs";

      export function stamp(
        path: string,
        fallback: Date | undefined,
      ): string {
        const stats = statSync(path);
        const selected = fallback ?? stats.mtime;
        return selected.toISOString() + ":" + stats.mtimeMs;
      }
    `,
  });

  assert.equal(compiled.sourceDiagnosticsText, "");
  assert.deepEqual(compiled.extensionDiagnostics, []);
  assert.deepEqual(compiled.result.diagnostics, []);
  const source = compiled.artifacts.get("src/Index.cs");
  assert.match(
    source,
    /Tsonic\.CSharp\.Js\.Date selected = fallback \?\? stats\.mtime;/u,
  );
  assert.match(source, /selected\.toISOString\(\)/u);
});

test("Node Stats members are unavailable without the installed capability", () => {
  const compiled = compileCsharpSource({
    surface: "js",
    sourceText: `
      import { statSync } from "node:fs";
      export function stamp(path: string): string {
        return statSync(path).mtime.toISOString();
      }
    `,
  });

  assert.match(
    compiled.sourceDiagnosticsText,
    /Cannot find (?:module|name) 'node:fs'/u,
  );
  assert.equal(compiled.artifacts.size, 0);
});
