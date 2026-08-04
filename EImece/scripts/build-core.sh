#!/usr/bin/env bash
# Build the ASP.NET Core 8 migration projects (Phase 2+).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"
export DOTNET_ROOT="${HOME}/.dotnet"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Installing .NET SDK 8..."
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "${HOME}/.dotnet"
fi

echo "Building Resources + EImece.Web (Debug)..."
dotnet restore "${ROOT}/EImece.Web/EImece.Web.csproj"
dotnet build "${ROOT}/Resources/Resources.csproj" -c Debug --no-restore 2>/dev/null || \
  dotnet build "${ROOT}/Resources/Resources.csproj" -c Debug
dotnet build "${ROOT}/EImece.Web/EImece.Web.csproj" -c Debug

echo "Build succeeded."
echo "  Run (Debug):  dotnet run --project ${ROOT}/EImece.Web/EImece.Web.csproj -c Debug --launch-profile EImece.Web"
echo "  Health URL:   http://localhost:5080/health"
