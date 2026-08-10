<#
.SYNOPSIS
  Uploads a restricted set of published ASP.NET folders to a remote FTP/FTPS endpoint.

.DESCRIPTION
  Intended for production deployment of MSBuild FileSystem publish output.
  - Authenticates with credentials from parameters (pass via GitHub Actions Secrets).
  - Prefers FTPS (Explicit TLS) when -UseFtps is set (default).
  - Never deletes remote files or directories (no mirror/purge).
  - Uploads ONLY these folders: bin, Views, Content, Scripts.
  - Never overwrites Web.config or anything under media/ (kept as-is on the server).

.PARAMETER LocalPath
  Local directory containing the published application (e.g. artifacts/publish).

.PARAMETER FtpHost
  FTP hostname (no scheme).

.PARAMETER FtpUsername
  FTP username.

.PARAMETER FtpPassword
  FTP password.

.PARAMETER FtpPath
  Remote base path (e.g. /site/wwwroot or /).

.PARAMETER FtpPort
  FTP port (default 21).

.PARAMETER UseFtps
  Use explicit FTPS (FTP over TLS). Default: $true.

.PARAMETER SkipTlsValidation
  Skip remote certificate validation (not recommended for production).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$LocalPath,

    [Parameter(Mandatory = $true)]
    [string]$FtpHost,

    [Parameter(Mandatory = $true)]
    [string]$FtpUsername,

    [Parameter(Mandatory = $true)]
    [string]$FtpPassword,

    [Parameter(Mandatory = $true)]
    [string]$FtpPath,

    [int]$FtpPort = 21,

    [bool]$UseFtps = $true,

    [bool]$SkipTlsValidation = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $LocalPath -PathType Container)) {
    throw "Local publish path does not exist: $LocalPath"
}

$LocalPath = (Resolve-Path -LiteralPath $LocalPath).Path

# Allowlist only — everything else (including Web.config and media/) stays on the server.
$AllowedRootFolders = @(
    'bin',
    'Views',
    'Content',
    'Scripts'
)

function Normalize-RelPath([string]$path) {
    return (($path -replace '\\', '/') -replace '^/+', '').Trim()
}

function Test-IsAllowedUpload([string]$relativePath) {
    $rel = Normalize-RelPath $relativePath
    if ([string]::IsNullOrWhiteSpace($rel)) { return $false }

    # Hard deny: never overwrite production Web.config or media.
    if ($rel -ieq 'Web.config' -or $rel -ieq 'web.config') { return $false }
    if ($rel.StartsWith('media/', [System.StringComparison]::OrdinalIgnoreCase) -or
        $rel -ieq 'media') {
        return $false
    }
    if ($rel -ieq 'ConnectionStrings.config') { return $false }

    $root = ($rel -split '/')[0]
    foreach ($allowed in $AllowedRootFolders) {
        if ($root -ieq $allowed) {
            # Skip local IDE / debug leftovers inside allowed folders.
            if ($rel -match '\.(user|suo|pdb)$') { return $false }
            return $true
        }
    }

    return $false
}

if ($SkipTlsValidation) {
    Write-Warning 'FTP TLS certificate validation is disabled for this run.'
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
}

