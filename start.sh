#!/usr/bin/env bash
set -euo pipefail
cd -- "$(dirname -- "${BASH_SOURCE[0]}")"
dotnet build --nologo --verbosity quiet
exec godot-mono --path "$PWD" "$@"
