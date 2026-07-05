#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Installing .NET SDK..."
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "${HOME}/.dotnet"
fi

echo "Restoring NuGet packages..."
python3 "${ROOT}/scripts/restore-packages.py"

echo "Building solution (Release)..."
dotnet msbuild "${ROOT}/EImece.sln" \
  /t:Clean,Build \
  /p:Configuration=Release \
  /p:DeployOnBuild=false \
  /v:minimal

echo "Build succeeded."
