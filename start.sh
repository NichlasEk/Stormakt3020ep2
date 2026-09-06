#!/usr/bin/env bash
set -euo pipefail
cd -- "$(dirname -- "${BASH_SOURCE[0]}")"
mkdir -p artifacts
godot-mono --headless --editor --path "$PWD" --import > artifacts/import.log 2>&1
dotnet build AtlandsArv.csproj --nologo --verbosity quiet
exec godot-mono --path "$PWD" "$@"
