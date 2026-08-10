<#
.SYNOPSIS
  Incrementally uploads published ASP.NET files to FTP/FTPS when size or checksum changed.

.DESCRIPTION
  Intended for production deployment of MSBuild FileSystem publish output.
  - Authenticates with credentials from parameters (pass via GitHub Actions Secrets).
  - Prefers FTPS (Explicit TLS) when -UseFtps is set (default).
  - Never deletes remote files or directories (no mirror/purge).
  - Never overwrites Web.config or anything under media/ (kept as-is on the server).
  - Uploads all other publish files only when the remote file is missing, the size
    differs, or the SHA-256 checksum differs.
  - Stores/reads `.eimece-deploy-manifest.json` on the server for checksum comparison
    (FTP has no portable remote hash command). Remote SIZE is always checked too.

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
$ManifestFileName = '.eimece-deploy-manifest.json'

function Normalize-RelPath([string]$path) {
    return (($path -replace '\\', '/') -replace '^/+', '').Trim()
}

function Test-IsExcludedFromDeploy([string]$relativePath) {
    $rel = Normalize-RelPath $relativePath
    if ([string]::IsNullOrWhiteSpace($rel)) { return $true }

    # Protected production files — never overwrite.
    if ($rel -ieq 'Web.config') { return $true }
    if ($rel -ieq 'ConnectionStrings.config') { return $true }
    if ($rel -ieq 'media' -or $rel.StartsWith('media/', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $true
    }

    # Local / VCS noise
    if ($rel -match '(^|/)\.git(/|$)' -or
        $rel -match '(^|/)\.github(/|$)' -or
        $rel -match '\.(user|suo|pdb)$' -or
        $rel -ieq 'web.config.backup') {
        return $true
    }

    # Manifest is uploaded separately after the sync pass.
    if ($rel -ieq $ManifestFileName) { return $true }

    return $false
}

function Get-Sha256Hex([string]$filePath) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($filePath)
        try {
            $hash = $sha.ComputeHash($stream)
            return ([System.BitConverter]::ToString($hash) -replace '-', '').ToLowerInvariant()
        }
        finally {
            $stream.Close()
        }
    }
    finally {
        $sha.Dispose()
    }
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

function New-FtpRequest([string]$url, [string]$method) {
    $req = [System.Net.FtpWebRequest]::Create($url)
    $req.Method = $method
    $req.Credentials = $credential
    $req.UseBinary = $true
    $req.UsePassive = $true
    $req.KeepAlive = $false
    $req.EnableSsl = $UseFtps
    return $req
}

function Ensure-RemoteDirectory([string]$remoteDirUrl) {
    $normalized = $remoteDirUrl.TrimEnd('/') + '/'
    if ($createdDirs.Contains($normalized)) { return }

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
            $req = New-FtpRequest $dirUrl ([System.Net.WebRequestMethods+Ftp]::MakeDirectory)
            $resp = $req.GetResponse()
            $resp.Close()
        }
        catch {
            $msg = $_.Exception.Message
            if ($msg -notmatch '550|exists|File unavailable') {
                Write-Warning "Could not ensure remote directory ${dirUrl}: $msg"
            }
        }

        [void]$createdDirs.Add($dirUrl)
    }
}

function Get-RemoteFileSize([string]$remoteFileUrl) {
    try {
        $req = New-FtpRequest $remoteFileUrl ([System.Net.WebRequestMethods+Ftp]::GetFileSize)
        $resp = [System.Net.FtpWebResponse]$req.GetResponse()
        try {
            return [int64]$resp.ContentLength
        }
        finally {
            $resp.Close()
        }
    }
    catch {
        return $null
    }
}

function Download-RemoteTextFile([string]$remoteFileUrl) {
    try {
        $req = New-FtpRequest $remoteFileUrl ([System.Net.WebRequestMethods+Ftp]::DownloadFile)
        $resp = $req.GetResponse()
        try {
            $stream = $resp.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            try {
                return $reader.ReadToEnd()
            }
            finally {
                $reader.Close()
            }
        }
        finally {
            $resp.Close()
        }
    }
    catch {
        return $null
    }
}

