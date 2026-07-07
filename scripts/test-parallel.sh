#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

node --test test/*.test.mjs &
node_pid=$!

dotnet test Tsonic.CSharp.Node.slnx --no-build --no-restore --verbosity minimal &
dotnet_pid=$!

status=0
wait "$node_pid" || status=$?
wait "$dotnet_pid" || status=$?
exit "$status"
