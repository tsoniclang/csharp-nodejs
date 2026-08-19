#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TSONIC_ROOT="$(cd "$REPO_ROOT/../tsonic" && pwd -P)"
CSHARP_TARGET_ROOT="$(cd "$REPO_ROOT/../tsonic-csharp" && pwd -P)"
SCRIPT_PATH="$REPO_ROOT/scripts/build.sh"

if [[ "${TSONIC_BUILD_LOCK_HELD:-0}" != "1" ]]; then
  exec "$TSONIC_ROOT/scripts/build/with-lock.sh" "$SCRIPT_PATH" "$@"
fi

if [[ "${TSONIC_SKIP_DEPENDENCY_BUILDS:-0}" != "1" ]]; then
  for package_dir in packages/source-core packages/target-api; do
    (cd "$TSONIC_ROOT/$package_dir" && npm run build)
  done
  (cd "$CSHARP_TARGET_ROOT" && npm run build)
fi

mkdir -p "$REPO_ROOT/.temp/build"
CANONICAL_TSCONFIG="$REPO_ROOT/.temp/build/tsconfig.canonical-tsonic.json"
cat > "$CANONICAL_TSCONFIG" <<JSON
{
  "extends": "../../tsconfig.json",
  "compilerOptions": {
    "paths": {
      "@tsonic/tsts": ["$TSONIC_ROOT/packages/tsts/dist/src/index.d.ts"],
      "@tsonic/target-api": ["$TSONIC_ROOT/packages/target-api/dist/public/index.d.ts"],
      "@tsonic/target-api/*": ["$TSONIC_ROOT/packages/target-api/dist/public/*.d.ts"],
      "@tsonic/target-csharp/provider": ["$CSHARP_TARGET_ROOT/dist/public/provider.d.ts"]
    }
  },
  "references": []
}
JSON

"$TSONIC_ROOT/scripts/build/tsgo-project.sh" "$CANONICAL_TSCONFIG" --pretty false
dotnet build "$REPO_ROOT/csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj" --verbosity minimal
