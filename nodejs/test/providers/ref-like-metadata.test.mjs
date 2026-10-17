import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { existsSync, mkdirSync, mkdtempSync, readdirSync, writeFileSync } from "node:fs";
import { basename, dirname, join } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import { csharpDelegateTargetType, csharpSourcePrimitiveTargetType } from "../../../../tsonic-csharp/dist/public/provider.js";
import { buildDotnetFixture } from "../../../../tsonic-csharp/test/helpers/dotnet-fixtures.mjs";
import { nodejsProviderTargetRelations } from "../../../dist/provider/target-relations.js";
import { nodeBufferObjectTargetType, nodeBufferToStringEndTargetType } from "../../../dist/provider/modules/buffer/target-types.js";
import { nodeChildProcessOptionsTarget } from "../../../dist/provider/modules/child-process.js";
import { borrowedHeaderValuesTargetType, headerValuesStorageType } from "../../../dist/provider/modules/http/types.js";
import { nodeStreamClassCallTargetMembers } from "../../../dist/provider/modules/stream.js";

test("Node provider generic-slot facts match its actual native assemblies", { timeout: 300_000 }, () => {
  const root = fileURLToPath(new URL("../../../", import.meta.url));
  const nativeProject = join(root, "csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj");
  const target = spawnSync("dotnet", ["msbuild", nativeProject, "-getProperty:TargetPath", "-nologo"],
    { encoding: "utf8", timeout: 60_000 });
  assert.equal(target.status, 0, `the native build owns its selected output: ${target.stdout}\n${target.stderr}`);
  const assembly = target.stdout.trim();
  assert.equal(existsSync(assembly), true, "the actual Node native build supplies the oracle input");
  const carriers = [nodeBufferObjectTargetType, nodeBufferToStringEndTargetType(),
    headerValuesStorageType, borrowedHeaderValuesTargetType,
    nodeChildProcessOptionsTarget(false).type, nodeChildProcessOptionsTarget(true).type,
    csharpDelegateTargetType("System.Action", [csharpSourcePrimitiveTargetType("int32")]),
    csharpDelegateTargetType("System.Func", [], csharpSourcePrimitiveTargetType("int32"))];
  for (const includeJsSurfaceMembers of [false, true]) {
    for (const relation of nodejsProviderTargetRelations(includeJsSurfaceMembers)) {
      const carrier = relation.kind === "type" ? relation.targetBinding.csharpType : relation.targetMember.declaringType;
      assert.equal(carrier?.kind, "target-named", "every native relation keeps its actual declaring carrier");
      carriers.push(carrier);
    }
  }
  const pipe = nodeStreamClassCallTargetMembers().find(member =>
    member.exportName === "Readable" && member.memberName === "pipe");
  assert.equal(pipe !== undefined, true, "the canonical native pipe metadata exists");
  const scratch = join(root, ".temp");
  mkdirSync(scratch, { recursive: true });
  const directory = mkdtempSync(join(scratch, "native-provider-metadata-"));
  const request = join(directory, "request.json");
  writeFileSync(request, JSON.stringify({
    names: [...new Set(carriers.map(carrier => carrier.id))], methodName: pipe.member.targetName,
  }));
  const references = readdirSync(dirname(assembly)).filter(name => name.endsWith(".dll")).map(name =>
    `<Reference Include="${xml(basename(name, ".dll"))}"><HintPath>${xml(join(dirname(assembly), name))}</HintPath></Reference>`
  ).join("\n");
  const project = join(directory, "ClrNodeMetadata.csproj");
  writeFileSync(project, `<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable>
<PublishAot>false</PublishAot><EnableTrimAnalyzer>false</EnableTrimAnalyzer>
</PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App" />
${references}</ItemGroup></Project>`);
  writeFileSync(join(directory, "Program.cs"), `using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Primitives;
using Tsonic.CSharp.Node;

using var request = JsonDocument.Parse(File.ReadAllText(args[0]));
var assemblies = new[] { typeof(object).Assembly, typeof(Readable).Assembly, typeof(StringValues).Assembly }.Distinct().ToArray();
var types = request.RootElement.GetProperty("names").EnumerateArray().Select(entry => {
    var name = entry.GetString()!;
    var type = assemblies.Select(assembly => assembly.GetType(name, false)).SingleOrDefault(candidate => candidate is not null)
        ?? throw new InvalidOperationException($"No actual native declaration for {name}.");
    return new { name, permissions = type.GetGenericArguments().Select(parameter =>
        (parameter.GenericParameterAttributes & GenericParameterAttributes.AllowByRefLike) != 0).ToArray() };
}).ToArray();
var method = typeof(Readable).GetMethods().Single(candidate => candidate.Name == request.RootElement.GetProperty("methodName").GetString());
var methodParameters = method.GetGenericArguments().Select(parameter => new {
    name = parameter.Name,
    allowsRefLike = (parameter.GenericParameterAttributes & GenericParameterAttributes.AllowByRefLike) != 0
}).ToArray();
Console.Write(JsonSerializer.Serialize(new { types, methodParameters }));
`);
  const executable = buildDotnetFixture({ project, projectDirectory: directory,
    outputDirectory: join(directory, "bin"), intermediateDirectory: join(directory, "obj/"),
    outputAssemblyName: "ClrNodeMetadata.dll" });
  const result = spawnSync("dotnet", [executable, request], { encoding: "utf8", timeout: 240_000 });
  assert.equal(result.status, 0, `actual native metadata oracle: ${result.stdout}\n${result.stderr}`);
  const native = JSON.parse(result.stdout);
  writeFileSync(join(directory, "oracle.json"), JSON.stringify(native, null, 2));
  const definitions = new Map(native.types.map(type => [type.name, type.permissions]));
  for (const carrier of carriers) {
    const permissions = definitions.get(carrier.id);
    assert.equal(permissions !== undefined, true, `${carrier.id}: actual native definition is present`);
    const selected = carrier.csharpAllowsRefLikeTypeArguments;
    assert.equal(Array.isArray(selected), true, `${carrier.id}: metadata is required`);
    assert.equal(selected.length, carrier.typeArguments?.length ?? 0, `${carrier.id}: selected slot arity`);
    assert.equal(selected.length, permissions.length, `${carrier.id}: actual native slot arity`);
    for (const [index, allowed] of permissions.entries()) {
      assert.equal(typeof selected[index], "boolean", `${carrier.id}[${index}]: exact Boolean metadata`);
      assert.equal(selected[index], allowed, `${carrier.id}[${index}]: actual native permission`);
    }
  }
  const slots = pipe.member.typeParameters;
  assert.equal(slots?.length, native.methodParameters.length, "pipe keeps every actual native method slot");
  for (const [index, parameter] of native.methodParameters.entries()) {
    assert.equal(slots[index].name, parameter.name, `pipe[${index}]: actual native slot identity`);
    assert.equal(typeof slots[index].csharpAllowsRefLike, "boolean", `pipe[${index}]: permission is required`);
    assert.equal(slots[index].csharpAllowsRefLike, parameter.allowsRefLike, `pipe[${index}]: actual native method permission`);
  }
  assert.equal(borrowedHeaderValuesTargetType.csharpAllowsRefLikeTypeArguments,
    headerValuesStorageType.csharpAllowsRefLikeTypeArguments, "borrowed HTTP facts retain the canonical native metadata");
});

function xml(value) {
  return value.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll('"', "&quot;");
}
