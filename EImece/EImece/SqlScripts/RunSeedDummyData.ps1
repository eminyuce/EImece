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
       Includes Tema Ornekleri + PT Dummy T1-T8 menus, each with MenuMainImage and 12 MenuGallery files.

    3. Data Only (No Images):
       .\RunSeedDummyData.ps1 -SeedDatabase -SkipImages
       (Populates SQL tables with dummy data without creating image files on disk).

    4. Theme pages only (no catalog wipe):
       .\RunSeedDummyData.ps1 -ThemePages
       (Upserts T1-T8 CMS menus in the storefront nav, attaches main + gallery images,
        writes menu-theme-*.jpg files. Same association as Admin media:
        /admin/media/?contentId={menuId}&mod=Menus&imageType=MenuGallery)

    5. Cleanup:
       .\RunSeedDummyData.ps1 -CleanupDatabase
       (Cleans up dummy seed data from the SQL database).

.EXAMPLE
    .\RunSeedDummyData.ps1 -ImagesOnly

.EXAMPLE
    .\RunSeedDummyData.ps1 -SeedDatabase

.EXAMPLE
    .\RunSeedDummyData.ps1 -ThemePages

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

    # Mode 4: Upsert PT Dummy T1-T8 menus + MenuMainImage + MenuGallery (no catalog wipe)
    [switch] $ThemePages,

    # Mode 5: Run SQL cleanup script
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
    throw @"
No database connection string available.
Set -ConnectionString, environment variable EIMECE_DB_CONNECTION_STRING, or a non-placeholder
EImeceDbConnection in Web.config / gitignored ConnectionStrings.config.
Do not hard-code SQL passwords in this script. See docs/SECURE_CONNECTION_STRINGS.md.
"@
}

if ($ConnectionString -match 'YOUR_SERVER|YOUR_DATABASE|YOUR_PASSWORD|CHANGEME|REPLACE_ME') {
    throw "Connection string still contains placeholders. Set EIMECE_DB_CONNECTION_STRING or pass -ConnectionString with real values."
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
} elseif ($ThemePages -and -not $SeedDatabase) {
    $modeName = "Theme Pages (T1-T8 menus + gallery)"
    if ($SkipImages) { $modeName = "Theme Pages (SQL only)" }
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
    $conn.add_InfoMessage({
        param($eventSender, $eventArgs)
        if ($eventArgs -and $eventArgs.Message) {
            Write-Host $eventArgs.Message
        }
    })
    $conn.Open()
    try {
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sql
        $cmd.CommandTimeout = 0
        $reader = $cmd.ExecuteReader()
        try {
            do {
                $printedHeader = $false
                while ($reader.Read()) {
                    if (-not $printedHeader) {
                        $cols = for ($i = 0; $i -lt $reader.FieldCount; $i++) { $reader.GetName($i) }
                        Write-Host ($cols -join " | ") -ForegroundColor DarkGray
                        $printedHeader = $true
                    }
                    $vals = for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                        if ($reader.IsDBNull($i)) { "" } else { [string]$reader.GetValue($i) }
                    }
                    Write-Host ($vals -join " | ")
                }
            } while ($reader.NextResult())
        }
        finally {
            $reader.Close()
        }
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
        Get-ChildItem -Path $MediaRoot -Filter "menu-theme-*.jpg" -File -ErrorAction SilentlyContinue | Remove-Item -Force
        $thumbRoot = Join-Path $MediaRoot "thumbs"
        if (Test-Path $thumbRoot) {
            Get-ChildItem -Path $thumbRoot -Filter "thbproduct-*.jpg" -File -ErrorAction SilentlyContinue | Remove-Item -Force
            Get-ChildItem -Path $thumbRoot -Filter "thbmenu-theme-*.jpg" -File -ErrorAction SilentlyContinue | Remove-Item -Force
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
elseif ($ThemePages) {
    $themePath = Join-Path $scriptDir "SeedThemePages.sql"
    Invoke-SqlScriptFile -Path $themePath
}

# Mode 1, 2 & 4: Generate Image Files (unless -SkipImages is passed)
if (-not $SkipImages) {
    $genScript = Join-Path $scriptDir "GenerateSeedImages.ps1"
    if (-not (Test-Path $genScript)) {
        throw "GenerateSeedImages.ps1 script not found: $genScript"
    }

    Write-Host "`nGenerating images for FileStorage records in database..." -ForegroundColor Cyan
    $genArgs = @{
        MediaRoot        = $MediaRoot
        ConnectionString = $ConnectionString
        MarkExisting     = $true
    }
    if ($ThemePages -and -not $SeedDatabase) {
        $genArgs.FileNameLike = "menu-theme-%"
        $genArgs.SkipExisting = $true
        Write-Host "  (theme pages: FileName LIKE 'menu-theme-%', skip existing files)" -ForegroundColor Yellow
    }
    & $genScript @genArgs

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
