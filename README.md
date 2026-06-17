# @tsonic/csharp-nodejs

C# implementation package for the portable `@tsonic/nodejs` source surface.

This package owns .NET-backed Node API behavior for the `csharp` target only.
It may import `@tsonic/dotnet`, ASP.NET Core bindings, and C# runtime packages.
The portable `@tsonic/nodejs` package must not.

User source imports remain target-neutral:

```ts
import { readFileSync } from "@tsonic/nodejs/fs.js";

const text = readFileSync("input.txt", "utf8");
```

The C# target maps that source operation to this package when building for
`--target csharp`.
