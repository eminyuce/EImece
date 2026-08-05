$ErrorActionPreference = "Continue"
$log = "C:\Users\eminy\source\repos\eminyuce\EImece\EImece\scripts\force-redeploy82.log"
"START $(Get-Date)" | Set-Content $log
$src = "C:\Users\eminy\source\repos\eminyuce\EImece\publish\Eimece_Core"
$dst = "C:\inetpub\wwwroot\Eimece_Core"
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"
& $appcmd stop site /site.name:Eimece_Core 2>&1 | Add-Content $log
& $appcmd stop apppool /apppool.name:Eimece_Core 2>&1 | Add-Content $log
Start-Sleep 3
# Kill any lingering process locking the folder
Get-Process w3wp -ErrorAction SilentlyContinue | ForEach-Object {
  try {
    $cmd = (Get-CimInstance Win32_Process -Filter "ProcessId=$($_.Id)").CommandLine
    if ($cmd -match 'Eimece_Core|EImece.Web') { Stop-Process -Id $_.Id -Force; "Killed $($_.Id)" | Add-Content $log }
  } catch { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
}
# Also stop all w3wp briefly for clean unlock
Get-Process w3wp -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep 2
Remove-Item "$dst\EImece.Web.dll" -Force -ErrorAction SilentlyContinue
Remove-Item "$dst\EImece.Domain.Core.dll" -Force -ErrorAction SilentlyContinue
robocopy $src $dst /E /IS /IT /R:5 /W:2 /NFL /NDL /NP | Out-Null
"ROBO=$LASTEXITCODE" | Add-Content $log
"DST DLL: $((Get-Item "$dst\EImece.Web.dll").LastWriteTime) $((Get-Item "$dst\EImece.Web.dll").Length)" | Add-Content $log
cmd /c "findstr /M /C:ReportController `"$dst\EImece.Web.dll`" && echo DST_HAS_REPORT || echo DST_NO_REPORT" | Add-Content $log

# web.config env
$wc = Join-Path $dst "web.config"
if (Test-Path $wc) {
  $raw = Get-Content $wc -Raw
  if ($raw -notmatch 'EImece__BypassAdminAuth') {
    $raw = $raw -replace '</aspNetCore>', "  <environmentVariables>`r`n          <environmentVariable name=`"ASPNETCORE_ENVIRONMENT`" value=`"Development`" />`r`n          <environmentVariable name=`"EImece__BypassAdminAuth`" value=`"true`" />`r`n        </environmentVariables>`r`n      </aspNetCore>"
    Set-Content $wc $raw -Encoding UTF8
  }
}
$jpath = Join-Path $dst "appsettings.json"
$j = Get-Content $jpath -Raw | ConvertFrom-Json
$j.EImece.BypassAdminAuth = $true
$j.ConnectionStrings.EImeceDbConnection = "Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;"
($j | ConvertTo-Json -Depth 20) | Set-Content $jpath -Encoding UTF8
"Bypass=$($j.EImece.BypassAdminAuth)" | Add-Content $log

& $appcmd start apppool /apppool.name:Eimece_Core 2>&1 | Add-Content $log
& $appcmd start site /site.name:Eimece_Core 2>&1 | Add-Content $log
"DONE" | Add-Content $log
