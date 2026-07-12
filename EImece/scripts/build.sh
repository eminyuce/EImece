#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
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
  /v:minimal \
  /clp:ErrorsOnly

if [[ ! -f "${ROOT}/EImece/bin/EImece.dll" && ! -f "${ROOT}/EImece/bin/Release/EImece.dll" ]]; then
  echo "ERROR: EImece.dll was not produced." >&2
  exit 1
fi

echo "Build succeeded."
echo "  Resources     -> ${ROOT}/Resources/bin/Release/Resources.dll"
echo "  EImece.Domain -> ${ROOT}/EImece.Domain/bin/Release/EImece.Domain.dll"
echo "  EImece        -> ${ROOT}/EImece/bin/EImece.dll"
echo ""
echo "See docs/BUILD_AND_RUN.md for run and verification steps."
