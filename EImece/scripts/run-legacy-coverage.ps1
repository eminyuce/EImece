#Requires -Version 5.1
<#
.SYNOPSIS
  Builds and runs legacy unit + integration tests with optional Coverlet coverage.

.EXAMPLE
  .\scripts\run-legacy-coverage.ps1
  .\scripts\run-legacy-coverage.ps1 -SkipIntegration
#>
param(
  [switch]$SkipIntegration
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$unitProj = Join-Path $root 'EImece.Tests\EImece.Tests.csproj'
$integProj = Join-Path $root 'EImece.Integration.Tests\EImece.Integration.Tests.csproj'
$outDir = Join-Path $root 'TestResults\legacy-coverage'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Write-Host '=== Build EImece.Tests ===' -ForegroundColor Cyan
msbuild $unitProj /t:Restore,Build /p:Configuration=Debug /v:m
if ($LASTEXITCODE -ne 0) { throw "Unit project build failed" }

$unitDll = Join-Path $root 'EImece.Tests\bin\Debug\EImece.Tests.dll'
Write-Host '=== Run unit tests (vstest) ===' -ForegroundColor Cyan
vstest.console.exe $unitDll /Logger:trx /ResultsDirectory:"$outDir\unit"
if ($LASTEXITCODE -ne 0) { Write-Warning "Unit tests reported failures (exit $LASTEXITCODE)" }

if (-not $SkipIntegration) {
  Write-Host '=== Build + run Integration.Tests (dotnet) ===' -ForegroundColor Cyan
  Push-Location (Join-Path $root 'EImece.Integration.Tests')
  try {
    dotnet test --configuration Debug --collect:"XPlat Code Coverage" --results-directory "$outDir\integration" --settings /dev/null 2>$null
    # Fallback without runsettings
    if ($LASTEXITCODE -ne 0) {
      dotnet test --configuration Debug --results-directory "$outDir\integration"
    }
  } finally { Pop-Location }
}

Write-Host @"

Coverage notes
--------------
- Unit: EImece.Tests (MSTest). Prefer OpenCover/Visual Studio Analyze Code Coverage for net481 assemblies.
- Integration: EImece.Integration.Tests uses Coverlet collector when available.
- Critical denominator: EImece.Domain.Services + Admin AjaxController + cart/coupon/order helpers.
- See docs/LEGACY_TEST_COVERAGE.md for scope, deferred areas, and risks.

Results under: $outDir
"@ -ForegroundColor Green
