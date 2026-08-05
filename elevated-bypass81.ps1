$log = "C:\Users\eminy\source\repos\eminyuce\EImece\elevated-bypass81.log"
"START $(Get-Date) whoami=$(whoami)" | Set-Content $log
$wc = "C:\inetpub\wwwroot\Eimece\Web.config"
$raw = Get-Content $wc -Raw
$raw2 = $raw -replace 'key="BypassAdminAuth" value="false"','key="BypassAdminAuth" value="true"'
Set-Content -Path $wc -Value $raw2 -Encoding UTF8
Select-String -Path $wc -Pattern BypassAdminAuth | ForEach-Object { $_.Line } | Add-Content $log
# Recycle all app pools (safe for local)
& "$env:windir\system32\inetsrv\appcmd.exe" list apppool /text:name 2>&1 | ForEach-Object {
  $p = $_.Trim(); if ($p) {
    & "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:$p 2>&1 | Add-Content $log
  }
}
# Also list which site is :81
& "$env:windir\system32\inetsrv\appcmd.exe" list site 2>&1 | Add-Content $log
"DONE" | Add-Content $log
