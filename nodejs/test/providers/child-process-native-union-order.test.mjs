import assert from "node:assert/strict";
import test from "node:test";
import { nodeChildProcessExports, nodeChildProcessOptionsTarget } from "../../../dist/provider/modules/child-process.js";

for (const js of [false, true]) {
  test(`${js ? "js" : "native"} child-process options retain their fixed runtime union roles without a Source program`, () => {
    const options = nodeChildProcessOptionsTarget(js);
    const declarations = nodeChildProcessExports(js);
    assert.equal(declarations.some(declaration => declaration.name === "SpawnSyncOptionsWithBufferEncoding"), true,
      "the actual provider declarations initialize before a Source program exists");
    if (js) {
      assert.equal(options.input.csharpRuntimeUnionSemantics, "native-ordered");
      assert.equal(options.input.csharpRuntimeUnionArms.length, 2);
      assert.equal(options.input.csharpRuntimeUnionArms[0].id, "Tsonic.CSharp.Js.Uint8Array");
      assert.equal(options.input.csharpRuntimeUnionArms[1].id, "Tsonic.CSharp.Node.Buffer");
    } else {
      assert.equal(options.input.id, "Tsonic.CSharp.Node.Buffer");
      assert.equal(options.input.csharpRuntimeUnionArms === undefined, true,
        "native input keeps the original Buffer carrier rather than acquiring a JS union");
    }
    const descriptor = js ? options.stdio.typeArguments[0] : options.stdio.element;
    assert.equal(descriptor.csharpSpecialType, "nullable",
      "the descriptor has one outer native absence state");
    assert.equal(descriptor.typeArguments.length, 1);
    const present = descriptor.typeArguments[0];
    assert.equal(present !== undefined, true, "nullable storage retains its exact present union");
    assert.equal(present.csharpRuntimeUnionSemantics, "native-ordered");
    assert.equal(present.csharpRuntimeUnionArms.length, 2);
    assert.equal(present.csharpRuntimeUnionArms[0].kind, "source-primitive");
    assert.equal(present.csharpRuntimeUnionArms[0].name, "float64");
    assert.equal(present.csharpRuntimeUnionArms[1].id, "System.String");
    assert.equal(present.csharpProjection === undefined, true,
      "fixed runtime role order needs neither a Source-sum projection nor a program descriptor");
    assert.equal(options.type.typeArguments[1], options.stdio,
      "the actual generic options ABI uses this same descriptor storage");
  });
}