function Upload-FileBytes([byte[]]$bytes, [string]$remoteFileUrl) {
    $parent = $remoteFileUrl.Substring(0, $remoteFileUrl.LastIndexOf('/') + 1)
    Ensure-RemoteDirectory $parent

    $req = New-FtpRequest $remoteFileUrl ([System.Net.WebRequestMethods+Ftp]::UploadFile)
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

function Upload-LocalFile([string]$localFile, [string]$remoteFileUrl) {
    $bytes = [System.IO.File]::ReadAllBytes($localFile)
    Upload-FileBytes -bytes $bytes -remoteFileUrl $remoteFileUrl
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
$manifestRemoteUrl = $remoteRoot.TrimEnd('/') + '/' + $ManifestFileName

Write-Host "Deploying published output via $(if ($UseFtps) { 'FTPS' } else { 'FTP' })"
Write-Host "  Host: $FtpHost"
Write-Host "  Port: $FtpPort"
Write-Host "  Path: $($builder.Path)"
Write-Host "  Local: $LocalPath"
Write-Host "  Remote delete: DISABLED"
Write-Host "  Excluded (never overwrite): Web.config, media/, ConnectionStrings.config"
Write-Host "  Sync mode: upload only when remote missing, size changed, or SHA-256 changed"

# Load previous remote checksum manifest (if present).
$remoteManifest = @{}
$manifestJson = Download-RemoteTextFile $manifestRemoteUrl
if ($manifestJson) {
    try {
        $parsed = $manifestJson | ConvertFrom-Json
        if ($null -ne $parsed -and $null -ne $parsed.files) {
            foreach ($prop in $parsed.files.PSObject.Properties) {
                $remoteManifest[$prop.Name] = $prop.Value
            }
        }
        Write-Host "  Remote manifest entries: $($remoteManifest.Count)"
    }
    catch {
        Write-Warning "Could not parse remote deploy manifest; checksum comparisons will treat files as unknown."
        $remoteManifest = @{}
    }
}
else {
    Write-Host '  Remote manifest: not found (first incremental sync will upload unknowns).'
}

$files = @(Get-ChildItem -LiteralPath $LocalPath -Recurse -File)
$uploaded = 0
$skippedExcluded = 0
$skippedUnchanged = 0
$newManifestFiles = [ordered]@{}

foreach ($file in $files) {
    $relative = $file.FullName.Substring($LocalPath.Length).TrimStart('\', '/')
    $relNorm = Normalize-RelPath $relative

    if (Test-IsExcludedFromDeploy $relNorm) {
        $skippedExcluded++
        continue
    }

    $localSize = [int64]$file.Length
    $localHash = Get-Sha256Hex $file.FullName
    $newManifestFiles[$relNorm] = [pscustomobject]@{
        size   = $localSize
        sha256 = $localHash
    }

    $remoteUrl = $remoteRoot.TrimEnd('/') + '/' + $relNorm
    $remoteSize = Get-RemoteFileSize $remoteUrl

    $reason = $null
    if ($null -eq $remoteSize) {
        $reason = 'missing on remote'
    }
    elseif ($remoteSize -ne $localSize) {
        $reason = "size changed (remote=$remoteSize local=$localSize)"
    }
    else {
        $prev = $null
        if ($remoteManifest.ContainsKey($relNorm)) {
            $prev = $remoteManifest[$relNorm]
        }

        $prevHash = $null
        $prevSize = $null
        if ($null -ne $prev) {
            if ($prev.PSObject.Properties['sha256']) { $prevHash = [string]$prev.sha256 }
            if ($prev.PSObject.Properties['size']) { $prevSize = [int64]$prev.size }
        }

        if ([string]::IsNullOrWhiteSpace($prevHash) -or $prevSize -ne $localSize) {
            # Same size on FTP, but no trustworthy checksum record — upload to be safe.
            $reason = 'checksum unknown (no matching manifest entry)'
        }
        elseif (-not $prevHash.Equals($localHash, [System.StringComparison]::OrdinalIgnoreCase)) {
            $reason = 'checksum changed'
        }
    }

    if ($null -eq $reason) {
        Write-Host "Unchanged $relNorm"
        $skippedUnchanged++
        continue
    }

    Write-Host "Uploading $relNorm ($reason)"
    Upload-LocalFile -localFile $file.FullName -remoteFileUrl $remoteUrl
    $uploaded++
}

# Publish updated checksum manifest for the next incremental run.
$manifestObject = [pscustomobject]@{
    version     = 1
    algorithm   = 'SHA256'
    generatedUtc = [DateTime]::UtcNow.ToString('o')
    files       = [pscustomobject]$newManifestFiles
}
$manifestBytes = [System.Text.Encoding]::UTF8.GetBytes(
    ($manifestObject | ConvertTo-Json -Depth 6 -Compress)
)
Write-Host "Uploading $ManifestFileName (checksum index for next deploy)"
Upload-FileBytes -bytes $manifestBytes -remoteFileUrl $manifestRemoteUrl

Write-Host ""
Write-Host "FTPS deployment finished."
Write-Host "  Uploaded: $uploaded"
Write-Host "  Skipped unchanged (size+checksum match): $skippedUnchanged"
Write-Host "  Skipped excluded (Web.config/media/etc.): $skippedExcluded"
Write-Host "  Remote Web.config and media/ were NOT modified."
Write-Host "  Remote files were NOT deleted."

$candidateCount = $uploaded + $skippedUnchanged
if ($candidateCount -eq 0) {
    throw 'No deployable files found after exclusions. Check the publish output.'
}