# Prefer TLS 1.2+
[System.Net.ServicePointManager]::SecurityProtocol = `
    [System.Net.SecurityProtocolType]::Tls12 -bor `
    [System.Net.SecurityProtocolType]::Tls11 -bor `
    [System.Net.SecurityProtocolType]::Tls

$credential = New-Object System.Net.NetworkCredential($FtpUsername, $FtpPassword)
$createdDirs = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)

function Ensure-RemoteDirectory([string]$remoteDirUrl) {
    $normalized = $remoteDirUrl.TrimEnd('/') + '/'
    if ($createdDirs.Contains($normalized)) { return }

    # Create each segment from the FTP root path downward.
    $uri = [Uri]$normalized
    $segments = @($uri.AbsolutePath.Trim('/').Split('/') | Where-Object { $_ })
    $builder = New-Object System.UriBuilder $uri
    $accum = ''

    foreach ($segment in $segments) {
        $accum = if ($accum) { "$accum/$segment" } else { $segment }
        $builder.Path = '/' + $accum
        $dirUrl = $builder.Uri.AbsoluteUri.TrimEnd('/') + '/'
        if ($createdDirs.Contains($dirUrl)) { continue }

        try {
            $req = [System.Net.FtpWebRequest]::Create($dirUrl)
            $req.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
            $req.Credentials = $credential
            $req.UseBinary = $true
            $req.UsePassive = $true
            $req.KeepAlive = $false
            $req.EnableSsl = $UseFtps
            $resp = $req.GetResponse()
            $resp.Close()
        }
        catch {
            # 550 often means the directory already exists — continue.
            $msg = $_.Exception.Message
            if ($msg -notmatch '550|exists|File unavailable') {
                Write-Warning "Could not ensure remote directory ${dirUrl}: $msg"
            }
        }

        [void]$createdDirs.Add($dirUrl)
    }
}

function Upload-File([string]$localFile, [string]$remoteFileUrl) {
    $parent = $remoteFileUrl.Substring(0, $remoteFileUrl.LastIndexOf('/') + 1)
    Ensure-RemoteDirectory $parent

    $bytes = [System.IO.File]::ReadAllBytes($localFile)
    $req = [System.Net.FtpWebRequest]::Create($remoteFileUrl)
    $req.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
    $req.Credentials = $credential
    $req.UseBinary = $true
    $req.UsePassive = $true
    $req.KeepAlive = $false
    $req.EnableSsl = $UseFtps
    $req.ContentLength = $bytes.Length

    $stream = $req.GetRequestStream()
    try {
        $stream.Write($bytes, 0, $bytes.Length)
    }
    finally {
        $stream.Close()
    }

    $resp = $req.GetResponse()
    try {
        $null = $resp.StatusDescription
    }
    finally {
        $resp.Close()
    }
}

$scheme = 'ftp'
$builder = New-Object System.UriBuilder
$builder.Scheme = $scheme
$builder.Host = $FtpHost
$builder.Port = $FtpPort
$builder.Path = ($FtpPath -replace '\\', '/')
if (-not $builder.Path.StartsWith('/')) {
    $builder.Path = '/' + $builder.Path
}
$remoteRoot = $builder.Uri.GetLeftPart([System.UriPartial]::Path).TrimEnd('/')

Write-Host "Deploying published output via $(if ($UseFtps) { 'FTPS' } else { 'FTP' })"
Write-Host "  Host: $FtpHost"
Write-Host "  Port: $FtpPort"
Write-Host "  Path: $($builder.Path)"
Write-Host "  Local: $LocalPath"
Write-Host "  Remote delete: DISABLED"
Write-Host "  Allowlist folders: $($AllowedRootFolders -join ', ')"
Write-Host "  Never overwrite: Web.config, media/"

$missingAllowed = @()
foreach ($folder in $AllowedRootFolders) {
    $candidate = Join-Path $LocalPath $folder
    if (-not (Test-Path -LiteralPath $candidate -PathType Container)) {
        $missingAllowed += $folder
    }
}
if ($missingAllowed.Count -gt 0) {
    throw "Publish output is missing required folder(s) for deploy: $($missingAllowed -join ', ')"
}

$files = Get-ChildItem -LiteralPath $LocalPath -Recurse -File
$uploaded = 0
$skipped = 0

foreach ($file in $files) {
    $relative = $file.FullName.Substring($LocalPath.Length).TrimStart('\', '/')
    $relNorm = Normalize-RelPath $relative

    if (-not (Test-IsAllowedUpload $relNorm)) {
        $skipped++
        continue
    }

    $remoteUrl = $remoteRoot.TrimEnd('/') + '/' + $relNorm
    Write-Host "Uploading $relNorm"
    Upload-File -localFile $file.FullName -remoteFileUrl $remoteUrl
    $uploaded++
}

Write-Host ""
Write-Host "FTPS deployment finished."
Write-Host "  Uploaded: $uploaded"
Write-Host "  Skipped (outside allowlist / protected): $skipped"
Write-Host "  Remote Web.config and media/ were NOT modified."
Write-Host "  Remote files were NOT deleted."

if ($uploaded -eq 0) {
    throw 'No files were uploaded. Check LocalPath and allowlist rules.'
}
