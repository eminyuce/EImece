<#
.SYNOPSIS
    Generates JPEG placeholders (+ thumbs) for all FileStorage rows in the database under media/images folder.

.DESCRIPTION
    Queries dbo.FileStorages for all existing image filenames, creates placeholding JPEG images
    (and thumbs/thb*.jpg) in the target media folder, and sets IsFileExist = 1 in the database.

.EXAMPLE
    .\GenerateSeedImages.ps1 -MediaRoot "C:\Users\eminy\source\repos\EImece\EImece\EImece\media\images" -ConnectionString "Server=.;Database=EImece;Trusted_Connection=True;TrustServerCertificate=True;"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $MediaRoot = "",

    [string] $ConnectionString = "Server=.;Database=EImece;Trusted_Connection=True;TrustServerCertificate=True;",

    [string] $Server = ".",

    [string] $Database = "EImece",

    [switch] $MarkExisting = $true,

    [int] $Count = 0,

    [int] $Width = 1200,

    [int] $Height = 900,

    [int] $ThumbWidth = 300
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Data

if ([string]::IsNullOrWhiteSpace($ConnectionString) -and $Server -and $Database) {
    $ConnectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;"
}

if ([string]::IsNullOrWhiteSpace($MediaRoot)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $appMedia = Join-Path (Split-Path -Parent $scriptDir) "media\images"
    if (Test-Path (Split-Path -Parent $appMedia)) {
        $MediaRoot = $appMedia
    } else {
        $MediaRoot = "C:\inetpub\wwwroot\Eimece\media\images"
    }
}

function Get-SeedFileNames {
    param([string] $ConnStr)

    if ($Count -gt 0) {
        return 1..$Count | ForEach-Object { "product-{0:D5}.jpg" -f $_ }
    }

    $sql = "SELECT DISTINCT FileName FROM dbo.FileStorages WHERE FileName IS NOT NULL AND FileName <> '' ORDER BY FileName;"

    # Preferred: System.Data.SqlClient
    try {
        $conn = New-Object System.Data.SqlClient.SqlConnection $ConnStr
        $conn.Open()
        try {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = $sql
            $reader = $cmd.ExecuteReader()
            $names = New-Object System.Collections.Generic.List[string]
            while ($reader.Read()) {
                $val = $reader.GetString(0)
                if (-not [string]::IsNullOrWhiteSpace($val)) {
                    $names.Add($val.Trim())
                }
            }
            return $names.ToArray()
        }
        finally {
            $conn.Close()
        }
    }
    catch {
        Write-Warning "SqlClient query failed: $_. Trying sqlcmd..."
    }

    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        $rows = sqlcmd -S $Server -d $Database -E -Q $sql -h -1 -W 2>$null |
            Where-Object { $_ -and $_.Trim() -ne '' -and $_ -notmatch 'rows affected' -and $_ -notmatch '^-+$' }
        return @($rows | ForEach-Object { $_.Trim() } | Where-Object { $_ -like '*.jpg' -or $_ -like '*.png' -or $_ -like '*.jpeg' })
    }

    throw "Could not retrieve FileStorage filenames from database."
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
    Write-Host "No FileStorage filenames found in database. Nothing to generate." -ForegroundColor Yellow
    return
}

Write-Host "Generating $($fileNames.Count) images under $MediaRoot ..." -ForegroundColor Cyan

$palette = @(
    [System.Drawing.Color]::FromArgb(255, 37, 99, 235),
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
    $label = "EImece Media`n$safeName"

    New-SeedJpeg -Path $fullPath -ImgWidth $Width -ImgHeight $Height -Label $label -BackColor $color
    New-SeedJpeg -Path $thumbPath -ImgWidth $ThumbWidth -ImgHeight $thumbHeight -Label $safeName -BackColor $color
    $written++

    if ($written % 50 -eq 0) {
        Write-Host "  ... generated $written / $($fileNames.Count)"
    }
}

Write-Host "Successfully generated $written images and $written thumbnails in $MediaRoot." -ForegroundColor Green

if ($MarkExisting -and $ConnectionString) {
    try {
        $conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
        $conn.Open()
        try {
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = "UPDATE dbo.FileStorages SET IsFileExist = 1, UpdatedDate = GETDATE(), FileSize = CASE WHEN FileSize IS NULL OR FileSize < 1000 THEN 85000 ELSE FileSize END WHERE FileName IS NOT NULL AND FileName <> ''"
            $affected = $cmd.ExecuteNonQuery()
            Write-Host "Updated $affected FileStorage records in database with IsFileExist = 1." -ForegroundColor Green
        }
        finally {
            $conn.Close()
        }
    }
    catch {
        Write-Warning "Could not update IsFileExist in database: $_"
    }
}
