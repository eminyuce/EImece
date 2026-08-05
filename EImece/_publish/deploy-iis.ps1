$ErrorActionPreference = "Stop"
$src = "c:\Users\eminy\source\repos\eminyuce\EImece\EImece\_publish\Eimece"
$dst = "C:\inetpub\wwwroot\Eimece"
$log = "c:\Users\eminy\source\repos\eminyuce\EImece\EImece\_publish\iis-deploy.log"
function Log($m) { "$(Get-Date -Format o) $m" | Tee-Object -FilePath $log -Append }

try {
  Import-Module WebAdministration
  if (Test-Path "IIS:\AppPools\Eimece") {
    Log "Stopping AppPool Eimece"
    if ((Get-WebAppPoolState -Name Eimece).Value -ne "Stopped") { Stop-WebAppPool -Name Eimece }
    $deadline = (Get-Date).AddSeconds(30)
    while ((Get-WebAppPoolState -Name Eimece).Value -ne "Stopped" -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 500 }
  }
  Get-Website | Where-Object { $_.PhysicalPath -eq $dst -or $_.Name -eq "Eimece" } | ForEach-Object {
    Log "Stopping site $($_.Name)"
    Stop-Website -Name $_.Name -ErrorAction SilentlyContinue
  }

  if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst -Force | Out-Null }
  Log "Robocopy $src -> $dst"
  & robocopy $src $dst /MIR /E /R:2 /W:2 /NFL /NDL /NJH /NJS /nc /ns /np
  Log "Robocopy exit=$LASTEXITCODE"

  $writable = @("$dst\App_Data","$dst\App_Data\logs","$dst\media","$dst\media\images","$dst\media\files","$dst\media\uploads")
  foreach ($p in $writable) {
    if (-not (Test-Path $p)) { New-Item -ItemType Directory -Path $p -Force | Out-Null }
    icacls $p /grant "IIS APPPOOL\Eimece:(OI)(CI)M" /T | Out-Null
    icacls $p /grant "IIS_IUSRS:(OI)(CI)M" /T | Out-Null
  }
  Log "Writable ACLs applied"

  if (Test-Path "IIS:\AppPools\Eimece") { Start-WebAppPool -Name Eimece; Log "Started AppPool Eimece" }
  Get-Website | Where-Object { $_.PhysicalPath -eq $dst -or $_.Name -eq "Eimece" } | ForEach-Object {
    Start-Website -Name $_.Name -ErrorAction SilentlyContinue
    Log "Started site $($_.Name)"
  }
  Log "DONE OK"
  exit 0
} catch {
  Log "FAILED: $_"
  exit 1
}
