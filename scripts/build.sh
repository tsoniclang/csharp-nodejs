#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TSONIC_ROOT="$(cd "$REPO_ROOT/../tsonic" && pwd -P)"
CSHARP_TARGET_ROOT="$(cd "$REPO_ROOT/../tsonic-csharp" && pwd -P)"
SCRIPT_PATH="$REPO_ROOT/scripts/build.sh"

if [[ "${TSONIC_TEST_PREPARED:-0}" == "1" && "${TSONIC_PREPARE_BUILD:-0}" != "1" ]]; then
  echo "FAIL: csharp-nodejs build attempted while TSONIC_TEST_PREPARED=1." >&2
  echo "Prepared test shards must consume existing artifacts and must not rebuild shared packages." >&2
  exit 1
fi

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
      "@tsonic/target-api": ["$TSONIC_ROOT/packages/target-api/dist/index.d.ts"],
      "@tsonic/target-csharp": ["$CSHARP_TARGET_ROOT/dist/index.d.ts"]
    }
  },
  "references": []
}
JSON

"$TSONIC_ROOT/scripts/build/tsgo-project.sh" "$CANONICAL_TSCONFIG" --pretty false
dotnet build "$REPO_ROOT/csharp/src/Tsonic.CSharp.Node/Tsonic.CSharp.Node.csproj" --verbosity minimal
