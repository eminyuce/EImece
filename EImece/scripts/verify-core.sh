#!/usr/bin/env bash
# Build + test the ASP.NET Core 8 host (Phase 9).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"
export DOTNET_ROOT="${HOME}/.dotnet"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Installing .NET SDK 8..."
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "${HOME}/.dotnet"
fi

# Neutral cwd — empty stub *.csproj files under ${ROOT} confuse the SDK.
cd /tmp

echo "==> Restore / build EImece.Web (Debug)"
dotnet restore "${ROOT}/EImece.Web/EImece.Web.csproj"
dotnet build "${ROOT}/EImece.Web/EImece.Web.csproj" -c Debug --no-restore

echo "==> Test EImece.Web.Tests"
dotnet test "${ROOT}/EImece.Web.Tests/EImece.Web.Tests.csproj" -c Debug --no-restore

if [[ "${VERIFY_CORE_CURL:-}" == "1" ]]; then
  BASE_URL="${VERIFY_CORE_URL:-http://localhost:5080}"
  echo "==> Curl smoke against ${BASE_URL}"
  curl -fsS "${BASE_URL}/health" | head -c 400; echo
  curl -fsS -o /dev/null -w "defaultImage %{http_code}\n" "${BASE_URL}/images/defaultImage/w100h100/default.jpg"
  curl -fsS -o /dev/null -w "checkout %{http_code}\n" "${BASE_URL}/Payment/Checkout/"
fi

echo "verify-core succeeded."
