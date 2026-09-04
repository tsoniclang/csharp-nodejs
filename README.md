# `@tsonic/csharp-nodejs`

C# Node capability for Tsonic. It owns exact `node:*` provider declarations,
C# operation relations, runtime references, and the `Tsonic.CSharp.Node`
runtime implementation.

Canonical product documentation:

- [Node capability](https://github.com/tsoniclang/tsonic/blob/main/docs/reference/node-capability.md)
- [C# Node support](https://github.com/tsoniclang/tsonic/blob/main/docs/reference/targets/csharp/node-capability.md)
- [C# support inventory](https://github.com/tsoniclang/tsonic/blob/main/docs/reference/targets/csharp/support-inventory.md)

Install this capability next to the C# target in the application package:

```sh
npm install --save-dev @tsonic/csharp-nodejs@^0.1.0
```

Authored source then imports standard modules such as `node:fs`. Selecting the
JavaScript source surface is a separate choice and is not required for Node
module imports.

## Develop this capability

```sh
npm install
npm run build
npm test
```

`npm test` uses Tsonic's bounded parallel gate and includes provider-contract,
emitted-C#, and .NET runtime proofs.
