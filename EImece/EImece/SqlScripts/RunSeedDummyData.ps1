<#
.SYNOPSIS
    EImece Database & Image Generator Utility.

.DESCRIPTION
    Flexible utility to generate dummy data, image files, or both for EImece.

    MODES OF OPERATION:
    1. Images Only (Default):
       .\RunSeedDummyData.ps1
       .\RunSeedDummyData.ps1 -ImagesOnly
       (Generates JPEG image files & thumbnails for existing database records without changing SQL data).

    2. Data + Images (Full Seed):
       .\RunSeedDummyData.ps1 -SeedDatabase
       (Populates SQL tables with dummy data AND generates missing JPEG image files).

    3. Data Only (No Images):
       .\RunSeedDummyData.ps1 -SeedDatabase -SkipImages
       (Populates SQL tables with dummy data without creating image files on disk).

    4. Cleanup:
       .\RunSeedDummyData.ps1 -CleanupDatabase
       (Cleans up dummy seed data from the SQL database).

.EXAMPLE
    .\RunSeedDummyData.ps1 -ImagesOnly

.EXAMPLE
    .\RunSeedDummyData.ps1 -SeedDatabase

.EXAMPLE
    .\RunSeedDummyData.ps1 -SeedDatabase -SkipImages
#>
[CmdletBinding()]
param(
    [string] $ConnectionString = "",

    [string] $Server = "",

    [string] $Database = "",

    # Target folder for generated images (auto-detected if omitted)
    [string] $MediaRoot = "",

    # Multiplier scale for seed data (1.0 = standard, 2.0 = double volume)
    [double] $Scale = 1.0,

    # Mode 1: Generate Images Only (default if no SQL switches supplied)
    [switch] $ImagesOnly,

    # Mode 2 & 3: Run SQL database seed script
    [switch] $SeedDatabase,

    # Mode 3: Skip image generation when seeding database data
    [switch] $SkipImages,

    # Mode 4: Run SQL cleanup script
    [switch] $CleanupDatabase
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appDir = Split-Path -Parent $scriptDir

# Auto-detect ConnectionString from env variable or Web.config if not explicitly provided
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    if ($env:EIMECE_DB_CONNECTION_STRING) {
        $ConnectionString = $env:EIMECE_DB_CONNECTION_STRING
    } else {
        $webConfig = Join-Path $appDir "Web.config"
        if (Test-Path $webConfig) {
            try {
                $xml = [xml](Get-Content $webConfig)
                $csNode = $xml.configuration.connectionStrings.add | Where-Object { $_.name -eq 'EImeceDbConnection' }
                if ($csNode -and $csNode.connectionString) {
                    $ConnectionString = $csNode.connectionString
                }
            } catch {}
        }
    }
}

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $ConnectionString = "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;"
}

# Auto-detect local project media path
if ([string]::IsNullOrWhiteSpace($MediaRoot)) {
    $projectMedia = Join-Path $appDir "media\images"
    if (Test-Path (Split-Path -Parent $projectMedia)) {
        $MediaRoot = $projectMedia
    } else {
        $MediaRoot = "C:\inetpub\wwwroot\Eimece\media\images"
    }
}

# Determine active execution mode
$modeName = "Images Only"
if ($CleanupDatabase) {
    $modeName = "Database Cleanup"
} elseif ($SeedDatabase -and $SkipImages) {
    $modeName = "Data Only (No Images)"
} elseif ($SeedDatabase) {
    $modeName = "Data + Images (Full Seed)"
}

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " EImece Data & Image Utility [$modeName]" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "ConnectionString  : $ConnectionString" -ForegroundColor Yellow
Write-Host "Media Root Folder : $MediaRoot" -ForegroundColor Yellow

# Helper function to run SQL script
function Invoke-SqlScriptFile {
    param(
        [string] $Path,
        [hashtable] $Replacements = @{}
    )

    if (-not (Test-Path $Path)) {
        throw "SQL script file not found: $Path"
    }

    $sql = Get-Content -Path $Path -Raw -Encoding UTF8
    foreach ($key in $Replacements.Keys) {
        $sql = $sql -replace $key, [string]$Replacements[$key]
    }

    Write-Host "`nExecuting SQL script $Path ..." -ForegroundColor Cyan

    Add-Type -AssemblyName System.Data
    $conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $conn.Open()
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sql
        $cmd.CommandTimeout = 0
        $null = $cmd.ExecuteNonQuery()
        Write-Host "SQL script executed successfully." -ForegroundColor Green
    }
    finally {
        $conn.Close()
    }
}

# Mode 4: Cleanup
if ($CleanupDatabase) {
    $cleanupPath = Join-Path $scriptDir "CleanupDummyData.sql"
    Invoke-SqlScriptFile -Path $cleanupPath
    if (-not $SkipImages -and (Test-Path $MediaRoot)) {
        Write-Host "Removing generated seed image files from $MediaRoot ..." -ForegroundColor Cyan
        Get-ChildItem -Path $MediaRoot -Filter "product-*.jpg" -File -ErrorAction SilentlyContinue | Remove-Item -Force
        $thumbRoot = Join-Path $MediaRoot "thumbs"
        if (Test-Path $thumbRoot) {
            Get-ChildItem -Path $thumbRoot -Filter "thbproduct-*.jpg" -File -ErrorAction SilentlyContinue | Remove-Item -Force
        }
    }
    Write-Host "`nCleanup Complete!" -ForegroundColor Green
    return
}

# Mode 2 & 3: Seed Database Data
if ($SeedDatabase) {
    $seedPath = Join-Path $scriptDir "SeedDummyData.sql"
    $scaleLiteral = ([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0}", $Scale))
    $replacements = @{
        'DECLARE @Scale\s+FLOAT\s+=\s+[0-9.]+' = "DECLARE @Scale         FLOAT        = $scaleLiteral"
    }
    Invoke-SqlScriptFile -Path $seedPath -Replacements $replacements
}

# Mode 1 & 2: Generate Image Files (unless -SkipImages is passed)
if (-not $SkipImages) {
    $genScript = Join-Path $scriptDir "GenerateSeedImages.ps1"
    if (-not (Test-Path $genScript)) {
        throw "GenerateSeedImages.ps1 script not found: $genScript"
    }

    Write-Host "`nGenerating images for all FileStorage records in database..." -ForegroundColor Cyan
    & $genScript -MediaRoot $MediaRoot -ConnectionString $ConnectionString -MarkExisting

    # Sync generated images to IIS website directory if present
    $iisMedia = "C:\inetpub\wwwroot\Eimece\media\images"
    if ((Test-Path $MediaRoot) -and (Test-Path (Split-Path -Parent $iisMedia)) -and ($MediaRoot -ne $iisMedia)) {
        Write-Host "`nSyncing generated images to IIS website folder ($iisMedia)..." -ForegroundColor Cyan
        try {
            Copy-Item -Path "$MediaRoot\*" -Destination $iisMedia -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "Successfully synced images to IIS." -ForegroundColor Green
        }
        catch {
            Write-Warning "Could not copy images to IIS folder: $_"
        }
    }
}

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " Task Completed Successfully! [$modeName]" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
