<#
.SYNOPSIS
    Generates missing product & media images for existing database records.

.DESCRIPTION
    Reads existing FileStorage rows from the EImece database and generates missing JPEG image files
    and thumbnails under media/images so that product detail pages, category listings, and storefront
    render properly without broken images.

.EXAMPLE
    .\RunSeedDummyData.ps1

.EXAMPLE
    .\RunSeedDummyData.ps1 -ConnectionString "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;"

.EXAMPLE
    .\RunSeedDummyData.ps1 -SeedDatabase
#>
[CmdletBinding(DefaultParameterSetName = 'Default')]
param(
    [string] $ConnectionString = "",

    [string] $Server = "",

    [string] $Database = "",

    # Target folder for generated images (auto-detected if omitted)
    [string] $MediaRoot = "",

    # Switch to re-run SQL database seed script if explicitly desired
    [switch] $SeedDatabase,

    # Switch to run SQL cleanup script
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

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " EImece Image Generator Utility" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "ConnectionString  : $ConnectionString" -ForegroundColor Yellow
Write-Host "Media Root Folder : $MediaRoot" -ForegroundColor Yellow

# Helper function to run SQL script
function Invoke-SqlScriptFile {
    param([string] $Path)

    if (-not (Test-Path $Path)) {
        throw "SQL script file not found: $Path"
    }

    $sql = Get-Content -Path $Path -Raw -Encoding UTF8
    Write-Host "Executing SQL script $Path ..." -ForegroundColor Cyan

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

# Optional: Run SQL Database Cleanup
if ($CleanupDatabase) {
    $cleanupPath = Join-Path $scriptDir "CleanupDummyData.sql"
    Invoke-SqlScriptFile -Path $cleanupPath
}

# Optional: Run SQL Database Seed
if ($SeedDatabase) {
    $seedPath = Join-Path $scriptDir "SeedDummyData.sql"
    Invoke-SqlScriptFile -Path $seedPath
}

# Core Task: Generate Images for Existing Database Records
$genScript = Join-Path $scriptDir "GenerateSeedImages.ps1"
if (-not (Test-Path $genScript)) {
    throw "GenerateSeedImages.ps1 script not found: $genScript"
}

Write-Host "`nGenerating images for all FileStorage records in database..." -ForegroundColor Cyan
& $genScript -MediaRoot $MediaRoot -ConnectionString $ConnectionString -MarkExisting

# If IIS media directory exists, sync generated images to IIS as well
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

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " Image Generation Complete!" -ForegroundColor Green
Write-Host " All product and media images have been generated." -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
