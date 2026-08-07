<#
.SYNOPSIS
    Creates JPEG placeholders (+ thumbs) for seed FileStorage rows under the site media/images folder.

.DESCRIPTION
    The app resolves uploads via Constants.ServerMapPath (~/media/images/) using FileStorage.FileName.
    SeedDummyData.sql inserts FileName values like product-00001.jpg; this script writes those files
    (and thumbs\thbproduct-00001.jpg) so admin/storefront images load.

.EXAMPLE
    .\GenerateSeedImages.ps1 -MediaRoot "C:\inetpub\wwwroot\Eimece\media\images"

.EXAMPLE
    .\GenerateSeedImages.ps1 -MediaRoot "C:\inetpub\wwwroot\Eimece\media\images" -ConnectionString "..." -MarkExisting
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $MediaRoot,

    [string] $ConnectionString,

    [string] $Server = "YUCE\SQLEXPRESS",

    [string] $Database = "yuva8905_yuvadan",

    [string] $SqlUser = "sqluser",

    [string] $SqlPassword = "sqluser",

    # When set, UPDATE IsFileExist=1 for seed FileStorages whose files were written.
    [switch] $MarkExisting,

    # Optional override instead of reading filenames from SQL.
    [int] $Count = 0,

    [int] $Width = 1200,

    [int] $Height = 900,

    [int] $ThumbWidth = 300
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

function Get-SeedFileNames {
    param([string] $ConnStr)

    if ($Count -gt 0) {
        return 1..$Count | ForEach-Object { "product-{0:D5}.jpg" -f $_ }
    }

    $sql = @"
SELECT FileName
FROM dbo.FileStorages
WHERE FileUrl LIKE N'/media/seed/%'
   OR FileUrl LIKE N'/media/images/product-%'
ORDER BY Id;
"@

    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        if ($ConnStr) {
            # Parse basic Server/Database/User from connection string when possible; fall back to params.
            $serverMatch = [regex]::Match($ConnStr, '(?i)(?:Server|Data Source)=([^;]+)')
            $dbMatch = [regex]::Match($ConnStr, '(?i)(?:Database|Initial Catalog)=([^;]+)')
            $userMatch = [regex]::Match($ConnStr, '(?i)(?:User ID|UID)=([^;]+)')
            $pwdMatch = [regex]::Match($ConnStr, '(?i)(?:Password|PWD)=([^;]+)')
            if ($serverMatch.Success) { $script:Server = $serverMatch.Groups[1].Value }
            if ($dbMatch.Success) { $script:Database = $dbMatch.Groups[1].Value }
            if ($userMatch.Success) { $script:SqlUser = $userMatch.Groups[1].Value }
            if ($pwdMatch.Success) { $script:SqlPassword = $pwdMatch.Groups[1].Value }
        }

        $rows = sqlcmd -S $Server -d $Database -U $SqlUser -P $SqlPassword -Q $sql -h -1 -W 2>$null |
            Where-Object { $_ -and $_.Trim() -ne '' -and $_ -notmatch 'rows affected' -and $_ -notmatch '^-+$' }
        return @($rows | ForEach-Object { $_.Trim() } | Where-Object { $_ -like '*.jpg' -or $_ -like '*.png' -or $_ -like '*.jpeg' })
    }

    throw "sqlcmd not found and -Count was not provided."
}

