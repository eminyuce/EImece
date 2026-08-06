<#
.SYNOPSIS
    Runs SeedDummyData.sql (and optionally CleanupDummyData.sql) against an EImece SQL Server database.

.EXAMPLE
    .\RunSeedDummyData.ps1 -ConnectionString "Server=.;Database=EImece;Trusted_Connection=True;TrustServerCertificate=True;"

.EXAMPLE
    .\RunSeedDummyData.ps1 -Server "." -Database "EImece" -Scale 2

.EXAMPLE
    .\RunSeedDummyData.ps1 -ConnectionString "..." -CleanupOnly
#>
[CmdletBinding(DefaultParameterSetName = 'ConnectionString')]
param(
    [Parameter(ParameterSetName = 'ConnectionString', Mandatory = $true)]
    [string] $ConnectionString,

    [Parameter(ParameterSetName = 'ServerDatabase')]
    [string] $Server = ".",

    [Parameter(ParameterSetName = 'ServerDatabase')]
    [string] $Database = "EImece",

    # Multiplies catalog/order bulk tables only (menus, slides, settings stay small).
    [ValidateScript({ $_ -gt 0 })]
    [double] $Scale = 1.0,

    [switch] $SkipCleanup,

    [switch] $CleanupOnly,

    [int] $CommandTimeoutSeconds = 0
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

if ($PSCmdlet.ParameterSetName -eq 'ServerDatabase') {
    $ConnectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;"
}

function Invoke-SqlFile {
    param(
        [string] $Path,
        [hashtable] $VariableReplacements = @{}
    )

    if (-not (Test-Path $Path)) {
        throw "SQL file not found: $Path"
    }

    $sql = Get-Content -Path $Path -Raw -Encoding UTF8
    foreach ($key in $VariableReplacements.Keys) {
        $sql = $sql -replace $key, [string]$VariableReplacements[$key]
    }

    Write-Host "Executing $Path ..." -ForegroundColor Cyan

    if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
        Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $sql -QueryTimeout $CommandTimeoutSeconds
        return
    }

    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        $tmp = [System.IO.Path]::GetTempFileName() + ".sql"
        Set-Content -Path $tmp -Value $sql -Encoding UTF8
        try {
            sqlcmd -S ($ConnectionString -replace '.*Server=([^;]+).*', '$1') `
                   -d ($ConnectionString -replace '.*Database=([^;]+).*', '$1') `
                   -E -i $tmp -b
            if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed with exit code $LASTEXITCODE" }
        }
        finally {
            Remove-Item $tmp -ErrorAction SilentlyContinue
        }
        return
    }

    # Fallback: .NET SqlClient
    Add-Type -AssemblyName System.Data
    $conn = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $conn.Open()
    try {
        # Split on GO batches if present; this script has none, run as one batch
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sql
        $cmd.CommandTimeout = $CommandTimeoutSeconds
        $reader = $cmd.ExecuteReader()
        do {
            $table = New-Object System.Data.DataTable
            $table.Load($reader)
            if ($table.Rows.Count -gt 0) {
                $table | Format-Table -AutoSize | Out-String | Write-Host
            }
        } while (-not $reader.IsClosed -and $reader.NextResult())
    }
    finally {
        $conn.Close()
    }
}

$cleanupPath = Join-Path $scriptDir "CleanupDummyData.sql"
$seedPath = Join-Path $scriptDir "SeedDummyData.sql"

if ($CleanupOnly) {
    Invoke-SqlFile -Path $cleanupPath
    Write-Host "Cleanup finished." -ForegroundColor Green
    return
}

$scaleLiteral = ([string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0}", $Scale))
$replacements = @{
    'DECLARE @Scale\s+FLOAT\s+=\s+[0-9.]+' = "DECLARE @Scale         FLOAT        = $scaleLiteral"
}

if ($SkipCleanup) {
    $replacements['DECLARE @CleanupFirst\s+BIT\s+=\s+\d+'] = "DECLARE @CleanupFirst  BIT          = 0"
}

Invoke-SqlFile -Path $seedPath -VariableReplacements $replacements
Write-Host "Seed finished. Scale=$Scale (menus/slides/settings stay small; catalog/orders scale)." -ForegroundColor Green
Write-Host "Login: admin@eimece.test (seed credential = 'Test' + '123' + '!')" -ForegroundColor Yellow
