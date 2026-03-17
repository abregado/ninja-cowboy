#!/usr/bin/env bash
set -euo pipefail

GODOT="${GODOT:-D:/Programs/Godot_v4.6.1-stable_mono_win64/Godot_v4.6.1-stable_mono_win64.exe}"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

dotnet build "$PROJECT_DIR/Ninja Cowboy.sln" && "$GODOT" --path "$PROJECT_DIR"