function New-SeedJpeg {
    param(
        [string] $Path,
        [int] $ImgWidth,
        [int] $ImgHeight,
        [string] $Label,
        [System.Drawing.Color] $BackColor
    )

    $bmp = New-Object System.Drawing.Bitmap $ImgWidth, $ImgHeight
    try {
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        try {
            $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
            $g.Clear($BackColor)

            # Soft panels for a non-flat placeholder look
            $panelBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(40, 255, 255, 255))
            $g.FillRectangle($panelBrush, [int]($ImgWidth * 0.08), [int]($ImgHeight * 0.12), [int]($ImgWidth * 0.84), [int]($ImgHeight * 0.76))
            $panelBrush.Dispose()

            $font = New-Object System.Drawing.Font "Segoe UI", ([Math]::Max(14, [int]($ImgWidth / 28))), ([System.Drawing.FontStyle]::Bold)
            $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(230, 255, 255, 255))
            $sf = New-Object System.Drawing.StringFormat
            $sf.Alignment = [System.Drawing.StringAlignment]::Center
            $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
            $rect = New-Object System.Drawing.RectangleF 0, 0, $ImgWidth, $ImgHeight
            $g.DrawString($Label, $font, $brush, $rect, $sf)

            $font.Dispose()
            $brush.Dispose()
            $sf.Dispose()
        }
        finally {
            $g.Dispose()
        }

        $dir = Split-Path -Parent $Path
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Force -Path $dir | Out-Null
        }

        $codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
        $encoder = [System.Drawing.Imaging.Encoder]::Quality
        $params = New-Object System.Drawing.Imaging.EncoderParameters 1
        $params.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter $encoder, 85L
        $bmp.Save($Path, $codec, $params)
        $params.Dispose()
    }
    finally {
        $bmp.Dispose()
    }
}

if (-not (Test-Path $MediaRoot)) {
    New-Item -ItemType Directory -Force -Path $MediaRoot | Out-Null
}
$thumbRoot = Join-Path $MediaRoot "thumbs"
if (-not (Test-Path $thumbRoot)) {
    New-Item -ItemType Directory -Force -Path $thumbRoot | Out-Null
}

$fileNames = @(Get-SeedFileNames -ConnStr $ConnectionString)
if ($fileNames.Count -eq 0) {
    Write-Host "No seed FileStorage filenames found. Nothing to generate." -ForegroundColor Yellow
    return
}

Write-Host "Generating $($fileNames.Count) seed images under $MediaRoot ..." -ForegroundColor Cyan

$palette = @(
    [System.Drawing.Color]::FromArgb(255, 41, 98, 255),
    [System.Drawing.Color]::FromArgb(255, 16, 185, 129),
    [System.Drawing.Color]::FromArgb(255, 245, 158, 11),
    [System.Drawing.Color]::FromArgb(255, 239, 68, 68),
    [System.Drawing.Color]::FromArgb(255, 139, 92, 246),
    [System.Drawing.Color]::FromArgb(255, 20, 184, 166)
)

$thumbHeight = [Math]::Max(1, [int]($Height * ($ThumbWidth / [double]$Width)))
$written = 0
$index = 0

foreach ($name in $fileNames) {
    $index++
    $safeName = [System.IO.Path]::GetFileName($name)
    if ([string]::IsNullOrWhiteSpace($safeName)) { continue }

    $fullPath = Join-Path $MediaRoot $safeName
    $thumbPath = Join-Path $thumbRoot ("thb" + $safeName)
    $color = $palette[($index - 1) % $palette.Count]
    $label = "EImece seed`n$safeName"

    New-SeedJpeg -Path $fullPath -ImgWidth $Width -ImgHeight $Height -Label $label -BackColor $color
    New-SeedJpeg -Path $thumbPath -ImgWidth $ThumbWidth -ImgHeight $thumbHeight -Label $safeName -BackColor $color
    $written++

    if ($written % 50 -eq 0) {
        Write-Host "  ... $written / $($fileNames.Count)"
    }
}

Write-Host "Wrote $written images and $written thumbs." -ForegroundColor Green

if ($MarkExisting) {
    $updateSql = @"
UPDATE dbo.FileStorages
SET IsFileExist = 1,
    UpdatedDate = GETDATE(),
    FileSize = CASE WHEN FileSize IS NULL OR FileSize < 1000 THEN 85000 ELSE FileSize END
WHERE FileUrl LIKE N'/media/seed/%'
   OR FileUrl LIKE N'/media/images/product-%';
SELECT COUNT(*) AS MarkedExisting FROM dbo.FileStorages WHERE IsFileExist = 1 AND (FileUrl LIKE N'/media/seed/%' OR FileUrl LIKE N'/media/images/product-%');
"@
    sqlcmd -S $Server -d $Database -U $SqlUser -P $SqlPassword -Q $updateSql -W
    Write-Host "Marked seed FileStorages as IsFileExist=1." -ForegroundColor Green
}
