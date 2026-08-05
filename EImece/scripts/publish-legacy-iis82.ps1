#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'
$msb = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
$proj = "C:\Users\eminy\source\repos\eminyuce\EImece\EImece\EImece\EImece.csproj"
$publishDir = "C:\inetpub\wwwroot\Eimece"
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"
$log = "C:\Users\eminy\source\repos\eminyuce\EImece\EImece\scripts\publish-legacy-iis82.log"

function Log($m) { $line = "$(Get-Date -Format o) $m"; Add-Content -Path $log -Value $line; Write-Host $line }

Remove-Item $log -ErrorAction SilentlyContinue
Log "Starting elevated publish"

# Stop sites that may lock files under Eimece
try {
  & $appcmd list site /text:name | ForEach-Object {
    $name = $_.Trim()
    if (-not $name) { return }
    $vdirs = & $appcmd list vdir /app.name:"$name/" /text:physicalPath 2>$null
    $bindings = & $appcmd list site "$name" /text:bindings 2>$null
    Log "SITE $name bindings=$bindings phys=$vdirs"
    if ($bindings -match ':82:' -or ($vdirs -match 'Eimece')) {
      Log "Stopping site $name"
      & $appcmd stop site /site.name:"$name" 2>&1 | ForEach-Object { Log $_ }
    }
  }
} catch {
  Log "Site stop warning: $($_.Exception.Message)"
}

# Also stop app pools referencing the path
try {
  Get-Process w3wp -ErrorAction SilentlyContinue | ForEach-Object {
    Log "Stopping w3wp PID $($_.Id)"
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
  }
} catch {}

Log "MSBuild publish..."
& $msb $proj `
  /p:Configuration=Release `
  /p:DeployOnBuild=true `
  /p:PublishProfile=FolderProfile `
  /p:VisualStudioVersion=17.0 `
  /v:m /nologo 2>&1 | ForEach-Object { Log $_ }

if ($LASTEXITCODE -ne 0) {
  Log "MSBuild failed with $LASTEXITCODE"
  exit $LASTEXITCODE
}

# Force BypassAdminAuth for local smoke testing on :82
$webConfigPath = Join-Path $publishDir 'Web.config'
[xml]$web = Get-Content $webConfigPath
$node = $web.configuration.appSettings.add | Where-Object { $_.key -eq 'BypassAdminAuth' }
if ($node) {
  $node.value = 'true'
  $web.Save($webConfigPath)
  Log "Set BypassAdminAuth=true in published Web.config"
} else {
  Log "WARNING: BypassAdminAuth key not found"
}

# Ensure / create IIS site on port 82 pointing at legacy publish folder
$siteName = 'EImece'
$existing = & $appcmd list site /name:$siteName 2>$null
if (-not $existing) {
  # Find which site owns :82
  $owner = $null
  & $appcmd list site /text:name | ForEach-Object {
    $n = $_.Trim(); if (-not $n) { return }
    $b = & $appcmd list site "$n" /text:bindings
    if ($b -match ':82:') { $owner = $n }
  }
  if ($owner) {
    Log "Rebinding site $owner to $publishDir on :82"
    & $appcmd set vdir "$owner/" /physicalPath:$publishDir 2>&1 | ForEach-Object { Log $_ }
    & $appcmd start site /site.name:"$owner" 2>&1 | ForEach-Object { Log $_ }
    $siteName = $owner
  } else {
    Log "Creating site $siteName"
    & $appcmd add apppool /name:EImeceAppPool /managedRuntimeVersion:v4.0 /managedPipelineMode:Integrated 2>&1 | ForEach-Object { Log $_ }
    & $appcmd add site /name:$siteName /physicalPath:$publishDir /bindings:http/*:82: 2>&1 | ForEach-Object { Log $_ }
    & $appcmd set app "$siteName/" /applicationPool:EImeceAppPool 2>&1 | ForEach-Object { Log $_ }
    & $appcmd start site /site.name:$siteName 2>&1 | ForEach-Object { Log $_ }
  }
} else {
  Log "Updating existing site $siteName"
  & $appcmd set vdir "$siteName/" /physicalPath:$publishDir 2>&1 | ForEach-Object { Log $_ }
  # Ensure binding includes :82
  $b = & $appcmd list site "$siteName" /text:bindings
  if ($b -notmatch ':82:') {
    & $appcmd set site /site.name:$siteName /+bindings.[protocol='http',bindingInformation='*:82:'] 2>&1 | ForEach-Object { Log $_ }
  }
  & $appcmd start site /site.name:$siteName 2>&1 | ForEach-Object { Log $_ }
}

# If another site still owns :82 and isn't our target, stop it and start ours
& $appcmd list site /text:name | ForEach-Object {
  $n = $_.Trim(); if (-not $n) { return }
  $b = & $appcmd list site "$n" /text:bindings
  $p = & $appcmd list vdir /app.name:"$n/" /text:physicalPath
  Log "FINAL SITE $n bindings=$b path=$p"
  if ($b -match ':82:' -and ($p -notmatch 'Eimece$' -and $p -notmatch 'Eimece\\?$')) {
    # Core site may still be on 82 — re-point it OR stop and use legacy site
    Log "Port 82 currently points to $p — retargeting to legacy $publishDir"
    & $appcmd set vdir "$n/" /physicalPath:$publishDir 2>&1 | ForEach-Object { Log $_ }
    # Switch pool to classic .NET 4 for MVC5
    $pool = & $appcmd list app "$n/" /text:applicationPool
    if ($pool) {
      & $appcmd set apppool /apppool.name:$pool /managedRuntimeVersion:v4.0 /managedPipelineMode:Integrated 2>&1 | ForEach-Object { Log $_ }
    }
    & $appcmd start site /site.name:"$n" 2>&1 | ForEach-Object { Log $_ }
  }
}

Log "Done"
exit 0
