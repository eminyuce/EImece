#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'
$log = 'C:\Users\eminy\source\repos\eminyuce\EImece\EImece\scripts\publish-core-iis82.log'
function Log($m) { "$(Get-Date -Format o) $m" | Tee-Object -FilePath $log -Append }

Remove-Item $log -ErrorAction SilentlyContinue
Log 'START publish Core to Eimece_Core'

$proj = 'C:\Users\eminy\source\repos\eminyuce\EImece\EImece\EImece.Web\EImece.Web.csproj'
$out = 'C:\Users\eminy\source\repos\eminyuce\EImece\publish\Eimece_Core'
$dst = 'C:\inetpub\wwwroot\Eimece_Core'
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"

New-Item -ItemType Directory -Force -Path $out | Out-Null
Push-Location $env:TEMP
try {
  & dotnet publish $proj -c Release -o $out --nologo
  if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed: $LASTEXITCODE" }
} finally { Pop-Location }
Log 'publish ok'

# Ensure local smoke bypass + connection string in published appsettings
$appsettings = Join-Path $out 'appsettings.json'
$json = Get-Content $appsettings -Raw | ConvertFrom-Json
$json.EImece.BypassAdminAuth = $true
$json.ConnectionStrings.EImeceDbConnection = 'Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;'
$json | ConvertTo-Json -Depth 20 | Set-Content $appsettings -Encoding UTF8

& $appcmd stop site /site.name:Eimece_Core 2>&1 | ForEach-Object { Log $_ }
& $appcmd stop apppool /apppool.name:Eimece_Core 2>&1 | ForEach-Object { Log $_ }
Start-Sleep -Seconds 2
Get-Process -Name 'w3wp','EImece.Web','dotnet' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Remove-Item (Join-Path $dst 'EImece.Web.dll') -Force -ErrorAction SilentlyContinue

robocopy $out $dst /MIR /R:5 /W:2 /NFL /NDL /NP | Out-Null
Log "robocopy exit=$LASTEXITCODE"
if (-not (Test-Path (Join-Path $dst 'EImece.Web.dll'))) { throw 'Deploy failed: EImece.Web.dll missing in IIS folder' }
Log "deployed DLL $((Get-Item (Join-Path $dst 'EImece.Web.dll')).LastWriteTime) $((Get-Item (Join-Path $dst 'EImece.Web.dll')).Length)"

# IIS aspNetCore env for Development + bypass (must run AFTER robocopy — MIR overwrites web.config)
$webConfig = Join-Path $dst 'web.config'
if (Test-Path $webConfig) {
  [xml]$xml = Get-Content $webConfig
  $asp = $xml.configuration.location.'system.webServer'.aspNetCore
  if (-not $asp) { $asp = $xml.configuration.'system.webServer'.aspNetCore }
  if ($asp) {
    $asp.SetAttribute('stdoutLogEnabled', 'true')
    $asp.SetAttribute('stdoutLogFile', '.\logs\stdout')
    $envNode = $asp.environmentVariables
    if (-not $envNode) {
      $envNode = $xml.CreateElement('environmentVariables')
      [void]$asp.AppendChild($envNode)
    }
    $envNode.RemoveAll()
    foreach ($pair in @(
      @{n='ASPNETCORE_ENVIRONMENT'; v='Development'},
      @{n='EImece__BypassAdminAuth'; v='true'}
    )) {
      $el = $xml.CreateElement('environmentVariable')
      $el.SetAttribute('name', $pair.n)
      $el.SetAttribute('value', $pair.v)
      [void]$envNode.AppendChild($el)
    }
    $xml.Save($webConfig)
    Log 'web.config env updated after robocopy'
  }
}

# Re-assert appsettings after robocopy
$appsettingsDst = Join-Path $dst 'appsettings.json'
if (Test-Path $appsettingsDst) {
  $j = Get-Content $appsettingsDst -Raw | ConvertFrom-Json
  $j.EImece.BypassAdminAuth = $true
  $j.ConnectionStrings.EImeceDbConnection = 'Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;'
  if (-not $j.Media) { $j | Add-Member -NotePropertyName Media -NotePropertyValue ([pscustomobject]@{}) }
  $j.Media.AbsoluteRootPath = ''
  $j | ConvertTo-Json -Depth 20 | Set-Content $appsettingsDst -Encoding UTF8
  Log 'appsettings BypassAdminAuth reasserted'
}

New-Item -ItemType Directory -Force -Path (Join-Path $dst 'logs') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $dst 'App_Data\DataProtection-Keys') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $dst 'wwwroot\media\images') | Out-Null

# Sync legacy media into Core wwwroot (copy, do not share AbsoluteRootPath — app pool may lack write ACL)
$legacyMedia = 'C:\inetpub\wwwroot\Eimece\media'
if (Test-Path $legacyMedia) {
  robocopy $legacyMedia (Join-Path $dst 'wwwroot\media') /E /R:1 /W:1 /NFL /NDL /NP | Out-Null
  Log "media sync from legacy exit=$LASTEXITCODE"
}

& $appcmd set apppool /apppool.name:Eimece_Core /managedRuntimeVersion: /managedPipelineMode:Integrated 2>&1 | ForEach-Object { Log $_ }
& $appcmd start site /site.name:Eimece_Core 2>&1 | ForEach-Object { Log $_ }
& $appcmd recycle apppool /apppool.name:Eimece_Core 2>&1 | ForEach-Object { Log $_ }

# Ensure :81 bypass remains
$legacyWc = 'C:\inetpub\wwwroot\Eimece\Web.config'
if (Test-Path $legacyWc) {
  $raw = Get-Content $legacyWc -Raw
  $raw2 = $raw -replace 'key="BypassAdminAuth" value="false"','key="BypassAdminAuth" value="true"'
  if ($raw2 -ne $raw) { Set-Content $legacyWc -Value $raw2 -Encoding UTF8; Log 'legacy BypassAdminAuth=true' }
  & $appcmd recycle apppool /apppool.name:Eimece 2>&1 | ForEach-Object { Log $_ }
}

Log 'DONE'
