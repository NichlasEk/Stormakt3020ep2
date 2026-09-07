#!/usr/bin/env bash
# Launch the current verified Linux checkpoint without rebuilding the project.
set -euo pipefail
project_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
release_dir="$project_dir/dist/AtlandsArv-0.9.1-cinematic"
if [[ ! -x "$release_dir/AtlandsArv.x86_64" || ! -f "$release_dir/AtlandsArv.pck" || ! -f "$release_dir/data_AtlandsArv_linuxbsd_x86_64/AtlandsArv.dll" ]]; then
    printf 'Den färdiga Linux-exporten saknas eller är ofullständig: %s\n' "$release_dir" >&2
    printf 'Behåll hela exportmappen tillsammans. För att bygga och starta från källkoden: %s/start.sh\n' "$project_dir" >&2
    exit 1
fi
cd -- "$release_dir"
exec ./AtlandsArv.x86_64 "$@"
